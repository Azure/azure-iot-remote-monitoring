using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Azure.Devices.Applications.RemoteMonitoring.Common.Configurations;
using Microsoft.Azure.Devices.Applications.RemoteMonitoring.Common.Helpers;
using Microsoft.Azure.Devices.Applications.RemoteMonitoring.Common.Models;
using Microsoft.Azure.Devices.Applications.RemoteMonitoring.DeviceAdmin.Infrastructure.Models;
using Microsoft.WindowsAzure.Storage.Table;
using Newtonsoft.Json;

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
        /// Get name list for combined DeviceTwinMethodEntityType flags
        /// </summary>
        /// <param name="entityType"></param>
        /// <returns>a list of combined names</returns>
        public async Task<IEnumerable<DeviceTwinMethodEntity>> GetNameListAsync(DeviceTwinMethodEntityType entityType)
        {
            List<string> filters = new List<string>();
            var flags = Enum.GetValues(typeof(DeviceTwinMethodEntityType));
            foreach (DeviceTwinMethodEntityType flag in flags)
            {
                if (entityType.HasFlag(flag) && flag != DeviceTwinMethodEntityType.All && flag != DeviceTwinMethodEntityType.Property)
                {
                    var condition = TableQuery.GenerateFilterCondition("PartitionKey", QueryComparisons.Equal, flag.ToString());
                    filters.Add(condition);
                }
            }
            TableQuery<DeviceTwinMethodTableEntity> query = new TableQuery<DeviceTwinMethodTableEntity>().Where(string.Join(" or ", filters));
            var entities = await _azureTableStorageClient.ExecuteQueryAsync(query);
            return entities.OrderByDescending(e => e.Timestamp).
                Select(e => new DeviceTwinMethodEntity
                {
                    Name = e.Name,
                    Parameters = JsonConvert.DeserializeObject<List<Parameter>>(e.MethodParameters),
                    Description = e.MethodDescription,
                });
        }

        /// <summary>
        /// Save a new name into the repository. Update it if it already exists.
        /// </summary>
        /// <param name="name"></param>
        /// <returns>true if succeeded, false if failed.</returns>
        public async Task<bool> AddNameAsync(DeviceTwinMethodEntityType entityType, DeviceTwinMethodEntity entity)
        {
            CheckSingleEntityType(entityType);
            DeviceTwinMethodTableEntity tableEntity = new DeviceTwinMethodTableEntity(entityType, entity.Name);
            tableEntity.MethodParameters = JsonConvert.SerializeObject(entity.Parameters);
            tableEntity.MethodDescription = entity.Description;
            tableEntity.ETag = "*";
            var result = await _azureTableStorageClient.DoTableInsertOrReplaceAsync<DeviceTwinMethodEntity, DeviceTwinMethodTableEntity>(tableEntity, BuildDeviceTwinMethodFromTableEntity);
            return (result.Status == TableStorageResponseStatus.Successful);
        }

        /// <summary>
        /// Delete an existing name from repository.
        /// </summary>
        /// <param name="entityType"></param>
        /// <param name="name"></param>
        /// <returns>true if succeeded, false if failed.</returns>
        public async Task<bool> DeleteNameAsync(DeviceTwinMethodEntityType entityType, string name)
        {
            CheckSingleEntityType(entityType);
            DeviceTwinMethodTableEntity tableEntity = new DeviceTwinMethodTableEntity(entityType, name);
            tableEntity.ETag = "*";
            var result = await _azureTableStorageClient.DoDeleteAsync<DeviceTwinMethodEntity, DeviceTwinMethodTableEntity>(tableEntity, BuildDeviceTwinMethodFromTableEntity);
            return (result.Status == TableStorageResponseStatus.Successful);
        }

        private void CheckSingleEntityType(DeviceTwinMethodEntityType entityType)
        {
            if (entityType == DeviceTwinMethodEntityType.DeviceInfo
                || entityType == DeviceTwinMethodEntityType.Tag
                || entityType == DeviceTwinMethodEntityType.DesiredProperty
                || entityType == DeviceTwinMethodEntityType.ReportedProperty
                || entityType == DeviceTwinMethodEntityType.Method)
                return;
            throw new ArgumentException("Can only pick up one of the flags: DeviceInfo, Tag, DesiredProperty, ReportedProperty, Method");
        }

        private DeviceTwinMethodEntity BuildDeviceTwinMethodFromTableEntity(DeviceTwinMethodTableEntity tableEntity)
        {
            if (tableEntity == null)
            {
                return null;
            }

            var parameters = new List<Parameter>();
            try
            {
                parameters = JsonConvert.DeserializeObject<List<Parameter>>(tableEntity.MethodParameters);
            }
            catch (Exception)
            {
                Trace.TraceError("Failed to deserialize object for method parameters: {0}", tableEntity.MethodParameters);
            }

            var deviceTwinMethod = new DeviceTwinMethodEntity
            {
                Name = tableEntity?.Name,
                Parameters = parameters,
                Description = tableEntity?.MethodDescription,
            };

            return deviceTwinMethod;
        }
    }
}
