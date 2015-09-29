using Microsoft.Azure.Devices.Applications.RemoteMonitoring.DeviceAdmin.Web.Security;

namespace Microsoft.Azure.Devices.Applications.RemoteMonitoring.DeviceAdmin.Web.Models
{
    public class DevicePropertiesModel
    {
        public dynamic DeviceProperties { get; set; }

        public bool IsDeviceEditEnabled
        {
            get
            {
                return PermsChecker.HasPermission(Permission.EditDeviceMetadata);
            }
        }
    }
}