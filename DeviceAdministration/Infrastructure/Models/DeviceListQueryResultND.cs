using System.Collections.Generic;
using Microsoft.Azure.Devices.Applications.RemoteMonitoring.Common.Models;

namespace Microsoft.Azure.Devices.Applications.RemoteMonitoring.DeviceAdmin.Infrastructure.Models
{
    public class DeviceListQueryResultND
    {
        public int TotalDeviceCount { get; set; }
        public int TotalFilteredCount { get; set; }
        public List<DeviceND> Results { get; set; }
    }
}
