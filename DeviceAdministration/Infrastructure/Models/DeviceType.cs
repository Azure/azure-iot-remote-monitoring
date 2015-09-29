using System;

namespace Microsoft.Azure.Devices.Applications.RemoteMonitoring.DeviceAdmin.Infrastructure.Models
{
    /// <summary>
    /// Represents a type of device that can be selected during the add device wizard
    /// </summary>
    public class DeviceType
    {
        public int DeviceTypeId { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public Uri ImageUrl { get; set; }
        public string InstructionsUrl { get; set; }
        public bool IsSimulatedDevice { get; set; }
    }
}
