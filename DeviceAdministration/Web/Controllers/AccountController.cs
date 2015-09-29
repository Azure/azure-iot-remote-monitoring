using System.Web;
using System.Web.Mvc;

namespace Microsoft.Azure.Devices.Applications.RemoteMonitoring.DeviceAdmin.Web.Controllers
{
    [OutputCache(CacheProfile = "NoCacheProfile")]
    public class AccountController : Controller
    {
        public ActionResult SignIn()
        {
            if (User.Identity.IsAuthenticated)
            {
                return RedirectToAction("Index", "Dashboard");
            }
            else
            {
                return View();
            }
        }

        // GET: SignOut
        public ActionResult SignOut()
        {
            HttpContext.GetOwinContext().Authentication.SignOut();
            return RedirectToAction("Index", "Dashboard");
        }
    }
}