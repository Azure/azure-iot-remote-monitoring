using Microsoft.Azure.Devices.Applications.RemoteMonitoring.Common.Models;

namespace Microsoft.Azure.Devices.Applications.RemoteMonitoring.DeviceAdmin.Infrastructure.Models
{
    public class DeviceWithKeys
    {
        public DeviceModel Device { get; set; }
        public SecurityKeys SecurityKeys { get; set; }

        public DeviceWithKeys(DeviceModel device, SecurityKeys securityKeys)
        {
            Device = device;
            SecurityKeys = securityKeys;
        }
    }
}
