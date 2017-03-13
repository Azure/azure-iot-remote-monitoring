using System;
using Microsoft.Azure.Devices.Applications.RemoteMonitoring.Common.Extensions;
using Microsoft.WindowsAzure.Storage.Table;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace Microsoft.Azure.Devices.Applications.RemoteMonitoring.DeviceAdmin.Infrastructure.Models
{
    public class DeviceListFilterTableEntity : TableEntity
    {
        public DeviceListFilterTableEntity(string filterId, string filterName)
        {
            if (filterId.IsAllowedTableKey() && filterName.IsAllowedTableKey())
            {
                PartitionKey = Id = filterId;
                RowKey = Name = filterName;
            }
            else
            {
                throw new ArgumentException(FormattableString.Invariant($"Incorrect table keys: {filterId}, {filterName}"));
            }
        }

        public DeviceListFilterTableEntity(DeviceListFilter filter)
        {
            if (filter.Id.IsAllowedTableKey() && filter.Name.IsAllowedTableKey())
            {
                PartitionKey = Id = filter.Id;
                RowKey = Name = filter.Name;
            }
            else
            {
                throw new ArgumentException(FormattableString.Invariant($"Incorrect table keys: {filter.Id}, {filter.Name}"));
            }
            Clauses = JsonConvert.SerializeObject(filter.Clauses, Formatting.None, new StringEnumConverter());
            SortColumn = filter.SortColumn;
            SortOrder = filter.SortOrder.ToString();
            AdvancedClause = filter.AdvancedClause;
            IsAdvanced = filter.IsAdvanced;
            IsTemporary = filter.IsTemporary;
        }

        public DeviceListFilterTableEntity() { }

        public string Id { get; set; }

        public string Name { get; set; }

        public string Clauses { get; set; }

        public string SortColumn { get; set; }

        public string SortOrder { get; set; }

        /// <summary>
        /// The advanced clause string customized by user.
        /// </summary>
        public string AdvancedClause { get; set; }

        /// <summary>
        /// Indicate if this is an advanced clause customized by user.
        /// </summary>
        public bool IsAdvanced { get; set; }

        /// <summary>
        /// Indicate if this is a temporary filter generated automatically.
        /// </summary>
        public bool IsTemporary { get; set; }
    }
}
