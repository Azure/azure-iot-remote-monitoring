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

        public FilterLogic(IDeviceListFilterRepository filterRepository, IJobRepository jobRepository)
        {
            _filterRepository = filterRepository;
            _jobRepository = jobRepository;
        }

        public async Task<Filter> SaveFilterAsync(Filter filter)
        {
            if (!string.IsNullOrWhiteSpace(filter.Id))
            {
                var jobs = await _jobRepository.QueryByFilterIdAsync(filter.Id);
                if (jobs.Any())
                {
                    throw new FilterAssociatedWithJobException(filter.Name, jobs.Select(j => j.JobName).Distinct().Take(3));
                }
            }
            var result = await _filterRepository.SaveFilterAsync(new DeviceListFilter(filter), true);
            return new Filter(result);
        }

        public async Task<IEnumerable<Filter>> GetRecentFiltersAsync(int max)
        {
            var filters = await _filterRepository.GetRecentFiltersAsync(max);
            return filters.Select(filter => new Filter(filter));
        }

        public async Task<Filter> GetFilterAsync(string filterId)
        {
            var filter = await _filterRepository.GetFilterAsync(filterId);
            if (filter == null) throw new FilterNotFoundException(filterId);
            return new Filter(filter);
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

        public string GenerateAdvancedClause(IEnumerable<Clause> clauses)
        {
            return new DeviceListFilter { Clauses = clauses?.ToList() }.GetSQLQuery();
        }

        public async Task<bool> DeleteFilterAsync(string filterId)
        {
            var jobs = await _jobRepository.QueryByFilterIdAsync(filterId);
            if (jobs.Any())
            {
                throw new FilterAssociatedWithJobException(filterId, jobs.Select(j => j.JobName).Distinct().Take(3));
            }
            return await _filterRepository.DeleteFilterAsync(filterId);
        }

        public async Task<IEnumerable<Filter>> GetFilterList(int skip, int take)
        {
            var filters = await _filterRepository.GetFilterListAsync(skip, take);
            return filters.Select(f => new Filter
            {
                Id = f.Id,
                Name = f.Name,
                IsTemporary = false,
                Clauses = f.Clauses,
                AdvancedClause = f.AdvancedClause,
                IsAdvanced = f.IsAdvanced,
            });
        }

        public async Task<IEnumerable<Clause>> GetSuggestClauses(int skip, int take)
        {
            return await _filterRepository.GetSuggestClausesAsync(skip, take);
        }
    }
}
