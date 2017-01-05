using Microsoft.WindowsAzure.Storage.Table;

namespace Microsoft.Azure.Devices.Applications.RemoteMonitoring.Common.Models
{
    public class IccidTableEntity : TableEntity
    {
        public IccidTableEntity(string iccid)
        {
            RowKey = iccid;
            PartitionKey = IccidRegistrationKey.Default.ToString();
        }

        public IccidTableEntity()
        {
            PartitionKey = IccidRegistrationKey.Default.ToString();
        }

        public string Iccid { get; set; }
        public string ProviderName { get; set; }
        public string LastSetLocaleServiceRequestId { get; set; }
    }

    public enum IccidRegistrationKey
    {
        Default = 0
    }
}
