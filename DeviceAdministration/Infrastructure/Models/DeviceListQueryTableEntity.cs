using System;
using Microsoft.WindowsAzure.Storage.Table;
using Microsoft.Azure.Devices.Applications.RemoteMonitoring.Common.Extensions;

namespace Microsoft.Azure.Devices.Applications.RemoteMonitoring.DeviceAdmin.Infrastructure.Models
{
    public class DeviceListQueryTableEntity : TableEntity
    {
        public DeviceListQueryTableEntity(string paritionKey, string rowKey)
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

        public DeviceListQueryTableEntity() { }

        public string Name { get; set; }

        public string Filters { get; set; }

        public string SortColumn { get; set; }

        public string SortOrder { get; set; }

        /// <summary>
        /// The complete SQL query string built from other fields.
        /// </summary>
        public string Sql { get; set; }

        /// <summary>
        /// Indicate if this is an advanced customized by user.
        /// </summary>
        public bool IsAdvanced { get; set; }
    }
}
