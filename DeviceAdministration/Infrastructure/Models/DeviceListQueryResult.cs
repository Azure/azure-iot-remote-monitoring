using System.Collections.Generic;

namespace Microsoft.Azure.Devices.Applications.RemoteMonitoring.DeviceAdmin.Infrastructure.Models
{
    public class DeviceListQueryResult
    {
        public int TotalDeviceCount { get; set; }
        public int TotalFilteredCount { get; set; }
        public List<dynamic> Results { get; set; }
    }
}
