using System.Web.Mvc;
using Microsoft.Azure.Devices.Applications.RemoteMonitoring.DeviceAdmin.Web.Helpers;
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

            NavigationHelper.ApplySelection(navigationMenu.NavigationMenuItems, controller, action);

            return PartialView("_NavigationMenu", navigationMenu.NavigationMenuItems);
        }

        public ActionResult NavigationSubmenu()
        {
            string action = ControllerContext.ParentActionViewContext.RouteData.Values["action"].ToString();
            string controller = ControllerContext.ParentActionViewContext.RouteData.Values["controller"].ToString();

            var menuItems = NavigationHelper.GetSubnavigationItemsForController(controller);
            if ((menuItems == null) || (menuItems.Count == 0))
            {
                return new EmptyResult();
            }

            NavigationHelper.ApplySubmenuSelection(menuItems, controller, action);

            return PartialView("_NavigationSubmenu", menuItems);
        }
    }
}