using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Azure.Devices.Applications.RemoteMonitoring.DeviceAdmin.Infrastructure.Models;
using Microsoft.Azure.Devices.Applications.RemoteMonitoring.Common.Configurations;
using Microsoft.Azure.Devices.Applications.RemoteMonitoring.Common.Helpers;
using Microsoft.WindowsAzure.Storage.Table;
using Microsoft.Azure.Devices.Applications.RemoteMonitoring.Common.Models;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace Microsoft.Azure.Devices.Applications.RemoteMonitoring.DeviceAdmin.Infrastructure.Repository
{
    public class DeviceListFilterRepository : IDeviceListFilterRepository
    {
        private readonly string _storageAccountConnectionString;
        private readonly IAzureTableStorageClient _azureTableStorageClient;
        private readonly string _filterTableName = "FilterList";
        private const string _filterTablePartitionKey = "filter";

        public DeviceListFilterRepository (IConfigurationProvider configurationProvider, IAzureTableStorageClientFactory tableStorageClientFactory)
        {
            _storageAccountConnectionString = configurationProvider.GetConfigurationSettingValue("device.StorageConnectionString");
            string filterTableName = configurationProvider.GetConfigurationSettingValueOrDefault("DeviceListFilterTableName", _filterTableName);
            _azureTableStorageClient = tableStorageClientFactory.CreateClient(_storageAccountConnectionString, filterTableName);
        }

        public async Task<bool> CheckFilterNameAsync(string name)
        {
            TableQuery<DeviceListFilterTableEntity> query = new TableQuery<DeviceListFilterTableEntity>().Where(TableQuery.GenerateFilterCondition("RowKey", QueryComparisons.Equal, name));
            var entities = await _azureTableStorageClient.ExecuteQueryAsync(query);
            return entities.Count() > 0;
        }

        public async Task<DeviceListFilter> GetFilterAsync(string name)
        {
            TableQuery<DeviceListFilterTableEntity> query = new TableQuery<DeviceListFilterTableEntity>().Where(TableQuery.GenerateFilterCondition("RowKey", QueryComparisons.Equal, name));
            var entities = await _azureTableStorageClient.ExecuteQueryAsync(query);
            if (entities.Count() == 0) return null;
            var entity = entities.First();
            return new DeviceListFilter
            {
                Name = entity.Name,
                Clauses = JsonConvert.DeserializeObject<List<Clause>>(entity.Clauses),
                AdvancedClause = entity.AdvancedClause,
                IsAdvanced = entity.IsAdvanced,
            };
        }

        public async Task<bool> SaveFilterAsync(DeviceListFilter filter, bool force = false)
        {
            // if force = false and query already exists, count>0
            if (!force && await CheckFilterNameAsync(filter.Name))
            {
                return false;
            }
            string filters = JsonConvert.SerializeObject(filter.Clauses, Formatting.None, new StringEnumConverter());
            DeviceListFilterTableEntity entity = new DeviceListFilterTableEntity(_filterTablePartitionKey, filter.Name);
            entity.ETag = "*";
            entity.Clauses = filters;
            entity.SortColumn = filter.SortColumn;
            entity.SortOrder = filter.SortOrder.ToString();
            entity.AdvancedClause = filter.AdvancedClause;
            entity.IsAdvanced = filter.IsAdvanced;
            var result = await _azureTableStorageClient.DoTableInsertOrReplaceAsync(entity, BuildFilterModelFromEntity);
            return (result.Status == TableStorageResponseStatus.Successful);
        }

        public async Task<bool> TouchFilterAsync(string name)
        {
            if (string.IsNullOrEmpty(name))
            {
                return false;
            }
            DeviceListFilterTableEntity entity = new DeviceListFilterTableEntity(_filterTablePartitionKey, name);
            var result = await _azureTableStorageClient.DoTouchAsync(entity, BuildFilterModelFromEntity);
            return result.Status == TableStorageResponseStatus.Successful;
        }

        public async Task<bool> DeleteFilterAsync(string name)
        {
            DeviceListFilterTableEntity entity = new DeviceListFilterTableEntity(_filterTablePartitionKey, name);
            entity.ETag = "*";
            var result = await _azureTableStorageClient.DoDeleteAsync(entity, BuildFilterModelFromEntity);
            return (result.Status == TableStorageResponseStatus.Successful);
        }

        public async Task<IEnumerable<DeviceListFilter>> GetRecentFiltersAsync(int Max = 20)
        {
            TableQuery<DeviceListFilterTableEntity> query = new TableQuery<DeviceListFilterTableEntity>();
            var entities = await _azureTableStorageClient.ExecuteQueryAsync(query);
            var ordered = entities.OrderByDescending(e => e.Timestamp);
            if (Max > 0)
            {
                return ordered.Take(Max).Select(e => BuildFilterModelFromEntity(e));
            }
            else
            {
                return ordered.Select(e => BuildFilterModelFromEntity(e));
            }
        }

        public async Task<IEnumerable<string>> GetFilterListAsync()
        {
            TableQuery<DeviceListFilterTableEntity> query = new TableQuery<DeviceListFilterTableEntity>();
            var entities = await _azureTableStorageClient.ExecuteQueryAsync(query);
            var ordered = entities.OrderBy(e => e.Name);
            return ordered.Select(e => e.Name);
        }

        private DeviceListFilter BuildFilterModelFromEntity(DeviceListFilterTableEntity entity)
        {
            if (entity == null)
            {
                return null;
            }

            List<Clause> filters = new List<Clause>();
            try
            {
                filters = JsonConvert.DeserializeObject<List<Clause>>(entity.Clauses);
            }
            catch (Exception)
            {
                Trace.TraceError("Can not deserialize filters to Json object", entity.Clauses);
            }

            QuerySortOrder order;
            if (!Enum.TryParse(entity.SortOrder, true, out order))
            {
                order = QuerySortOrder.Descending;
            }

            return new DeviceListFilter
            {
                Name = entity.Name,
                Clauses = filters,
                SortColumn = entity.SortColumn,
                SortOrder = order,
                AdvancedClause = entity.AdvancedClause,
                IsAdvanced = entity.IsAdvanced,
            };
        }
    }
}
