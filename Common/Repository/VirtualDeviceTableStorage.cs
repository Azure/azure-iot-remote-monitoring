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
        private readonly string _storageConnectionString;
        private readonly string _deviceTableName;

        public VirtualDeviceTableStorage(IConfigurationProvider configProvider)
        {
            _storageConnectionString = configProvider.GetConfigurationSettingValue("device.StorageConnectionString");
            _deviceTableName = configProvider.GetConfigurationSettingValue("device.TableName");
        }

        public async Task<List<InitialDeviceConfig>> GetDeviceListAsync()
        {
            List<InitialDeviceConfig> devices = new List<InitialDeviceConfig>();
            var devicesTable = await AzureTableStorageHelper.GetTableAsync(_storageConnectionString, _deviceTableName);
            TableQuery<DeviceListEntity> query = new TableQuery<DeviceListEntity>();
            foreach (var device in devicesTable.ExecuteQuery(query))
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

        public Task<InitialDeviceConfig> GetDevice(string deviceId, string hostName)
        {
            var query = new TableQuery<DeviceListEntity>().Where(TableQuery.CombineFilters(
                TableQuery.GenerateFilterCondition("PartitionKey", QueryComparisons.Equal, deviceId),
                TableOperators.And,
                TableQuery.GenerateFilterCondition("RowKey", QueryComparisons.Equal, hostName)));
            return this.GetDeviceAsync(query);
        }

        public async Task<bool> RemoveDeviceAsync(string deviceId)
        {
            var devicesTable = await AzureTableStorageHelper.GetTableAsync(_storageConnectionString, _deviceTableName);
            var device = await this.GetDeviceAsync(deviceId);
            if (device != null)
            {
                var operation = TableOperation.Retrieve<DeviceListEntity>(device.DeviceId, device.HostName);
                var result = await devicesTable.ExecuteAsync(operation);

                var deleteDevice = (DeviceListEntity)result.Result;
                if (deleteDevice != null)
                {
                    var deleteOperation = TableOperation.Delete(deleteDevice);
                    await devicesTable.ExecuteAsync(deleteOperation);
                    return true;
                }
            }
            return false;
        }

        public async Task AddOrUpdateDeviceAsync(InitialDeviceConfig deviceConfig)
        {
            var devicesTable = await AzureTableStorageHelper.GetTableAsync(_storageConnectionString, _deviceTableName);
            var deviceEnity = new DeviceListEntity()
            {
                DeviceId = deviceConfig.DeviceId,
                HostName = deviceConfig.HostName,
                Key = deviceConfig.Key
            };
            var operation = TableOperation.InsertOrReplace(deviceEnity);
            await devicesTable.ExecuteAsync(operation);
        }

        private async Task<InitialDeviceConfig> GetDeviceAsync(TableQuery<DeviceListEntity> query)
        {
            var devicesTable = await AzureTableStorageHelper.GetTableAsync(_storageConnectionString, _deviceTableName);
            foreach (var device in devicesTable.ExecuteQuery<DeviceListEntity>(query))
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
