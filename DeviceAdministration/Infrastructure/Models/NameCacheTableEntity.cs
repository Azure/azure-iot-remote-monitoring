using Microsoft.WindowsAzure.Storage.Table;

namespace Microsoft.Azure.Devices.Applications.RemoteMonitoring.DeviceAdmin.Infrastructure.Models
{
    public class NameCacheTableEntity : TableEntity
    {
        public NameCacheTableEntity(NameCacheEntityType entityType, string name)
        {
            this.PartitionKey = entityType.ToString();
            this.RowKey = name;
        }

        public NameCacheTableEntity() { }

        [IgnoreProperty]
        public string Name
        {
            get { return this.RowKey; }
            set { this.RowKey = value; }
        }

        public string MethodParameters { get; set; }

        public string MethodDescription { get; set; }
    }
}
