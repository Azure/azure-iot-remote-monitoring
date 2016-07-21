using Microsoft.Azure.Devices.Applications.RemoteMonitoring.Common.Models;

namespace Microsoft.Azure.Devices.Applications.RemoteMonitoring.DeviceAdmin.Infrastructure.Models
{
    public class DeviceWithKeysND
    {
        public DeviceND Device { get; set; }
        public SecurityKeys SecurityKeys { get; set; }

        public DeviceWithKeysND(DeviceND device, SecurityKeys securityKeys)
        {
            Device = device;
            SecurityKeys = securityKeys;
        }
    }
}
