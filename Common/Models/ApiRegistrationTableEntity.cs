using System;
using System.Globalization;
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
            PartitionKey = GetPartitionKey(ApiRegistrationProviderType.Jasper);
            RowKey = GetRowKey(ApiRegistrationProviderType.Jasper);
        }

        public static string GetPartitionKey(ApiRegistrationProviderType providerType)
        {
            return Convert.ToString((int)providerType, CultureInfo.InvariantCulture);
        }

        public static string GetRowKey(ApiRegistrationProviderType providerType)
        {
            return Enum.GetName(typeof(ApiRegistrationProviderType), providerType);
        }
    }

    public enum ApiRegistrationProviderType
    {
        Jasper
    }
}
