using System;

namespace Microsoft.Azure.Devices.Applications.RemoteMonitoring.DeviceAdmin.Infrastructure.Models
{
    public class DeviceJobHistoryModel
    {
        public string Name { get; set; }

        public string Status { get; set; }

        public DateTime LastUpdatedUtc { get; set; }
    }
}
