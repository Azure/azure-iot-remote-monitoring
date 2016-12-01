using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Azure.Devices.Applications.RemoteMonitoring.DeviceAdmin.Infrastructure.Exceptions;
using Microsoft.Azure.Devices.Applications.RemoteMonitoring.DeviceAdmin.Infrastructure.Models;
using Microsoft.Azure.Devices.Applications.RemoteMonitoring.DeviceAdmin.Infrastructure.Repository;

namespace Microsoft.Azure.Devices.Applications.RemoteMonitoring.DeviceAdmin.Infrastructure.BusinessLogic
{
    public class QueryLogic : IQueryLogic
    {
        public readonly IDeviceListQueryRepository _queryRepository;
        public readonly IJobRepository _jobRepository;
        private readonly int MaxRetryCount = 20;

        public QueryLogic(IDeviceListQueryRepository queryRepository, IJobRepository jobRepository)
        {
            _queryRepository = queryRepository;
            _jobRepository = jobRepository;
        }

        public async Task<bool> AddQueryAsync(Query query)
        {
            var jobs = await _jobRepository.QueryByQueryNameAsync(query.Name);
            if (jobs.Any())
            {
                throw new QueryAssociatedWithJobException(query.Name, jobs.Select(j => j.JobName).Distinct().Take(3));
            }
            DeviceListQuery deviceQuery = new DeviceListQuery
            {
                Name = query.Name,
                Filters = query.Filters,
                Sql = query.Sql,
                IsAdvanced = query.IsAdvanced,
            };
            return await _queryRepository.SaveQueryAsync(deviceQuery, true);
        }

        public async Task<IEnumerable<Query>> GetRecentQueriesAsync(int max)
        {
            var queries = await _queryRepository.GetRecentQueriesAsync(max);
            return queries.Select(q => new Query
            {
                Name = q.Name,
                Filters = q.Filters,
                Sql = q.Sql,
                IsTemporary = false,
                IsAdvanced = q.IsAdvanced,
            });
        }

        public async Task<Query> GetQueryAsync(string queryName)
        {
            var query = await _queryRepository.GetQueryAsync(queryName);
            if (query != null)
            {
                return new Query
                {
                    Name = query.Name,
                    IsTemporary = false,
                    Filters = query.Filters,
                    Sql = query.Sql,
                    IsAdvanced = query.IsAdvanced,
                };
            }

            throw new QueryNotFoundException(queryName);
        }

        public async Task<string> GetAvailableQueryNameAsync(string queryName = "MyNewQuery")
        {
            for (int i = 1; i <= MaxRetryCount; ++i)
            {
                string availableName = string.Format("{0}{1}", queryName, i);
                if (! await _queryRepository.CheckQueryNameAsync(availableName))
                {
                    return availableName;
                }
            }
            return queryName + DateTime.Now.ToString("MM-dd-hh-mm-dd-fffff");
        }

        public string GenerateSql(IEnumerable<FilterInfo> filters)
        {
            return new DeviceListQuery { Filters = filters?.ToList() }.GetSQLQuery();
        }

        public async Task<bool> DeleteQueryAsync(string queryName)
        {
            var jobs = await _jobRepository.QueryByQueryNameAsync(queryName);
            if (jobs.Any())
            {
                throw new QueryAssociatedWithJobException(queryName, jobs.Select(j => j.JobName).Distinct().Take(3));
            }
            return await _queryRepository.DeleteQueryAsync(queryName);
        }

        public async Task<IEnumerable<string>> GetQueryNameList()
        {
            return await _queryRepository.GetQueryNameListAsync();
        }

    }
}
