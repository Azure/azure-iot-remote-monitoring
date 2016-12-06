using System;
using Microsoft.WindowsAzure.Storage.Table;
using Microsoft.Azure.Devices.Applications.RemoteMonitoring.Common.Extensions;

namespace Microsoft.Azure.Devices.Applications.RemoteMonitoring.DeviceAdmin.Infrastructure.Models
{
    public class DeviceListFilterTableEntity : TableEntity
    {
        public DeviceListFilterTableEntity(string paritionKey, string rowKey)
        {
            if (paritionKey.IsAllowedTableKey() && rowKey.IsAllowedTableKey())
            {
                this.PartitionKey = paritionKey;
                this.RowKey = rowKey;
                this.Name = rowKey;
            }
            else
            {
                throw new ArgumentException($"Incorrect table keys: {paritionKey}, {rowKey}");
            }
        }

        public DeviceListFilterTableEntity() { }

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
    }
}
