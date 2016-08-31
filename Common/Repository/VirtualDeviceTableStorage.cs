using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Azure.Devices.Applications.RemoteMonitoring.Common.Configurations;
using Microsoft.Azure.Devices.Applications.RemoteMonitoring.Common.Models;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Table;
using Microsoft.Azure.Devices.Applications.RemoteMonitoring.Common.Helpers;

namespace Microsoft.Azure.Devices.Applications.RemoteMonitoring.Common.Repository
{
    public class VirtualDeviceTableStorage : IVirtualDeviceStorage
    {
        private readonly IAzureTableStorageClient _azureTableStorageClient;

        public VirtualDeviceTableStorage(IConfigurationProvider configProvider, IAzureTableStorageClientFactory tableStorageClientFactory)
        {
            var storageConnectionString = configProvider.GetConfigurationSettingValue("device.StorageConnectionString");
            var deviceTableName = configProvider.GetConfigurationSettingValue("device.TableName");
            _azureTableStorageClient = tableStorageClientFactory.CreateClient(storageConnectionString, deviceTableName);
        }

        public async Task<List<InitialDeviceConfig>> GetDeviceListAsync()
        {
            List<InitialDeviceConfig> devices = new List<InitialDeviceConfig>();
            TableQuery<DeviceListEntity> query = new TableQuery<DeviceListEntity>();
            var devicesResult = await _azureTableStorageClient.ExecuteQueryAsync(query);
            foreach (var device in devicesResult)
            {
                var deviceConfig = new InitialDeviceConfig()
                {
                    HostName = device.HostName,
                    DeviceId = device.DeviceId,
                    Key = device.Key
                };
                devices.Add(deviceConfig);
            }
            return devices;
        }

        public Task<InitialDeviceConfig> GetDeviceAsync(string deviceId)
        {
            var query = new TableQuery<DeviceListEntity>().Where(TableQuery.GenerateFilterCondition("PartitionKey", QueryComparisons.Equal, deviceId));
            return this.GetDeviceAsync(query);
        }

        public async Task<bool> RemoveDeviceAsync(string deviceId)
        {
            var device = await this.GetDeviceAsync(deviceId);
            if (device != null)
            {
                var operation = TableOperation.Retrieve<DeviceListEntity>(device.DeviceId, device.HostName);
                var result = await _azureTableStorageClient.ExecuteAsync(operation);

                var deleteDevice = (DeviceListEntity)result.Result;
                if (deleteDevice != null)
                {
                    var deleteOperation = TableOperation.Delete(deleteDevice);
                    await _azureTableStorageClient.ExecuteAsync(deleteOperation);
                    return true;
                }
            }
            return false;
        }

        public async Task AddOrUpdateDeviceAsync(InitialDeviceConfig deviceConfig)
        {
            var deviceEnity = new DeviceListEntity()
            {
                DeviceId = deviceConfig.DeviceId,
                HostName = deviceConfig.HostName,
                Key = deviceConfig.Key
            };
            var operation = TableOperation.InsertOrReplace(deviceEnity);
            await _azureTableStorageClient.ExecuteAsync(operation);
        }

        private async Task<InitialDeviceConfig> GetDeviceAsync(TableQuery<DeviceListEntity> query)
        {
            var devicesResult = await _azureTableStorageClient.ExecuteQueryAsync(query);
            foreach (var device in devicesResult)
            {
                // Always return first device found
                return new InitialDeviceConfig
                {
                    DeviceId = device.DeviceId,
                    HostName = device.HostName,
                    Key = device.Key
                };
            }
            return null;
        }
    }
}
