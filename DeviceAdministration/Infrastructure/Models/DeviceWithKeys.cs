using Microsoft.Azure.Devices.Applications.RemoteMonitoring.Common.Models;

namespace Microsoft.Azure.Devices.Applications.RemoteMonitoring.DeviceAdmin.Infrastructure.Models
{
    public class DeviceWithKeys
    {
        public dynamic Device { get; set; }
        public SecurityKeys SecurityKeys { get; set; }

        public DeviceWithKeys(dynamic device, SecurityKeys securityKeys)
        {
            Device = device;
            SecurityKeys = securityKeys;
        }
    }
}
