using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Azure.Devices.Applications.RemoteMonitoring.Common.Configurations;
using Microsoft.Azure.Devices.Applications.RemoteMonitoring.Common.Helpers;
using Microsoft.Azure.Devices.Applications.RemoteMonitoring.DeviceAdmin.Infrastructure.Models;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Table;

namespace Microsoft.Azure.Devices.Applications.RemoteMonitoring.DeviceAdmin.Infrastructure.Repository
{
    static public class RepositoryInitializer
    {
        static public async Task SeedTablesAsync()
        {
            var configurationProvider = new ConfigurationProvider();

            await ClearTableAsync(configurationProvider.GetConfigurationSettingValue("device.StorageConnectionString"), "QueryList");
            await ClearTableAsync(configurationProvider.GetConfigurationSettingValue("device.StorageConnectionString"), "JobTable");

            var queryRepository = new DeviceListQueryRepository(configurationProvider, new AzureTableStorageClientFactory());
            var jobRepository = new JobRepository(configurationProvider, new AzureTableStorageClientFactory());

            var deviceIdsByJobId = new Dictionary<string, HashSet<string>>();

            var registryManager = RegistryManager.CreateFromConnectionString(configurationProvider.GetConfigurationSettingValue("iotHub.ConnectionString"));
            var query = registryManager.CreateQuery("SELECT * FROM devices.jobs");
            while (query.HasMoreResults)
            {
                var jobs = await query.GetNextAsDeviceJobAsync();

                foreach (var job in jobs)
                {
                    HashSet<string> deviceIds;
                    if (!deviceIdsByJobId.TryGetValue(job.JobId, out deviceIds))
                    {
                        deviceIds = new HashSet<string>();
                        deviceIdsByJobId.Add(job.JobId, deviceIds);
                    }
                    deviceIds.Add(job.DeviceId);
                }
            }

            var groups = deviceIdsByJobId.GroupBy(pair => pair.Value.Select(id => id.GetHashCode()).Aggregate((h1, h2) => h1 ^ h2));
            foreach (var group in groups)
            {
                var deviceIds = group.First().Value.OrderBy(s => s);

                var queryModel = new DeviceListQuery
                {
                    Name = $"Query for {string.Join(", ", deviceIds)}",
                    Filters = new List<FilterInfo>(),
                    SortColumn = "DeviceID",
                    SortOrder = QuerySortOrder.Ascending,
                    Sql = $"SELECT * FROM devices WHERE deviceId IN [{string.Join(", ", deviceIds.Select(id => $"'{id}'"))}]"
                };

                await queryRepository.SaveQueryAsync(queryModel);

                foreach (var job in group)
                {
                    string jobName = job.Key.Length > 3 ?
                        job.Key.Substring(job.Key.Length - 3) :
                        job.Key;

                    var jobModel = new JobRepositoryModel(job.Key, queryModel.Name, $"job-{jobName}");

                    await jobRepository.AddAsync(jobModel);
                }
            }
        }

        static private async Task ClearTableAsync(string connectionString, string tableName)
        {
            var account = CloudStorageAccount.Parse(connectionString);
            var client = account.CreateCloudTableClient();
            var table = client.GetTableReference(tableName);

            if (!(await table.ExistsAsync()))
            {
                return;
            }

            foreach (var entity in table.ExecuteQuery(new TableQuery()))
            {
                await table.ExecuteAsync(TableOperation.Delete(entity));
            }
        }
    }
}
