using Microsoft.Azure.Devices.Applications.RemoteMonitoring.Common.Models;
using System.Collections.Generic;

namespace Microsoft.Azure.Devices.Applications.RemoteMonitoring.DeviceAdmin.Infrastructure.Models
{
    public class NameCacheEntity
    {
        public string Name { get; set; }
        public List<Parameter> Parameters { get; set; }
        public string Description { get; set; }
    }
}
