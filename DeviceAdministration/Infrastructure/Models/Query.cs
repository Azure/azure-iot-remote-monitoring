using System.Collections.Generic;

namespace Microsoft.Azure.Devices.Applications.RemoteMonitoring.DeviceAdmin.Infrastructure.Models
{
    public class Query
    {
        public string Name { get; set; }

        public List<FilterInfo> Filters { get; set; }

        public string Sql { get; set; }

        public bool IsTemporary { get; set; }
    }
}
