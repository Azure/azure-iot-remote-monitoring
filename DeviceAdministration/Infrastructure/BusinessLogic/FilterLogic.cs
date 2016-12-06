using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Azure.Devices.Applications.RemoteMonitoring.DeviceAdmin.Infrastructure.Exceptions;
using Microsoft.Azure.Devices.Applications.RemoteMonitoring.DeviceAdmin.Infrastructure.Models;
using Microsoft.Azure.Devices.Applications.RemoteMonitoring.DeviceAdmin.Infrastructure.Repository;

namespace Microsoft.Azure.Devices.Applications.RemoteMonitoring.DeviceAdmin.Infrastructure.BusinessLogic
{
    public class FilterLogic : IFilterLogic
    {
        public readonly IDeviceListFilterRepository _filterRepository;
        public readonly IJobRepository _jobRepository;
        private readonly int MaxRetryCount = 20;

        public FilterLogic(IDeviceListFilterRepository queryRepository, IJobRepository jobRepository)
        {
            _filterRepository = queryRepository;
            _jobRepository = jobRepository;
        }

        public async Task<bool> AddFilterAsync(Filter filter)
        {
            var jobs = await _jobRepository.QueryByQueryNameAsync(filter.Name);
            if (jobs.Any())
            {
                throw new FilterAssociatedWithJobException(filter.Name, jobs.Select(j => j.JobName).Distinct().Take(3));
            }
            DeviceListFilter newFilter = new DeviceListFilter
            {
                Name = filter.Name,
                Clauses = filter.Clauses,
                AdvancedClause = filter.AdvancedClause,
                IsAdvanced = filter.IsAdvanced,
            };
            return await _filterRepository.SaveFilterAsync(newFilter, true);
        }

        public async Task<IEnumerable<Filter>> GetRecentFiltersAsync(int max)
        {
            var filters = await _filterRepository.GetRecentFiltersAsync(max);
            return filters.Select(q => new Filter
            {
                Name = q.Name,
                Clauses = q.Clauses,
                AdvancedClause = q.AdvancedClause,
                IsTemporary = false,
                IsAdvanced = q.IsAdvanced,
            });
        }

        public async Task<Filter> GetFilterAsync(string filterName)
        {
            var filter = await _filterRepository.GetFilterAsync(filterName);
            if (filter != null)
            {
                return new Filter
                {
                    Name = filter.Name,
                    IsTemporary = false,
                    Clauses = filter.Clauses,
                    AdvancedClause = filter.AdvancedClause,
                    IsAdvanced = filter.IsAdvanced,
                };
            }

            throw new FilterNotFoundException(filterName);
        }

        public async Task<string> GetAvailableFilterNameAsync(string filterName = "MyNewFilter")
        {
            for (int i = 1; i <= MaxRetryCount; ++i)
            {
                string availableName = string.Format("{0}{1}", filterName, i);
                if (! await _filterRepository.CheckFilterNameAsync(availableName))
                {
                    return availableName;
                }
            }
            return filterName + DateTime.Now.ToString("MM-dd-hh-mm-dd-fffff");
        }

        public string GenerateAdvancedClause(IEnumerable<Clause> filters)
        {
            return new DeviceListFilter { Clauses = filters?.ToList() }.GetSQLQuery();
        }

        public async Task<bool> DeleteFilterAsync(string filterName)
        {
            var jobs = await _jobRepository.QueryByQueryNameAsync(filterName);
            if (jobs.Any())
            {
                throw new FilterAssociatedWithJobException(filterName, jobs.Select(j => j.JobName).Distinct().Take(3));
            }
            return await _filterRepository.DeleteFilterAsync(filterName);
        }

        public async Task<IEnumerable<string>> GetFilterList()
        {
            return await _filterRepository.GetFilterListAsync();
        }

    }
}
