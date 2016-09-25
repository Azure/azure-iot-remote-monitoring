using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.WindowsAzure.Storage.Table;

namespace Microsoft.Azure.Devices.Applications.RemoteMonitoring.Common.Models
{
    public class IccidTableEntity : TableEntity
    {
        public IccidTableEntity()
        {
            this.RowKey = Guid.NewGuid().ToString();
            this.PartitionKey = IccidRegistrationKey.Default.ToString();
        }

        public string Iccid { get; set; }
        public string ProviderName { get; set; }

    }

    public enum IccidRegistrationKey
    {
        Default = 0
    }
}
