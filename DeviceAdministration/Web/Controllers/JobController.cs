using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web.Mvc;
using GlobalResources;
using Microsoft.Azure.Devices.Applications.RemoteMonitoring.Common.Extensions;
using Microsoft.Azure.Devices.Applications.RemoteMonitoring.DeviceAdmin.Infrastructure.BusinessLogic;
using Microsoft.Azure.Devices.Applications.RemoteMonitoring.DeviceAdmin.Infrastructure.Models;
using Microsoft.Azure.Devices.Applications.RemoteMonitoring.DeviceAdmin.Infrastructure.Repository;
using Microsoft.Azure.Devices.Applications.RemoteMonitoring.DeviceAdmin.Web.Models;
using Microsoft.Azure.Devices.Applications.RemoteMonitoring.DeviceAdmin.Web.Security;
using Microsoft.Azure.Devices.Shared;
using Newtonsoft.Json;

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

            var result = new DeviceJobModel(jobResponse);
            var t = await GetJobNameAndFilterNameAsync(result);
            result.JobName = t.Item1;
            result.FilterId = t.Item2;
            result.FilterName = t.Item3;

            return PartialView("_JobProperties", result);
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
                            return new DesiredPropetiesEditViewModel
                            {
                                PropertyName = p.Key,
                                PropertyValue = p.Value.Value.ToString(),
                            };
                        }).ToList(),
                        Tags = twin.Tags.AsEnumerableFlatten().Select(t =>
                        {
                            return new TagsEditViewModel
                            {
                                TagName = t.Key,
                                TagValue = t.Value.Value.ToString(),
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

        [RequirePermission(Permission.ManageJobs)]
        [HttpPost]
        public async Task<ActionResult> ScheduleTwinUpdate(ScheduleTwinUpdateModel model)
        {
            var twin = new Twin();
            foreach (var tagModel in model.Tags.Where(m => !string.IsNullOrWhiteSpace(m.TagName)))
            {
                twin.Set(tagModel.TagName, tagModel.isDeleted ? null : tagModel.TagValue);
                await _nameCacheLogic.AddNameAsync(tagModel.TagName);
            }

            foreach (var propertyModel in model.DesiredProperties.Where(m => !string.IsNullOrWhiteSpace(m.PropertyName)))
            {
                twin.Set(propertyModel.PropertyName, propertyModel.isDeleted ? null : propertyModel.PropertyValue);
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

            await _jobRepository.AddAsync(new JobRepositoryModel(jobId, model.FilterId, model.JobName, deviceListFilter.Name, null));

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

        [RequirePermission(Permission.ManageJobs)]
        [HttpPost]
        public async Task<ActionResult> ScheduleDeviceMethod(ScheduleDeviceMethodModel model)
        {
            string methodName = model.MethodName.Split('(').First();

            var parameters = model.Parameters?.ToDictionary(p => p.ParameterName, p => p.ParameterValue) ?? new Dictionary<string, string>();
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
                (int)(model.MaxExecutionTimeInMinutes * 60));

            await _jobRepository.AddAsync(new JobRepositoryModel(jobId, model.FilterId, model.JobName, deviceListFilter.Name, model.MethodName));

            return RedirectToAction("Index", "Job", new { jobId = jobId });
        }

        private async Task<Tuple<string, string, string>> GetJobNameAndFilterNameAsync(DeviceJobModel job)
        {
            try
            {
                var model = await _jobRepository.QueryByJobIDAsync(job.JobId);
                string filterId = model.FilterId;
                string filterName = model.FilterName;
                if (string.IsNullOrEmpty(filterName))
                {
                    filterName = job.QueryCondition ?? Strings.NotApplicableValue;
                }
                if (filterName == "*" || DeviceListFilterRepository.DefaultDeviceListFilter.Id.Equals(filterId))
                {
                    filterName = Strings.AllDevices;
                }
                return Tuple.Create(model.JobName ?? Strings.NotApplicableValue, filterId, filterName);
            }
            catch
            {
                string externalJobName = string.Format(Strings.ExternalJobNamePrefix, job.JobId);
                return Tuple.Create(externalJobName, string.Empty, job.QueryCondition ?? Strings.NotApplicableValue);
            }
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
