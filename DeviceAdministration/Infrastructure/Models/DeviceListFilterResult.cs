using System.Collections.Generic;
using Microsoft.Azure.Devices.Applications.RemoteMonitoring.Common.Models;

namespace Microsoft.Azure.Devices.Applications.RemoteMonitoring.DeviceAdmin.Infrastructure.Models
{
    public class DeviceListFilterResult
    {
        public int TotalDeviceCount { get; set; }
        public int TotalFilteredCount { get; set; }
        public List<DeviceModel> Results { get; set; }
    }
}
