using GlobalResources;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web.Mvc;
using Microsoft.Azure.Devices.Applications.RemoteMonitoring.Common.Extensions;
using Microsoft.Azure.Devices.Applications.RemoteMonitoring.DeviceAdmin.Infrastructure.BusinessLogic;
using Microsoft.Azure.Devices.Applications.RemoteMonitoring.DeviceAdmin.Infrastructure.Models;
using Microsoft.Azure.Devices.Applications.RemoteMonitoring.DeviceAdmin.Infrastructure.Repository;
using Microsoft.Azure.Devices.Applications.RemoteMonitoring.DeviceAdmin.Web.Helpers;
using Microsoft.Azure.Devices.Applications.RemoteMonitoring.DeviceAdmin.Web.Models;
using Microsoft.Azure.Devices.Applications.RemoteMonitoring.DeviceAdmin.Web.Security;
using Microsoft.Azure.Devices.Shared;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Microsoft.Azure.Devices.Applications.RemoteMonitoring.DeviceAdmin.Web.Controllers
{
    public class JobController : Controller
    {
        private readonly IJobRepository _jobRepository;
        private readonly IDeviceListFilterRepository _filterRepository;
        private readonly IIoTHubDeviceManager _iotHubDeviceManager;
        private readonly INameCacheLogic _nameCacheLogic;

        public JobController(
            IJobRepository jobRepository,
            IDeviceListFilterRepository filterRepository,
            IIoTHubDeviceManager iotHubDeviceManager,
            INameCacheLogic nameCacheLogic)
        {
            _jobRepository = jobRepository;
            _filterRepository = filterRepository;
            _iotHubDeviceManager = iotHubDeviceManager;
            _nameCacheLogic = nameCacheLogic;
        }

        [RequirePermission(Permission.ViewJobs)]
        public ActionResult Index(string jobId)
        {
            ViewBag.JobId = jobId;
            return View();
        }

        [HttpGet]
        [RequirePermission(Permission.ViewJobs)]
        public async Task<ActionResult> GetJobProperties(string jobId)
        {
            var jobResponse = await _iotHubDeviceManager.GetJobResponseByJobIdAsync(jobId);

            var job = new DeviceJobModel(jobResponse);
            await AddMoreDetailsToJobAsync(job);

            return PartialView("_JobProperties", job);
        }

        [RequirePermission(Permission.ViewJobs)]
        public async Task<ActionResult> ScheduleJob(string filterId)
        {
            if (string.IsNullOrEmpty(filterId))
            {
                filterId = DeviceListFilterRepository.DefaultDeviceListFilter.Id;
            }

            var jobs = await _jobRepository.QueryByFilterIdAsync(filterId);
            var tasks = jobs.Select(async job =>
            {
                JobResponse jobResponse;

                try
                {
                    jobResponse = await _iotHubDeviceManager.GetJobResponseByJobIdAsync(job.JobId);
                }
                catch
                {
                    jobResponse = null;
                }

                return new NamedJobResponseModel
                {
                    Name = job.JobName,
                    Job = jobResponse
                };
            });

            var preScheduleJobModel = new PreScheduleJobModel
            {
                FilterId = filterId,
                JobsSharingQuery = (await Task.WhenAll(tasks))
                    .Where(model => model.Job != null)
                    .OrderByDescending(model => model.Job.CreatedTimeUtc)
            };

            return PartialView("_ScheduleJob", preScheduleJobModel);
        }

        [RequirePermission(Permission.ManageJobs)]
        public async Task<ActionResult> CloneJob(string jobId)
        {
            var jobResponse = await _iotHubDeviceManager.GetJobResponseByJobIdAsync(jobId);
            var job = await _jobRepository.QueryByJobIDAsync(jobId);
            switch (jobResponse.Type)
            {
                case JobType.ScheduleUpdateTwin:
                    var twin = jobResponse.UpdateTwin;
                    return View("ScheduleTwinUpdate", new ScheduleTwinUpdateModel
                    {
                        FilterId = job.FilterId,
                        FilterName = job.FilterName,
                        JobName = job.JobName,
                        OriginalJobId = jobId,
                        DesiredProperties = twin.Properties.Desired.AsEnumerableFlatten().Select(p =>
                        {
                            Newtonsoft.Json.Linq.JTokenType valuetype = p.Value.Value.Type;
                            return new DesiredPropetiesEditViewModel
                            {
                                PropertyName = $"desired.{p.Key}",
                                PropertyValue = p.Value.Value.ToString(),
                                DataType = convertToTwinDataType(valuetype)
                            };
                        }).ToList(),
                        Tags = twin.Tags.AsEnumerableFlatten().Select(t =>
                        {
                            Newtonsoft.Json.Linq.JTokenType valuetype = t.Value.Value.Type;
                            return new TagsEditViewModel
                            {
                                TagName = $"tags.{t.Key}",
                                TagValue = t.Value.Value.ToString(),
                                DataType = convertToTwinDataType(valuetype)
                            };
                        }).ToList(),
                    });
                case JobType.ScheduleDeviceMethod:
                    var parameters = JsonConvert.DeserializeObject<Dictionary<string, string>>(jobResponse.CloudToDeviceMethod.GetPayloadAsJson());
                    return View("ScheduleDeviceMethod", new ScheduleDeviceMethodModel
                    {
                        FilterId = job.FilterId,
                        FilterName = job.FilterName,
                        JobName = job.JobName,
                        OriginalJobId = jobId,
                        MethodName = job.MethodName,
                        Parameters = parameters.Select(pair => new MethodParameterEditViewModel
                        {
                            ParameterName = pair.Key,
                            ParameterValue = pair.Value,
                        }).ToList(),
                        MaxExecutionTimeInMinutes = (int)jobResponse.MaxExecutionTimeInSeconds / 60,
                    });
                default:
                    return await ScheduleJob(job.FilterId);
            }
        }

        [RequirePermission(Permission.ManageJobs)]
        public async Task<ActionResult> ScheduleIconUpdate(string filterId)
        {
            var deviceListFilter = await GetFilterById(filterId);
            return View(new ScheduleTwinUpdateModel
            {
                FilterId = filterId,
                FilterName = deviceListFilter.Name,
                JobName = DateTime.Now.ToString(Strings.NewScheduleJobNameFormat)
            });
        }

        [RequirePermission(Permission.ManageJobs)]
        public async Task<ActionResult> ScheduleTwinUpdate(string filterId)
        {
            var deviceListFilter = await GetFilterById(filterId);
            return View(new ScheduleTwinUpdateModel
            {
                FilterId = filterId,
                FilterName = deviceListFilter.Name,
                JobName = DateTime.Now.ToString(Strings.NewScheduleJobNameFormat)
            });
        }

        [HttpPost]
        [RequirePermission(Permission.ManageJobs)]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> ScheduleTwinUpdate(ScheduleTwinUpdateModel model)
        {
            var twin = new Twin();
            foreach (var tagModel in model.Tags.Where(m => !string.IsNullOrWhiteSpace(m.TagName)))
            {
                string key = tagModel.TagName;
                if (tagModel.isDeleted)
                {
                    twin.Set(key, null);
                }
                else
                {
                    TwinExtension.Set(twin, key, getDyanmicValue(tagModel.DataType, tagModel.TagValue));
                }
                await _nameCacheLogic.AddNameAsync(tagModel.TagName);
            }

            foreach (var propertyModel in model.DesiredProperties.Where(m => !string.IsNullOrWhiteSpace(m.PropertyName)))
            {
                string key = propertyModel.PropertyName;
                if (propertyModel.isDeleted)
                {
                    twin.Set(key, null);
                }
                else
                {
                    TwinExtension.Set(twin, key, getDyanmicValue(propertyModel.DataType, propertyModel.PropertyValue));
                }
                await _nameCacheLogic.AddNameAsync(propertyModel.PropertyName);
            }
            twin.ETag = "*";

            var deviceListFilter = await GetFilterById(model.FilterId);
            string queryCondition = deviceListFilter.GetSQLCondition();

            // The query condition can not be empty when schduling job, use a default clause
            // is_defined(deviceId) to represent no clause condition for all devices.
            var jobId = await _iotHubDeviceManager.ScheduleTwinUpdate(
                string.IsNullOrWhiteSpace(queryCondition) ? "is_defined(deviceId)" : queryCondition,
                twin,
                model.StartDateUtc,
                (int)(model.MaxExecutionTimeInMinutes * 60));

            await _jobRepository.AddAsync(new JobRepositoryModel(jobId, model.FilterId, model.JobName, deviceListFilter.Name, ExtendJobType.ScheduleUpdateTwin, null));

            return RedirectToAction("Index", "Job", new { jobId = jobId });
        }

        [HttpPost]
        [RequirePermission(Permission.ManageJobs)]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> ScheduleIconUpdate(ScheduleTwinUpdateModel model)
        {
            var twin = new Twin() { ETag = "*" };
            twin.Set(model.Tags[0].TagName, model.Tags[0].isDeleted ? null : model.Tags[0].TagValue);
            await _nameCacheLogic.AddNameAsync(model.Tags[0].TagName);

            var deviceListFilter = await GetFilterById(model.FilterId);
            string queryCondition = deviceListFilter.GetSQLCondition();

            // The query condition can not be empty when schduling job, use a default clause
            // is_defined(deviceId) to represent no clause condition for all devices.
            var jobId = await _iotHubDeviceManager.ScheduleTwinUpdate(
                string.IsNullOrWhiteSpace(queryCondition) ? "is_defined(deviceId)" : queryCondition,
                twin,
                model.StartDateUtc,
                Convert.ToInt64(model.MaxExecutionTimeInMinutes * 60));

            await _jobRepository.AddAsync(new JobRepositoryModel(jobId,
                model.FilterId,
                model.JobName,
                deviceListFilter.Name,
                model.Tags[0].isDeleted ? ExtendJobType.ScheduleRemoveIcon : ExtendJobType.ScheduleUpdateIcon,
                null));

            return RedirectToAction("Index", "Job", new { jobId = jobId });
        }

        [RequirePermission(Permission.ManageJobs)]
        public async Task<ActionResult> ScheduleDeviceMethod(string filterId)
        {
            var deviceListFilter = await GetFilterById(filterId);
            return View(new ScheduleDeviceMethodModel
            {
                FilterId = filterId,
                FilterName = deviceListFilter.Name,
                JobName = DateTime.Now.ToString(Strings.NewScheduleJobNameFormat)
            });
        }

        [HttpPost]
        [RequirePermission(Permission.ManageJobs)]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> ScheduleDeviceMethod(ScheduleDeviceMethodModel model)
        {
            string methodName = model.MethodName.Split('(').First();

            var parameters = model.Parameters?.ToDictionary(p => p.ParameterName, p => getDyanmicValue(p.Type, p.ParameterValue)) ?? new Dictionary<string, dynamic>();
            string payload = JsonConvert.SerializeObject(parameters);

            var deviceListFilter = await GetFilterById(model.FilterId);
            string queryCondition = deviceListFilter.GetSQLCondition();

            // The query condition can not be empty when schduling job, use a default clause
            // is_defined(deviceId) to represent no clause condition for all devices.
            var jobId = await _iotHubDeviceManager.ScheduleDeviceMethod(
                string.IsNullOrWhiteSpace(queryCondition) ? "is_defined(deviceId)" : queryCondition,
                methodName,
                payload,
                model.StartDateUtc,
                Convert.ToInt64(model.MaxExecutionTimeInMinutes * 60));

            await _jobRepository.AddAsync(new JobRepositoryModel(jobId, model.FilterId, model.JobName, deviceListFilter.Name, ExtendJobType.ScheduleDeviceMethod, model.MethodName));

            return RedirectToAction("Index", "Job", new { jobId = jobId });
        }

        private dynamic getDyanmicValue(TwinDataType type, dynamic value)
        {
            switch (type)
            {
                case Infrastructure.Models.TwinDataType.String:
                    string valueString = value.ToString();
                    return valueString as dynamic;
                case Infrastructure.Models.TwinDataType.Number:
                    int valueInt;
                    float valuefloat;
                    if (int.TryParse(value.ToString(), out valueInt))
                    {
                        return valueInt as dynamic;
                    }
                    else if (float.TryParse(value.ToString(), out valuefloat))
                    {
                        return valuefloat as dynamic;
                    }
                    else
                    {
                        return value as string;
                    }
                case Infrastructure.Models.TwinDataType.Boolean:
                    bool valueBool;
                    if (bool.TryParse(value.ToString(), out valueBool))
                    {
                        return valueBool as dynamic;
                    }
                    else
                    {
                        return value as string;
                    }
                default: return value as string;
            }
        }

        private TwinDataType convertToTwinDataType(JTokenType valuetype)
        {
            return valuetype.HasFlag(Newtonsoft.Json.Linq.JTokenType.Float & Newtonsoft.Json.Linq.JTokenType.Integer) ? TwinDataType.Number
                    : valuetype.HasFlag(Newtonsoft.Json.Linq.JTokenType.Boolean) ? TwinDataType.Boolean
                    : TwinDataType.String;
        }

        private async Task AddMoreDetailsToJobAsync(DeviceJobModel job)
        {
            Task<JobRepositoryModel> queryJobTask = _jobRepository.QueryByJobIDAsync(job.JobId);
            await DeviceJobHelper.AddMoreDetailsToJobAsync(job, queryJobTask);
        }

        private async Task<DeviceListFilter> GetFilterById(string filterId)
        {
            if (filterId == "*" || filterId == DeviceListFilterRepository.DefaultDeviceListFilter.Id)
            {
                //[WORKAROUND] No condition available for "All Devices"
                return DeviceListFilterRepository.DefaultDeviceListFilter;
            }
            else
            {
                return await _filterRepository.GetFilterAsync(filterId);
            }
        }
    }
}
