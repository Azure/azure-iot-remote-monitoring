using System;
using Microsoft.WindowsAzure.Storage.Table;
using Microsoft.Azure.Devices.Applications.RemoteMonitoring.Common.Extensions;

namespace Microsoft.Azure.Devices.Applications.RemoteMonitoring.DeviceAdmin.Infrastructure.Models
{
    public class DeviceListColumnsTableEntity : TableEntity
    {
        public DeviceListColumnsTableEntity(string userId)
        {
            if (userId.IsAllowedTableKey())
            {
                this.RowKey = userId;
            }
            else
            {
                throw new ArgumentException("Incorrect name as table key: {0}", userId);
            }
        }

        public DeviceListColumnsTableEntity() { }

        public string Columns { get; set; }
    }
}
