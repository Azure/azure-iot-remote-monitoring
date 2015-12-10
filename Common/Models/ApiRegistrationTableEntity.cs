using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.WindowsAzure.Storage.Table;

namespace Microsoft.Azure.Devices.Applications.RemoteMonitoring.Common.Models
{
    public class ApiRegistrationTableEntity : TableEntity
    {
        public string BaseUrl { get; set; }
        public string LicenceKey { get; set; }
        public string Username { get; set; }
        public string Password { get; set; }

        // when more providers are available take as param
        public ApiRegistrationTableEntity()
        {
            PartitionKey = Convert.ToString((int) ApiRegistrationProviderType.Jasper);
            RowKey = Enum.GetName(typeof (ApiRegistrationProviderType), ApiRegistrationProviderType.Jasper);
        }
    }

    public enum ApiRegistrationProviderType
    {
        Jasper
    }
    
}
