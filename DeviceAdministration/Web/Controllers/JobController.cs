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

        [RequirePermission(Permission.ManageJobs)]
        public async Task<ActionResult> ScheduleJob(string queryName)
        {
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

            var scheduleJobModel = new ScheduleJobModel
            {
                QueryName = queryName,
                JobsSharingQuery = (await Task.WhenAll(tasks)).Where(model => model.Job != null)
            };

            return PartialView("_ScheduleJob", scheduleJobModel);
        }

        [RequirePermission(Permission.ManageJobs)]
        public async Task<ActionResult> ScheduleTwinUpdate()
        {
            //ToDo: Jump to the view with desired model

            return View();
        }

        [RequirePermission(Permission.ManageJobs)]
        public async Task<ActionResult> ScheduleDeviceMethod()
        {
            //ToDo: Jump to the view with desired model

            return View();
        }
    }
}
