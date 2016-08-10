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
        public DeviceManagement.Infrustructure.Connectivity.Models.Enums.ApiRegistrationProviderType? ApiRegistrationProviderType { get; set; }

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

        public static int? ConvertApiProviderTypeToInt(ApiRegistrationProviderType? providerType)
        {
            if (!providerType.HasValue)
            {
                return null;
            }
            return Convert.ToInt32(providerType, CultureInfo.InvariantCulture);
        }

        public static ApiRegistrationProviderType? ConvertIntToApiProvider(int? providerType)
        {
            if (!providerType.HasValue)
            {
                return null;
            }
            return (ApiRegistrationProviderType)providerType;
        }

        public static string GetRowKey(ApiRegistrationKey providerType)
        {
            return Enum.GetName(typeof(ApiRegistrationKey), providerType);
        }

        public static ApiRegistrationProviderType GetRowKey(string providerTypeString)
        {
            return (ApiRegistrationProviderType)Enum.Parse(typeof(ApiRegistrationProviderType), providerTypeString);
        }
    }

    public enum ApiRegistrationProviderType
    {
        Jasper=0,
        Ericsson=1
    }

    public enum ApiRegistrationKey
    {
        Default=0
    }
}
