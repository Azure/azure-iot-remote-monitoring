using System;
using Microsoft.WindowsAzure.Storage.Table;
using Microsoft.Azure.Devices.Applications.RemoteMonitoring.Common.Extensions;

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
                throw new ArgumentException($"Incorrect table keys: {filterId}, {filterName}");
            }
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
        public bool IsTemporary { get; internal set; }
    }
}
