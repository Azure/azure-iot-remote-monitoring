using System.Collections.Generic;
using Microsoft.Azure.Devices.Applications.RemoteMonitoring.DeviceAdmin.Web.Security;

namespace Microsoft.Azure.Devices.Applications.RemoteMonitoring.DeviceAdmin.Web.Navigation
{
    public class NavigationMenuItem
    {
        public string Text { get; set; }
        public string Action { get; set; }
        public string Controller { get; set; }
        public bool Selected { get; set; }
        public string Class { get; set; }
        public List<NavigationMenuItem> Children { get; set; }

        /// <summary>
        /// Most basic permission user would need to display menu item
        /// </summary>
        public Permission MinimumPermission { get; set; }
    }
}