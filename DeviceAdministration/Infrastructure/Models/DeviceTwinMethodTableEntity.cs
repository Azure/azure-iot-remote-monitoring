using Microsoft.WindowsAzure.Storage.Table;

namespace Microsoft.Azure.Devices.Applications.RemoteMonitoring.DeviceAdmin.Infrastructure.Models
{
    public class DeviceTwinMethodTableEntity : TableEntity
    {
        public DeviceTwinMethodTableEntity(DeviceTwinMethodEntityType entityEnum, string name)
        {
            this.PartitionKey = entityEnum.ToString();
            this.RowKey = name;
            switch (entityEnum)
            {
                case DeviceTwinMethodEntityType.Tag:
                    this.TagName = name;
                    break;
                case DeviceTwinMethodEntityType.Property:
                    this.PropertyName = name;
                    break;
                case DeviceTwinMethodEntityType.Method:
                    this.MethodName = name;
                    break;
            }
        }

        public DeviceTwinMethodTableEntity() { }

        public string TagName { get; set; }

        public string PropertyName { get; set; }

        public string MethodName { get; set; }

        public string MethodParameters { get; set; }

        public string MethodDescription { get; set; }
    }
}
