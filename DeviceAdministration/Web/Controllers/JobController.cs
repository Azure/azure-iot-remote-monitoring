using System;
using System.Linq;
using System.Threading.Tasks;
using System.Web.Mvc;
using GlobalResources;
using Microsoft.Azure.Devices.Applications.RemoteMonitoring.Common.Extensions;
using Microsoft.Azure.Devices.Applications.RemoteMonitoring.DeviceAdmin.Infrastructure.Models;
using Microsoft.Azure.Devices.Applications.RemoteMonitoring.DeviceAdmin.Infrastructure.Repository;
using Microsoft.Azure.Devices.Applications.RemoteMonitoring.DeviceAdmin.Web.Models;
using Microsoft.Azure.Devices.Applications.RemoteMonitoring.DeviceAdmin.Web.Security;

namespace Microsoft.Azure.Devices.Applications.RemoteMonitoring.DeviceAdmin.Web.Controllers
{
    public class JobController : Controller
    {
        private readonly IJobRepository _jobRepository;
        private readonly IDeviceListQueryRepository _queryRepository;
        private readonly IIoTHubDeviceManager _iotHubDeviceManager;

        public JobController(
            IJobRepository jobRepository,
            IDeviceListQueryRepository queryRepository,
            IIoTHubDeviceManager iotHubDeviceManager)
        {
            _jobRepository = jobRepository;
            _queryRepository = queryRepository;
            _iotHubDeviceManager = iotHubDeviceManager;
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
            var t = await GetJobNameAndQueryNameAsync(result);
            result.JobName = t.Item1;
            result.QueryName = t.Item2;

            return PartialView("_JobProperties", result);
        }

        [RequirePermission(Permission.ManageJobs)]
        public async Task<ActionResult> ScheduleJob(string queryName)
        {
            // [WORKAROUND] The default query name for case it is empty
            if (string.IsNullOrEmpty(queryName))
            {
                queryName = "*";
            }

            var jobs = await _jobRepository.QueryByQueryNameAsync(queryName);
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
                QueryName = queryName,
                JobsSharingQuery = (await Task.WhenAll(tasks)).Where(model => model.Job != null)
            };

            return PartialView("_ScheduleJob", preScheduleJobModel);
        }

        [RequirePermission(Permission.ManageJobs)]
        public ActionResult ScheduleTwinUpdate(string queryName)
        {
            //ToDo: Jump to the view with desired model

            return View(new ScheduleTwinUpdateModel
            {
                QueryName = queryName
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
            }

            foreach (var propertyModel in model.DesiredProperties.Where(m => !string.IsNullOrWhiteSpace(m.PropertyName)))
            {
                twin.Set(propertyModel.PropertyName, propertyModel.isDeleted ? null : propertyModel.PropertyName);
            }

            twin.ETag = "*";

            string queryCondition;
            if (model.QueryName == "*")
            {
                //[WORKAROUND] No condition available for "All Devices"
                queryCondition = "tags.HubEnabledState='Running'";
            }
            else
            {
                var query = await _queryRepository.GetQueryAsync(model.QueryName);
                queryCondition = query.GetSQLCondition();
            }

            var jobId = await _iotHubDeviceManager.ScheduleTwinUpdate(queryCondition,
                twin,
                model.StartDateUtc,
                model.MaxExecutionTimeInMinutes * 60);

            await _jobRepository.AddAsync(new JobRepositoryModel(jobId, model.QueryName, model.JobName));

            return Redirect("/Job/Index");
        }

        [RequirePermission(Permission.ManageJobs)]
        public ActionResult ScheduleDeviceMethod(string queryName)
        {
            //ToDo: Jump to the view with desired model

            return View(new ScheduleDeviceMethodModel
            {
                QueryName = queryName
            });
        }

        [RequirePermission(Permission.ManageJobs)]
        [HttpPost]
        public ActionResult ScheduleDeviceMethod(ScheduleDeviceMethodModel model)
        {
            //ToDo: Jump to the view with desired model

            return View(new ScheduleDeviceMethodModel());
        }

        private async Task<Tuple<string, string>> GetJobNameAndQueryNameAsync(DeviceJobModel job)
        {
            try
            {
                var model = await _jobRepository.QueryByJobIDAsync(job.JobId);
                return Tuple.Create(model.JobName ?? Strings.NotApplicableValue, model.QueryName ?? job.QueryCondition);
            }
            catch
            {
                return Tuple.Create(job.JobId, job.QueryCondition ?? Strings.NotApplicableValue);
            }
        }
    }
}
