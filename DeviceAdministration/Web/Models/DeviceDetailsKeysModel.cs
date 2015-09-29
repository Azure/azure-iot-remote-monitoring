using Microsoft.Azure.Devices.Applications.RemoteMonitoring.DeviceAdmin.Web.Security;

namespace Microsoft.Azure.Devices.Applications.RemoteMonitoring.DeviceAdmin.Web.Models
{
    public class DeviceDetailsKeysModel
    {
        public string DeviceId { get; set; }

        public bool IsAllowedToViewKeys
        {
            get
            {
                return PermsChecker.HasPermission(Permission.ViewDeviceSecurityKeys);
            }
        }
    }
}