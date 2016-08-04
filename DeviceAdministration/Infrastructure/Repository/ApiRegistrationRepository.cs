using System.Collections.Generic;
using System.Linq;
using Microsoft.Azure.Devices.Applications.RemoteMonitoring.Common.Configurations;
using Microsoft.Azure.Devices.Applications.RemoteMonitoring.Common.Helpers;
using Microsoft.Azure.Devices.Applications.RemoteMonitoring.Common.Models;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Table;
using System;

namespace Microsoft.Azure.Devices.Applications.RemoteMonitoring.DeviceAdmin.Infrastructure.Repository
{
    public class ApiRegistrationRepository : IApiRegistrationRepository
    {
        private readonly CloudTable _table;
        private const string API_TABLE_NAME = "ApiRegistration";

        public ApiRegistrationRepository(IConfigurationProvider configProvider)
        {
            _table = AzureTableStorageHelper.GetTable(
                   configProvider.GetConfigurationSettingValue("device.StorageConnectionString"), API_TABLE_NAME);
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
                    ApiRegistrationProviderType = ApiRegistrationTableEntity
                                                    .ConvertApiProviderTypeToInt(apiRegistrationModel.ApiRegistrationProvider.Value)
            };

                _table.Execute(TableOperation.InsertOrMerge(incomingEntity));
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

            var response = _table.ExecuteQuery(query);
            if (response == null) return new ApiRegistrationModel();

            var apiRegistrationTableEntities = response as IList<ApiRegistrationTableEntity> ?? response.ToList();
            var apiRegistrationTableEntity = apiRegistrationTableEntities.FirstOrDefault();

            if (apiRegistrationTableEntity == null) return new ApiRegistrationModel();

            return new ApiRegistrationModel()
            {
                Username = apiRegistrationTableEntity.Username,
                BaseUrl = apiRegistrationTableEntity.BaseUrl,
                LicenceKey = apiRegistrationTableEntity.LicenceKey,
                Password = apiRegistrationTableEntity.Password,
                ApiRegistrationProvider = ApiRegistrationTableEntity
                                            .ConvertIntToApiProvider(apiRegistrationTableEntity.ApiRegistrationProviderType)
            };

        }

        public bool IsApiRegisteredInAzure()
        {
            var retrieveOperation = TableOperation.Retrieve<ApiRegistrationTableEntity>(ApiRegistrationTableEntity.GetPartitionKey(ApiRegistrationKey.Default),
                                        ApiRegistrationTableEntity.GetRowKey(ApiRegistrationKey.Default));
            var retrievedResult = _table.Execute(retrieveOperation);
            return retrievedResult.Result != null;
        }

        public bool DeleteApiDetails()
        {
            var entity = new DynamicTableEntity(ApiRegistrationTableEntity.GetPartitionKey(ApiRegistrationKey.Default),
                                ApiRegistrationTableEntity.GetRowKey(ApiRegistrationKey.Default));
            entity.ETag = "*";
            _table.Execute(TableOperation.Delete(entity));
            return true;
        }
    }
}