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
        public ActionResult Index()
        {
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

        [RequirePermission(Permission.ManageJobs)]
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
        public async Task<ActionResult> ScheduleTwinUpdate(string filterId)
        {
            var deviceListFilter = await GetFilterById(filterId);
            return View(new ScheduleTwinUpdateModel
            {
                FilterId = filterId,
                FilterName = deviceListFilter.Name,
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

            var jobId = await _iotHubDeviceManager.ScheduleTwinUpdate(queryCondition,
                twin,
                model.StartDateUtc,
                model.MaxExecutionTimeInMinutes * 60);

            await _jobRepository.AddAsync(new JobRepositoryModel(jobId, model.FilterId, model.JobName, deviceListFilter.Name));

            return Redirect("/Job/Index");
        }

        [RequirePermission(Permission.ManageJobs)]
        public async Task<ActionResult> ScheduleDeviceMethod(string filterId)
        {
            var deviceListFilter = await GetFilterById(filterId);
            return View(new ScheduleDeviceMethodModel
            {
                FilterId = filterId,
                FilterName = deviceListFilter.Name,
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

            var jobId = await _iotHubDeviceManager.ScheduleDeviceMethod(queryCondition, methodName, payload, model.StartDateUtc, model.MaxExecutionTimeInMinutes * 60);

            await _jobRepository.AddAsync(new JobRepositoryModel(jobId, model.FilterId, model.JobName, deviceListFilter.Name));

            return Redirect("/Job/Index");
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
                return Tuple.Create(job.JobId, string.Empty, job.QueryCondition ?? Strings.NotApplicableValue);
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
