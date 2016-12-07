using System.Collections.Generic;

namespace Microsoft.Azure.Devices.Applications.RemoteMonitoring.DeviceAdmin.Infrastructure.Models
{
    public class Filter
    {
        public string Id { get; set; }

        public string Name { get; set; }

        public List<Clause> Clauses { get; set; }

        public string AdvancedClause { get; set; }

        public bool IsTemporary { get; set; }

        public bool IsAdvanced { get; set; }
    }
}
