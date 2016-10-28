using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Azure.Devices.Applications.RemoteMonitoring.Common.Configurations;
using Microsoft.Azure.Devices.Applications.RemoteMonitoring.Common.Helpers;
using Microsoft.Azure.Devices.Applications.RemoteMonitoring.Common.Models;
using Microsoft.Azure.Devices.Applications.RemoteMonitoring.DeviceAdmin.Infrastructure.Models;
using Microsoft.WindowsAzure.Storage.Table;

namespace Microsoft.Azure.Devices.Applications.RemoteMonitoring.DeviceAdmin.Infrastructure.Repository
{
    /// <summary>
    /// Class for working with persistence of Device Twin (Tag and Property)
    /// and Method list defined for all devices. Note that we store Twin and
    /// Method list in the same table in separate columns. The list maintain
    /// a maximum set of unique tag names, property names and method names.
    /// </summary>
    public class DeviceTwinMethodRegistrationRepository : IDeviceTwinMethodRegistrationRepository
    {
        private readonly string _storageAccountConnectionString;
        private readonly IAzureTableStorageClient _azureTableStorageClient;
        private readonly string _deviceTwinMethodTableName;

        public DeviceTwinMethodRegistrationRepository(IConfigurationProvider configurationProvider, IAzureTableStorageClientFactory tableStorageClientFactory)
        {
            _storageAccountConnectionString = configurationProvider.GetConfigurationSettingValue("device.StorageConnectionString");
            _deviceTwinMethodTableName = configurationProvider.GetConfigurationSettingValueOrDefault("DeviceTwinMethodTableName", "DeviceTwinMethodList");
            _azureTableStorageClient = tableStorageClientFactory.CreateClient(_storageAccountConnectionString, _deviceTwinMethodTableName);
        }

        /// <summary>
        /// Retrieve a list of tag names saved for all devices
        /// to help user to pick up when define the query.
        /// </summary>
        /// <returns>All tag names or empty list</returns>
        public async Task<IEnumerable<string>> GetAllDeviceTagNamesAsync()
        {
            TableQuery<DeviceTwinMethodTableEntity> query = new TableQuery<DeviceTwinMethodTableEntity>().Where(TableQuery.GenerateFilterCondition("PartitionKey", QueryComparisons.Equal, DeviceTwinMethodEntityType.Tag.ToString()));
            var entities = await _azureTableStorageClient.ExecuteQueryAsync(query);
            return entities.OrderByDescending(e => e.Timestamp).Select(e => e.TagName).ToList();
        }

        /// <summary>
        /// Save a device tag name to the server. This may be either a new tag or
        /// an update to an existing tag.
        /// </summary>
        /// <param name="name"></param>
        /// <returns>true if succeeded, false if failed.</returns>
        public async Task<bool> AddDeviceTagNameAsync(string name)
        {
            DeviceTwinMethodTableEntity tableEntity = new DeviceTwinMethodTableEntity(DeviceTwinMethodEntityType.Tag, name);
            tableEntity.ETag = "*";
            var result = await _azureTableStorageClient.DoTableInsertOrReplaceAsync<DeviceTwinMethodEntity, DeviceTwinMethodTableEntity>(tableEntity, BuildDeviceTwinMethodFromTableEntity);
            return (result.Status == TableStorageResponseStatus.Successful);
        }

        public async Task<bool> DeleteDeviceTagNameAsync(string name)
        {
            DeviceTwinMethodTableEntity tableEntity = new DeviceTwinMethodTableEntity(DeviceTwinMethodEntityType.Tag, name);
            tableEntity.ETag = "*";
            var result = await _azureTableStorageClient.DoDeleteAsync<DeviceTwinMethodEntity, DeviceTwinMethodTableEntity>(tableEntity, BuildDeviceTwinMethodFromTableEntity);
            return (result.Status == TableStorageResponseStatus.Successful);
        }

        /// <summary>
        /// Retrieve a list of property names saved for all devices
        /// to help user to pick up when define the query.
        /// </summary>
        /// <returns>All property names or empty list</returns>
        public async Task<IEnumerable<string>> GetAllDevicePropertyNamesAsync()
        {
            TableQuery<DeviceTwinMethodTableEntity> query = new TableQuery<DeviceTwinMethodTableEntity>().Where(TableQuery.GenerateFilterCondition("PartitionKey", QueryComparisons.Equal, DeviceTwinMethodEntityType.Property.ToString()));
            var entities = await _azureTableStorageClient.ExecuteQueryAsync(query);
            return entities.OrderByDescending(e => e.Timestamp).Select(e => e.PropertyName).ToList();
        }

        /// <summary>
        /// Save a device property name to the server. This may be either a new property or
        /// an update to an existing property.
        /// </summary>
        /// <param name="name"></param>
        /// <returns>true if succeeded, false if failed.</returns>
        public async Task<bool> AddDevicePropertyNameAsync(string name)
        {
            DeviceTwinMethodTableEntity tableEntity = new DeviceTwinMethodTableEntity(DeviceTwinMethodEntityType.Property, name);
            tableEntity.ETag = "*";
            var result = await _azureTableStorageClient.DoTableInsertOrReplaceAsync<DeviceTwinMethodEntity, DeviceTwinMethodTableEntity>(tableEntity, BuildDeviceTwinMethodFromTableEntity);
            return (result.Status == TableStorageResponseStatus.Successful);
        }

        public async Task<bool> DeleteDevicePropertyNameAsync(string name)
        {
            DeviceTwinMethodTableEntity tableEntity = new DeviceTwinMethodTableEntity(DeviceTwinMethodEntityType.Property, name);
            tableEntity.ETag = "*";
            var result = await _azureTableStorageClient.DoDeleteAsync<DeviceTwinMethodEntity, DeviceTwinMethodTableEntity>(tableEntity, BuildDeviceTwinMethodFromTableEntity);
            return (result.Status == TableStorageResponseStatus.Successful);
        }

        /// <summary>
        /// Retrieve a list of Device Method saved for all devices
        /// to help user to pick up when define the command.
        /// </summary>
        /// <returns>All DeviceMethods or empty list</returns>
        public async Task<IEnumerable<DeviceMethod>> GetAllDeviceMethodsAsync()
        {
            TableQuery<DeviceTwinMethodTableEntity> query = new TableQuery<DeviceTwinMethodTableEntity>().Where(TableQuery.GenerateFilterCondition("PartitionKey", QueryComparisons.Equal, DeviceTwinMethodEntityType.Method.ToString()));
            var entities = await _azureTableStorageClient.ExecuteQueryAsync(query);
            return entities.OrderByDescending(e => e.Timestamp).
                Select(e => new DeviceMethod
                {
                    Name = e.MethodName,
                    Parameters = e.MethodParameters,
                    Description = e.MethodDescription,
                });
        }

        /// <summary>
        /// Save a device method to the server. This may be either a new method or
        /// an update to an existing method.
        /// </summary>
        /// <param name="method"></param>
        /// <returns>true if succeeded, false if failed.</returns>
        public async Task<bool> AddDeviceMethodAsync(DeviceMethod method)
        {
            DeviceTwinMethodTableEntity tableEntity = new DeviceTwinMethodTableEntity(DeviceTwinMethodEntityType.Method, method.Name);
            tableEntity.MethodParameters = method.Parameters;
            tableEntity.MethodDescription = method.Description;
            tableEntity.ETag = "*";
            var result = await _azureTableStorageClient.DoTableInsertOrReplaceAsync<DeviceTwinMethodEntity, DeviceTwinMethodTableEntity>(tableEntity, BuildDeviceTwinMethodFromTableEntity);
            return (result.Status == TableStorageResponseStatus.Successful);
        }

        public async Task<bool> DeleteDeviceMethodAsync(DeviceMethod method)
        {
            DeviceTwinMethodTableEntity tableEntity = new DeviceTwinMethodTableEntity(DeviceTwinMethodEntityType.Method, method.Name);
            tableEntity.MethodParameters = method.Parameters;
            tableEntity.MethodDescription = method.Description;
            tableEntity.ETag = "*";
            var result = await _azureTableStorageClient.DoDeleteAsync<DeviceTwinMethodEntity, DeviceTwinMethodTableEntity>(tableEntity, BuildDeviceTwinMethodFromTableEntity);
            return (result.Status == TableStorageResponseStatus.Successful);
        }

        private DeviceTwinMethodEntity BuildDeviceTwinMethodFromTableEntity(DeviceTwinMethodTableEntity tableEntity)
        {
            if (tableEntity == null)
            {
                return null;
            }
            var deviceTwinMethod = new DeviceTwinMethodEntity
            {
                TagName = tableEntity?.TagName,
                PropertyName = tableEntity?.PropertyName,
                Method = new DeviceMethod
                {
                    Name = tableEntity?.MethodName,
                    Parameters = tableEntity?.MethodParameters,
                    Description = tableEntity?.MethodDescription,
                }
            };

            if (!string.IsNullOrWhiteSpace(tableEntity.ETag))
            {
                deviceTwinMethod.ETag = tableEntity.ETag;
            }

            return deviceTwinMethod;
        }
    }
}
