using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading.Tasks;
using Microsoft.Azure.Devices.Applications.RemoteMonitoring.Common.Configurations;
using Microsoft.Azure.Devices.Applications.RemoteMonitoring.Common.Helpers;
using Microsoft.Azure.Devices.Applications.RemoteMonitoring.Common.Models;
using Microsoft.Azure.Devices.Applications.RemoteMonitoring.DeviceAdmin.Infrastructure.Models;
using Microsoft.WindowsAzure.Storage.Table;
using Newtonsoft.Json;

namespace Microsoft.Azure.Devices.Applications.RemoteMonitoring.DeviceAdmin.Infrastructure.Repository
{
    public class DeviceListColumnsRepository : IDeviceListColumnsRepository
    {
        private readonly string _storageAccountConnectionString;
        private readonly IAzureTableStorageClient _azureTableStorageClient;
        private readonly string _tableName = "ColumnList";
        private const string _tablePartitionKey = "columns";

        public DeviceListColumnsRepository(IConfigurationProvider configurationProvider, IAzureTableStorageClientFactory tableStorageClientFactory)
        {
            _storageAccountConnectionString = configurationProvider.GetConfigurationSettingValue("device.StorageConnectionString");
            _azureTableStorageClient = tableStorageClientFactory.CreateClient(_storageAccountConnectionString, _tableName);
        }

        public async Task<bool> SaveAsync(string userId, IEnumerable<DeviceListColumns> columns)
        {
            DeviceListColumnsTableEntity entity = new DeviceListColumnsTableEntity(userId);
            entity.PartitionKey = _tablePartitionKey;
            entity.ETag = "*";
            entity.Columns = JsonConvert.SerializeObject(columns);
            
            var result = await _azureTableStorageClient.DoTableInsertOrReplaceAsync(entity, BuildQueryModelFromEntity);
            return (result.Status == TableStorageResponseStatus.Successful);
        }

        public async Task<IEnumerable<DeviceListColumns>> GetAsync(string userId)
        {
            var operation = TableOperation.Retrieve<DeviceListColumnsTableEntity>(_tablePartitionKey, userId);
            var retrievedEntity = await _azureTableStorageClient.ExecuteAsync(operation);
            return BuildQueryModelFromEntity((DeviceListColumnsTableEntity)retrievedEntity.Result);
        }

        private IEnumerable<DeviceListColumns> BuildQueryModelFromEntity(DeviceListColumnsTableEntity entity)
        {
            if (entity == null)
            {
                return null;
            }

            IEnumerable<DeviceListColumns> columns = null;
            try
            {
                columns = JsonConvert.DeserializeObject<IEnumerable<DeviceListColumns>>(entity.Columns);
            }
            catch (Exception)
            {
                Trace.TraceError("Can not deserialize filters to Json object", entity.Columns);
            }

            return columns;
        }
    }
}
