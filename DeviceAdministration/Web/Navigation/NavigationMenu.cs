using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using GlobalResources;
using Microsoft.Azure.Devices.Applications.RemoteMonitoring.DeviceAdmin.Web.Helpers;
using Microsoft.Azure.Devices.Applications.RemoteMonitoring.DeviceAdmin.Web.Security;

namespace Microsoft.Azure.Devices.Applications.RemoteMonitoring.DeviceAdmin.Web.Navigation
{
    public class NavigationMenu
    {
        private readonly List<NavigationMenuItem> _navigationMenuItems;

        public NavigationMenu()
        {
            _navigationMenuItems = new List<NavigationMenuItem>()
            {
                new NavigationMenuItem
                {
                    Text = Strings.NavigationMenuItemDashboard,
                    Action = "Index",
                    Controller = "Dashboard",
                    Selected = false,
                    Class = "navigation__link--dashboard",
                    MinimumPermission = Permission.ViewTelemetry,
                },
                new NavigationMenuItem
                {
                    Text = Strings.NavigationMenuItemDevices,
                    Action = "Index",
                    Controller = "Device",
                    Selected = false,
                    Class = "navigation__link--devices",
                    MinimumPermission = Permission.ViewDevices,
                },
                new NavigationMenuItem
                {
                    Text = Strings.Rules,
                    Action = "Index",
                    Controller = "DeviceRules",
                    Selected = false,
                    Class = "navigation__link--rules",
                    MinimumPermission = Permission.ViewRules,
                },
                new NavigationMenuItem
                {
                    Text = Strings.NavigationMenuItemActions,
                    Action = "Index",
                    Controller = "Actions",
                    Selected = false,
                    Class = "navigation__link--actions",
                    MinimumPermission = Permission.ViewActions,
                },
                new NavigationMenuItem
                {
                    Text = Strings.NavigationMenuItemJobs,
                    Action = "Index",
                    Controller = "Job",
                    Selected = false,
                    Class = "navigation__link--jobs",
                    MinimumPermission = Permission.ViewJobs,
                },
                new NavigationMenuItem
                {
                    Text = Strings.NavigationMenuItemsAdvanced,
                    Action = "CellularConn",
                    Controller = "Advanced",
                    Selected = false,
                    Class = "nav_advanced",
                    MinimumPermission = Permission.CellularConn,
                },
            };
        }

        public List<NavigationMenuItem> NavigationMenuItems
        {
            get
            {
                // only show menu items the user has permission for
                var visibleItems = new List<NavigationMenuItem>();
                foreach (var menuItem in _navigationMenuItems)
                {
                    var subNavItems = NavigationHelper.GetSubnavigationItemsForController(menuItem.Controller);

                    if ((subNavItems != null) &&
                        subNavItems.Any(t => PermsChecker.HasPermission(t.MinimumPermission)))
                    {
                        visibleItems.Add(menuItem);
                    }
                    else if (PermsChecker.HasPermission(menuItem.MinimumPermission))
                    {
                        visibleItems.Add(menuItem);
                    }
                }

                return visibleItems;
            }
        }

        public NavigationMenuItem Select(string controllerName, string actionName)
        {
            foreach (var navigationMenuItem in _navigationMenuItems)
            {
                if (navigationMenuItem.Controller == controllerName && navigationMenuItem.Action == actionName)
                {
                    navigationMenuItem.Selected = true;
                    navigationMenuItem.Class = string.Format(CultureInfo.InvariantCulture, "{0} {1}", navigationMenuItem.Class, "selected");
                    return navigationMenuItem;
                }
            }

            return null;
        }
    }
}
