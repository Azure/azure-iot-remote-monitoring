using System.Web.Mvc;
using Microsoft.Azure.Devices.Applications.RemoteMonitoring.DeviceAdmin.Web.Navigation;

namespace Microsoft.Azure.Devices.Applications.RemoteMonitoring.DeviceAdmin.Web.Controllers
{

    public class NavigationController : Controller
    {
        public ActionResult NavigationMenu()
        {
            var navigationMenu = new NavigationMenu();

            string action = ControllerContext.ParentActionViewContext.RouteData.Values["action"].ToString();
            string controller = ControllerContext.ParentActionViewContext.RouteData.Values["controller"].ToString();

            navigationMenu.Select(controller, action);

            return PartialView("_NavigationMenu", navigationMenu.NavigationMenuItems);
        }
    }
}