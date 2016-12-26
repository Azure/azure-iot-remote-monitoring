using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Azure.Devices.Applications.RemoteMonitoring.Common.Configurations;
using Microsoft.Azure.Devices.Applications.RemoteMonitoring.Common.Helpers;
using Microsoft.Azure.Devices.Applications.RemoteMonitoring.Common.Models;
using Microsoft.Azure.Devices.Applications.RemoteMonitoring.DeviceAdmin.Infrastructure.Exceptions;
using Microsoft.Azure.Devices.Applications.RemoteMonitoring.DeviceAdmin.Infrastructure.Models;
using Microsoft.WindowsAzure.Storage.Table;
using Newtonsoft.Json;

namespace Microsoft.Azure.Devices.Applications.RemoteMonitoring.DeviceAdmin.Infrastructure.Repository
{
    public class DeviceListFilterRepository : IDeviceListFilterRepository
    {
        private readonly string _storageAccountConnectionString;
        private readonly IAzureTableStorageClient _filterTableStorageClient;
        private readonly IAzureTableStorageClient _clauseTableStorageClient;
        private readonly string _filterTableName = "FilterList";
        private readonly string _clauseTableName = "SuggestedClausesList";
        private static bool DefaultFilterInitialized = false;
        public static readonly DeviceListFilter DefaultDeviceListFilter = new DeviceListFilter
        {
            Id = Guid.Empty.ToString(),
            Name = "All Devices",
            Clauses = new List<Clause>(),
            AdvancedClause = null,
            IsAdvanced = false,
            IsTemporary = false,
        };

        public DeviceListFilterRepository (IConfigurationProvider configurationProvider, IAzureTableStorageClientFactory filterTableStorageClientFactory, IAzureTableStorageClientFactory clausesTableStorageClientFactory)
        {
            _storageAccountConnectionString = configurationProvider.GetConfigurationSettingValue("device.StorageConnectionString");
            string filterTableName = configurationProvider.GetConfigurationSettingValueOrDefault("DeviceListFilterTableName", _filterTableName);
            _filterTableStorageClient = filterTableStorageClientFactory.CreateClient(_storageAccountConnectionString, filterTableName);
            string clauseTableName = configurationProvider.GetConfigurationSettingValueOrDefault("SuggestedClauseTableName", _clauseTableName);
            _clauseTableStorageClient = clausesTableStorageClientFactory.CreateClient(_storageAccountConnectionString, clauseTableName);
            InitializeDefaultFilter();
        }

        public async Task InitializeDefaultFilter()
        {
            if (!DefaultFilterInitialized)
            {
                DefaultFilterInitialized = true;
                await _filterTableStorageClient.DoTableInsertOrReplaceAsync(new DeviceListFilterTableEntity(DefaultDeviceListFilter) { ETag = "*" }, BuildFilterModelFromEntity);
            }
        }

        public async Task<bool> CheckFilterNameAsync(string name)
        {
            TableQuery<DeviceListFilterTableEntity> query = new TableQuery<DeviceListFilterTableEntity>().Where(TableQuery.GenerateFilterCondition("RowKey", QueryComparisons.Equal, name));
            var entities = await _filterTableStorageClient.ExecuteQueryAsync(query);
            return entities.Count() > 0;
        }

        public async Task<DeviceListFilter> GetFilterAsync(string id)
        {
            TableQuery<DeviceListFilterTableEntity> query = new TableQuery<DeviceListFilterTableEntity>().Where(TableQuery.GenerateFilterCondition("PartitionKey", QueryComparisons.Equal, id));
            var entities = await _filterTableStorageClient.ExecuteQueryAsync(query);
            if (!entities.Any()) return null;
            return new DeviceListFilter(entities.First());
        }

        public async Task<DeviceListFilter> SaveFilterAsync(DeviceListFilter filter, bool force = false)
        {
            var oldFilter = await GetFilterAsync(filter.Id);
            if (oldFilter == null) {
                filter.Id = Guid.NewGuid().ToString();
            }
            else
            {
                if (!force) return oldFilter;
            }

            DeviceListFilterTableEntity newEntity = new DeviceListFilterTableEntity(filter) { ETag = "*" };
            var result = await _filterTableStorageClient.DoTableInsertOrReplaceAsync(newEntity, BuildFilterModelFromEntity);

            if (result.Status == TableStorageResponseStatus.Successful)
            {
                // Safely delete old filter after the new renamed filter is saved successfully
                if (oldFilter != null && !oldFilter.Name.Equals(filter.Name, StringComparison.InvariantCulture))
                {
                    var oldEntity = new DeviceListFilterTableEntity(oldFilter) { ETag = "*" };
                    await _filterTableStorageClient.DoDeleteAsync(oldEntity, e => (object)null);
                }
                SaveSuggestClausesAsync(filter.Clauses);
                return await GetFilterAsync(filter.Id);
            }

            throw new FilterSaveException(filter.Id, filter.Name);
        }

        public async Task<bool> TouchFilterAsync(string id)
        {
            if (string.IsNullOrEmpty(id))
            {
                return false;
            }
            var filter = await GetFilterAsync(id);
            if (filter == null) return false;

            DeviceListFilterTableEntity entity = new DeviceListFilterTableEntity(id, filter.Name) { ETag = "*" };
            var result = await _filterTableStorageClient.DoTouchAsync(entity, BuildFilterModelFromEntity);
            return result.Status == TableStorageResponseStatus.Successful;
        }

        public async Task<bool> DeleteFilterAsync(string id)
        {
            var filter = await GetFilterAsync(id);
            // if the filter doesn't exist, return true idempotently just behave as it has been deleted successfully.
            if (filter == null) return true;

            DeviceListFilterTableEntity entity = new DeviceListFilterTableEntity(id, filter.Name);
            entity.ETag = "*";
            var result = await _filterTableStorageClient.DoDeleteAsync(entity, BuildFilterModelFromEntity);
            return (result.Status == TableStorageResponseStatus.Successful);
        }

        public async Task<IEnumerable<DeviceListFilter>> GetRecentFiltersAsync(int Max = 20)
        {
            TableQuery<DeviceListFilterTableEntity> query = new TableQuery<DeviceListFilterTableEntity>();
            var entities = await _filterTableStorageClient.ExecuteQueryAsync(query);
            // replace the timestamp of default filter with current time so that it is always sorted at top of filter list.
            var ordered = entities.Select(e => {
                if (e.Id.Equals(DefaultDeviceListFilter.Id)) {
                    e.Timestamp = DateTimeOffset.Now;
                }
                return e;
            }).OrderByDescending(e => e.Timestamp);
            if (Max > 0)
            {
                return ordered.Take(Max).Select(e => BuildFilterModelFromEntity(e));
            }
            else
            {
                return ordered.Select(e => BuildFilterModelFromEntity(e));
            }
        }

        public async Task<IEnumerable<DeviceListFilter>> GetFilterListAsync(int skip = 0, int take = 1000)
        {
            TableQuery<DeviceListFilterTableEntity> query = new TableQuery<DeviceListFilterTableEntity>();
            var entities = await _filterTableStorageClient.ExecuteQueryAsync(query);
            var ordered = entities.OrderBy(e => e.Name);
            if (take > 0)
            {
                return ordered.Skip(skip).Take(take).Select(e => BuildFilterModelFromEntity(e));
            }
            else
            {
                return ordered.Skip(skip).Select(e => BuildFilterModelFromEntity(e));
            }
        }

        public async Task<IEnumerable<Clause>> GetSuggestClausesAsync(int skip, int take)
        {
            TableQuery<ClauseTableEntity> query = new TableQuery<ClauseTableEntity>();
            var entities = await _clauseTableStorageClient.ExecuteQueryAsync(query);
            var ordered = entities.OrderByDescending(e => e.Timestamp);
            if (take > 0)
            {
                return ordered.Skip(skip).Take(take).Select(e => BuildClauseFromEntity(e));
            }
            else
            {
                return ordered.Skip(skip).Select(e => BuildClauseFromEntity(e));
            }
        }

        private async Task SaveSuggestClausesAsync(List<Clause> clauses)
        {
            var tasks = clauses.Select(async clause => {
                var operation = TableOperation.InsertOrReplace(new ClauseTableEntity(clause) { ETag = "*" });
                return await _clauseTableStorageClient.ExecuteAsync(operation);
            });

           await Task.WhenAll(tasks);
        }

        private DeviceListFilter BuildFilterModelFromEntity(DeviceListFilterTableEntity entity)
        {
            if (entity == null)
            {
                return null;
            }

            List<Clause> clauses = new List<Clause>();
            try
            {
                clauses = JsonConvert.DeserializeObject<List<Clause>>(entity.Clauses);
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

            return new DeviceListFilter (entity)
            {
                Clauses = clauses,
                SortOrder = order,
            };
        }

        private Clause BuildClauseFromEntity(ClauseTableEntity entity)
        {
            if (entity == null)
            {
                return null;
            }

            ClauseType clauseType;
            try
            {
                Enum.TryParse(entity.ClauseType, out clauseType);
            }
            catch
            {
                clauseType = ClauseType.EQ;
            }
            return new Clause
            {
                ColumnName = entity.ColumnName,
                ClauseType = clauseType,
                ClauseValue = entity.ClauseValue
            };
        }
    }
}
