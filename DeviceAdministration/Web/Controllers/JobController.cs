using System.Threading.Tasks;
using System.Web.Mvc;
using Microsoft.Azure.Devices.Applications.RemoteMonitoring.DeviceAdmin.Web.Security;

namespace Microsoft.Azure.Devices.Applications.RemoteMonitoring.DeviceAdmin.Web.Controllers
{
    public class JobController : Controller
    {
        [RequirePermission(Permission.ManageJobs)]
        public async Task<ActionResult> ScheduleJob()
        {
            //ToDo: create model

            return PartialView("_ScheduleJob");
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
