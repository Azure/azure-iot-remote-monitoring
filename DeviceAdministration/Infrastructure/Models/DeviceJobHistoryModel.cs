using System;

namespace Microsoft.Azure.Devices.Applications.RemoteMonitoring.DeviceAdmin.Infrastructure.Models
{
    public class DeviceJobHistoryModel
    {
        public string JobID { get; set; }

        public string JobName { get; set; }

        public string JobStatus { get; set; }

        public DateTime JobLastUpdatedTimeUtc { get; set; }
    }
}
