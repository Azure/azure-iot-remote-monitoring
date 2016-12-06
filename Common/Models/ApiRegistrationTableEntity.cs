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
        public string ApiRegistrationProviderType { get; set; }

        // Erricsson Only
        public string EnterpriseSenderNumber { get; set; }
        public string RegistrationID { get; set; }
        public string SmsEndpointBaseUrl { get; set; }

        // when more providers are available take as param
        public ApiRegistrationTableEntity()
        {
            PartitionKey = GetPartitionKey(ApiRegistrationKey.Default);
            RowKey = GetRowKey(ApiRegistrationKey.Default);
        }

        public static string GetPartitionKey(ApiRegistrationKey providerType)
        {
            return Convert.ToString((int)providerType, CultureInfo.InvariantCulture);
        }

        public static string GetRowKey(ApiRegistrationKey providerType)
        {
            return Enum.GetName(typeof(ApiRegistrationKey), providerType);
        }

        public static ApiRegistrationKey GetRowKey(string providerTypeString)
        {
            return (ApiRegistrationKey)Enum.Parse(typeof(ApiRegistrationKey), providerTypeString);
        }
    }

    public enum ApiRegistrationKey
    {
        Default=0
    }
}
