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

        private static object _initializeLock = new object();
        private static bool DefaultFilterInitialized = false;

        private static readonly DeviceListFilter[] _builtInFilters = new DeviceListFilter[]
        {
            new DeviceListFilter
            {
                Id = "00000000-0000-0000-0000-000000000000",
                Name = "All Devices",
                Clauses = new List<Clause>()
            },
            new DeviceListFilter
            {
                Id = "00000000-0000-0000-0000-000000000001",
                Name = "Unhealthy devices",
                Clauses = new List<Clause>
                {
                    new Clause
                    {
                        ColumnName = "reported.Config.TemperatureMeanValue",
                        ClauseType = ClauseType.GT,
                        ClauseValue = "60",
                        ClauseDataType = TwinDataType.Number
                    }
                }
            },
            new DeviceListFilter
            {
                Id = "00000000-0000-0000-0000-000000000002",
                Name = "Old firmware devices",
                Clauses = new List<Clause>
                {
                    new Clause
                    {
                        ColumnName = "reported.System.FirmwareVersion",
                        ClauseType = ClauseType.LT,
                        ClauseValue = "2.0",
                        ClauseDataType = TwinDataType.String
                    }
                }
            },
        };

        public static readonly DeviceListFilter DefaultDeviceListFilter = _builtInFilters.First();

        public DeviceListFilterRepository(IConfigurationProvider configurationProvider, IAzureTableStorageClientFactory filterTableStorageClientFactory, IAzureTableStorageClientFactory clausesTableStorageClientFactory)
        {
            _storageAccountConnectionString = configurationProvider.GetConfigurationSettingValue("device.StorageConnectionString");
            string filterTableName = configurationProvider.GetConfigurationSettingValueOrDefault("DeviceListFilterTableName", _filterTableName);
            _filterTableStorageClient = filterTableStorageClientFactory.CreateClient(_storageAccountConnectionString, filterTableName);
            string clauseTableName = configurationProvider.GetConfigurationSettingValueOrDefault("SuggestedClauseTableName", _clauseTableName);
            _clauseTableStorageClient = clausesTableStorageClientFactory.CreateClient(_storageAccountConnectionString, clauseTableName);

            var task = InitializeDefaultFilter();
        }

        public async Task InitializeDefaultFilter()
        {
            // Ensure initializing will be performed only one time
            lock (_initializeLock)
            {
                if (!DefaultFilterInitialized)
                {
                    DefaultFilterInitialized = true;
                }
                else
                {
                    return;
                }
            }

            foreach (var filter in _builtInFilters)
            {
                await _filterTableStorageClient.DoTableInsertOrReplaceAsync(
                    new DeviceListFilterTableEntity(filter)
                    {
                        ETag = "*"
                    },
                    BuildFilterModelFromEntity);
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
            if (oldFilter == null)
            {
                filter.Id = Guid.NewGuid().ToString();
            }
            else
            {
                if (!force) return oldFilter;
            }

            if (filter.Name != Constants.UnnamedFilterName)
            {
                var query = new TableQuery<DeviceListFilterTableEntity>().Where(
                    TableQuery.CombineFilters(
                        TableQuery.GenerateFilterCondition("PartitionKey", QueryComparisons.NotEqual, filter.Id),
                        TableOperators.And,
                        TableQuery.GenerateFilterCondition("RowKey", QueryComparisons.Equal, filter.Name)
                    )
                );
                var entities = await _filterTableStorageClient.ExecuteQueryAsync(query);
                if (entities.Any())
                {
                    throw new FilterDuplicatedNameException(filter.Id, filter.Name);
                }
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

        public async Task<IEnumerable<DeviceListFilter>> GetRecentFiltersAsync(int Max = 20, bool excludeTemporary = true)
        {
            TableQuery<DeviceListFilterTableEntity> query = new TableQuery<DeviceListFilterTableEntity>();
            var entities = await _filterTableStorageClient.ExecuteQueryAsync(query);
            // replace the timestamp of default filter with current time so that it is always sorted at top of filter list.
            var ordered = entities.Where(e => !Constants.UnnamedFilterName.Equals(e.Name.Trim(), StringComparison.InvariantCultureIgnoreCase) && (!excludeTemporary || !e.IsTemporary))
                .Select(e =>
                {
                    if (e.Id.Equals(DefaultDeviceListFilter.Id))
                    {
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

        public async Task<IEnumerable<DeviceListFilter>> GetFilterListAsync(int skip = 0, int take = 1000, bool excludeTemporary = true)
        {
            TableQuery<DeviceListFilterTableEntity> query = new TableQuery<DeviceListFilterTableEntity>();
            var entities = await _filterTableStorageClient.ExecuteQueryAsync(query);
            var ordered = entities.Where(e => !Constants.UnnamedFilterName.Equals(e.Name.Trim(), StringComparison.InvariantCultureIgnoreCase) && (!excludeTemporary || !e.IsTemporary))
                .OrderBy(e => e.Name);
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
            var ordered = entities.OrderByDescending(e => e.HitCounter).ThenByDescending(e => e.Timestamp);
            if (take > 0)
            {
                return ordered.Skip(skip).Take(take).Select(e => BuildClauseFromEntity(e));
            }
            else
            {
                return ordered.Skip(skip).Select(e => BuildClauseFromEntity(e));
            }
        }

        public async Task<int> SaveSuggestClausesAsync(IEnumerable<Clause> clauses)
        {
            if (clauses == null || clauses.Count() == 0)
            {
                return 0;
            }

            var tasks = clauses.Select(async clause =>
            {
                var newClause = new ClauseTableEntity(clause) { ETag = "*" };
                TableQuery<ClauseTableEntity> query = new TableQuery<ClauseTableEntity>()
                    .Where(TableQuery.GenerateFilterCondition("PartitionKey", QueryComparisons.Equal, newClause.PartitionKey));
                var clauseEntities = await _clauseTableStorageClient.ExecuteQueryAsync(query);
                // There is limitation of scalability for this implementation to increase the hit count because
                // storage table don't have good support for concurrent write for the same entity.
                TableOperation operation;
                if (clauseEntities.Any())
                {
                    newClause.HitCounter = clauseEntities.First().HitCounter + 1;
                    operation = TableOperation.Replace(newClause);
                }
                else
                {
                    operation = TableOperation.Insert(newClause);
                }

                return await _clauseTableStorageClient.ExecuteAsync(operation);
            });

            return (await Task.WhenAll(tasks)).Count();
        }

        public async Task<int> DeleteSuggestClausesAsync(IEnumerable<Clause> clauses)
        {
            if (clauses == null || clauses.Count() == 0)
            {
                return 0;
            }

            var tasks = clauses.Select(async c =>
            {
                var entity = new ClauseTableEntity(c) { ETag = "*" };
                return await _clauseTableStorageClient.DoDeleteAsync(entity, BuildClauseFromEntity);
            });

            var results = await Task.WhenAll(tasks);
            return results.Where(r => r.Status == TableStorageResponseStatus.Successful).Count();
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

            return new DeviceListFilter(entity)
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

            TwinDataType clauseDataType;
            try
            {
                Enum.TryParse(entity.ClauseDataType, out clauseDataType);
            }
            catch
            {
                clauseDataType = TwinDataType.String;
            }

            return new Clause
            {
                ColumnName = entity.ColumnName,
                ClauseType = clauseType,
                ClauseValue = entity.ClauseValue,
                ClauseDataType = clauseDataType,
            };
        }
    }
}
