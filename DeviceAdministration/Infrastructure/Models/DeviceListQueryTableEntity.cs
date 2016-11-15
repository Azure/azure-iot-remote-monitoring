using System;
using Microsoft.WindowsAzure.Storage.Table;
using Microsoft.Azure.Devices.Applications.RemoteMonitoring.Common.Extensions;

namespace Microsoft.Azure.Devices.Applications.RemoteMonitoring.DeviceAdmin.Infrastructure.Models
{
    public class DeviceListQueryTableEntity : TableEntity
    {
        public DeviceListQueryTableEntity(string name)
        {
            if (name.IsAllowedTableKey())
            {
                this.RowKey = name;
                this.Name = name;
            }
            else
            {
                throw new ArgumentException("Incorrect name as table key: {0}", name);
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
    }
}
