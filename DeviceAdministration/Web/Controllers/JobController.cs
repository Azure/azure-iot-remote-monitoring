using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web.Mvc;
using Microsoft.Azure.Devices.Applications.RemoteMonitoring.DeviceAdmin.Infrastructure.Repository;
using Microsoft.Azure.Devices.Applications.RemoteMonitoring.DeviceAdmin.Web.Models;
using Microsoft.Azure.Devices.Applications.RemoteMonitoring.DeviceAdmin.Web.Security;


namespace Microsoft.Azure.Devices.Applications.RemoteMonitoring.DeviceAdmin.Web.Controllers
{
    public class JobController : Controller
    {
        private readonly IJobRepository _jobRepository;
        private readonly IIoTHubDeviceManager _iotHubDeviceManager;

        public JobController(IJobRepository jobRepository, IIoTHubDeviceManager iotHubDeviceManager)
        {
            _jobRepository = jobRepository;
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
            return PartialView("_JobProperties", new DeviceJobModel(jobResponse));
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
        public ActionResult ScheduleTwinUpdate(ScheduleTwinUpdateModel model)
        {
            //ToDo: Jump to the view with desired model

            return View(new ScheduleTwinUpdateModel());
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
    }
}
