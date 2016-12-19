using System.Collections.Generic;

namespace Microsoft.Azure.Devices.Applications.RemoteMonitoring.DeviceAdmin.Infrastructure.Models
{
    public class Filter
    {
        public Filter(DeviceListFilter filter)
        {
            Id = filter.Id;
            Name = filter.Name;
            Clauses = filter.Clauses;
            AdvancedClause = filter.AdvancedClause;
            IsAdvanced = filter.IsAdvanced;
            IsTemporary = filter.IsTemporary;
        }

        public Filter() { }

        public string Id { get; set; }

        public string Name { get; set; }

        public List<Clause> Clauses { get; set; }

        public string AdvancedClause { get; set; }

        public bool IsTemporary { get; set; }

        public bool IsAdvanced { get; set; }

        public int AssociatedJobsCount { get; set; }
    }
}
