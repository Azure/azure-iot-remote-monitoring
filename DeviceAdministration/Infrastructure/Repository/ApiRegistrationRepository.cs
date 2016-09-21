using System.Collections.Generic;
using System.Linq;
using Microsoft.Azure.Devices.Applications.RemoteMonitoring.Common.Configurations;
using Microsoft.Azure.Devices.Applications.RemoteMonitoring.Common.Helpers;
using Microsoft.Azure.Devices.Applications.RemoteMonitoring.Common.Models;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Table;
using System;
using DeviceManagement.Infrustructure.Connectivity.Models.Enums;

namespace Microsoft.Azure.Devices.Applications.RemoteMonitoring.DeviceAdmin.Infrastructure.Repository
{
    public class ApiRegistrationRepository : IApiRegistrationRepository
    {
        private const string API_TABLE_NAME = "ApiRegistration";
        private readonly IAzureTableStorageClient _azureTableStorageClient;

        public ApiRegistrationRepository(IConfigurationProvider configProvider, IAzureTableStorageClientFactory tableStorageClientFactory)
        {
            _azureTableStorageClient = tableStorageClientFactory.CreateClient(configProvider.GetConfigurationSettingValue("device.StorageConnectionString"), API_TABLE_NAME);
        }

        public bool AmendRegistration(ApiRegistrationModel apiRegistrationModel)
        {
            try
            {
                var incomingEntity = new ApiRegistrationTableEntity()
                {
                    Password = apiRegistrationModel.Password,
                    BaseUrl = apiRegistrationModel.BaseUrl,
                    Username = apiRegistrationModel.Username,
                    LicenceKey = apiRegistrationModel.LicenceKey,
                    ApiRegistrationProviderType = apiRegistrationModel.ApiRegistrationProvider.ToString()
                };

                _azureTableStorageClient.Execute(TableOperation.InsertOrMerge(incomingEntity));
            }
            catch (StorageException)
            {
                return false;
            }
            return true;
        }

        public ApiRegistrationModel RecieveDetails()
        {
            var query = new TableQuery<ApiRegistrationTableEntity>().
                Where(TableQuery.GenerateFilterCondition("PartitionKey", QueryComparisons.Equal,
                    ApiRegistrationTableEntity.GetPartitionKey(ApiRegistrationKey.Default)));

            var response = _azureTableStorageClient.ExecuteQuery(query);
            if (response == null) return new ApiRegistrationModel();

            var apiRegistrationTableEntities = response as IList<ApiRegistrationTableEntity> ?? response.ToList();
            var apiRegistrationTableEntity = apiRegistrationTableEntities.FirstOrDefault();

            if (apiRegistrationTableEntity == null) return new ApiRegistrationModel();

            var apiRegistrationProvider = apiRegistrationTableEntity.ApiRegistrationProviderType != null ?
                (ApiRegistrationProviderType)Enum.Parse(typeof(ApiRegistrationProviderType), apiRegistrationTableEntity.ApiRegistrationProviderType) :
                (ApiRegistrationProviderType?)null;

            return new ApiRegistrationModel()
            {
                Username = apiRegistrationTableEntity.Username,
                BaseUrl = apiRegistrationTableEntity.BaseUrl,
                LicenceKey = apiRegistrationTableEntity.LicenceKey,
                Password = apiRegistrationTableEntity.Password,
                ApiRegistrationProvider = apiRegistrationProvider
            };

        }

        public bool IsApiRegisteredInAzure()
        {
            var retrieveOperation = TableOperation.Retrieve<ApiRegistrationTableEntity>(ApiRegistrationTableEntity.GetPartitionKey(ApiRegistrationKey.Default),
                                        ApiRegistrationTableEntity.GetRowKey(ApiRegistrationKey.Default));
            var retrievedResult = _azureTableStorageClient.Execute(retrieveOperation);
            return retrievedResult.Result != null;
        }

        public bool DeleteApiDetails()
        {
            var entity = new DynamicTableEntity(ApiRegistrationTableEntity.GetPartitionKey(ApiRegistrationKey.Default),
                                ApiRegistrationTableEntity.GetRowKey(ApiRegistrationKey.Default));
            entity.ETag = "*";
            _azureTableStorageClient.Execute(TableOperation.Delete(entity));
            return true;
        }
    }
}