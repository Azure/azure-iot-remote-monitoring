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
    public class NameCacheRepository : INameCacheRepository
    {
        public int MaxBatchSize => 100;

        private readonly string _storageAccountConnectionString;
        private readonly IAzureTableStorageClient _azureTableStorageClient;
        private readonly string _nameCacheTableName;

        public NameCacheRepository(IConfigurationProvider configurationProvider, IAzureTableStorageClientFactory tableStorageClientFactory)
        {
            _storageAccountConnectionString = configurationProvider.GetConfigurationSettingValue("device.StorageConnectionString");
            _nameCacheTableName = configurationProvider.GetConfigurationSettingValueOrDefault("NameCacheTableName", "NameCacheList");
            _azureTableStorageClient = tableStorageClientFactory.CreateClient(_storageAccountConnectionString, _nameCacheTableName);
        }

        /// <summary>
        /// Get name list for combined NameCacheEntityType flags
        /// </summary>
        /// <param name="entityType"></param>
        /// <returns>a list of combined names</returns>
        public async Task<IEnumerable<NameCacheEntity>> GetNameListAsync(NameCacheEntityType entityType)
        {
            List<string> filters = new List<string>();
            var flags = Enum.GetValues(typeof(NameCacheEntityType));
            foreach (NameCacheEntityType flag in flags)
            {
                if (entityType.HasFlag(flag) && flag != NameCacheEntityType.All && flag != NameCacheEntityType.Property)
                {
                    var condition = TableQuery.GenerateFilterCondition("PartitionKey", QueryComparisons.Equal, flag.ToString());
                    filters.Add(condition);
                }
            }
            TableQuery<NameCacheTableEntity> query = new TableQuery<NameCacheTableEntity>().Where(string.Join(" or ", filters));
            var entities = await _azureTableStorageClient.ExecuteQueryAsync(query);
            return entities.OrderBy(e => e.PartitionKey).ThenBy(e => e.RowKey).
                Select(e => new NameCacheEntity
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
        public async Task<bool> AddNameAsync(NameCacheEntityType entityType, NameCacheEntity entity)
        {
            CheckSingleEntityType(entityType);
            NameCacheTableEntity tableEntity = new NameCacheTableEntity(entityType, entity.Name);
            tableEntity.MethodParameters = JsonConvert.SerializeObject(entity.Parameters);
            tableEntity.MethodDescription = entity.Description;
            tableEntity.ETag = "*";
            var result = await _azureTableStorageClient.DoTableInsertOrReplaceAsync<NameCacheEntity, NameCacheTableEntity>(tableEntity, BuildNameCacheFromTableEntity);
            return (result.Status == TableStorageResponseStatus.Successful);
        }

        /// <summary>
        /// Add a set of names in batch
        /// Reminder: Considering it will be called periodically to update the cache, it was
        /// not designed as a realiable routine to reduce complexity
        /// </summary>
        /// <param name="entityType">Type of adding names</param>
        /// <param name="names">Names to be added</param>
        /// <returns>The asychornize task</returns>
        public async Task AddNamesAsync(NameCacheEntityType entityType, IEnumerable<string> names)
        {
            CheckSingleEntityType(entityType);

            var operations = names.Select(name => TableOperation.InsertOrReplace(new NameCacheTableEntity(entityType, name)
            {
                MethodParameters = "null",  // [WORKAROUND] Existing code requires "null" rather than null for tag or properties
                ETag = "*"
            }));

            while (operations.Any())
            {
                var batch = new TableBatchOperation();

                operations.Take(MaxBatchSize).ToList().ForEach(op => batch.Add(op));
                await _azureTableStorageClient.ExecuteBatchAsync(batch);

                operations = operations.Skip(MaxBatchSize);
            }
        }

        /// <summary>
        /// Delete an existing name from repository.
        /// </summary>
        /// <param name="entityType"></param>
        /// <param name="name"></param>
        /// <returns>true if succeeded, false if failed.</returns>
        public async Task<bool> DeleteNameAsync(NameCacheEntityType entityType, string name)
        {
            CheckSingleEntityType(entityType);
            NameCacheTableEntity tableEntity = new NameCacheTableEntity(entityType, name);
            tableEntity.ETag = "*";
            var result = await _azureTableStorageClient.DoDeleteAsync<NameCacheEntity, NameCacheTableEntity>(tableEntity, BuildNameCacheFromTableEntity);
            return (result.Status == TableStorageResponseStatus.Successful);
        }

        private void CheckSingleEntityType(NameCacheEntityType entityType)
        {
            if (entityType == NameCacheEntityType.DeviceInfo
                || entityType == NameCacheEntityType.Tag
                || entityType == NameCacheEntityType.DesiredProperty
                || entityType == NameCacheEntityType.ReportedProperty
                || entityType == NameCacheEntityType.Method)
                return;
            throw new ArgumentException("Can only pick up one of the flags: DeviceInfo, Tag, DesiredProperty, ReportedProperty, Method");
        }

        private NameCacheEntity BuildNameCacheFromTableEntity(NameCacheTableEntity tableEntity)
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

            var nameCacheEntity = new NameCacheEntity
            {
                Name = tableEntity?.Name,
                Parameters = parameters,
                Description = tableEntity?.MethodDescription,
            };

            return nameCacheEntity;
        }
    }
}
