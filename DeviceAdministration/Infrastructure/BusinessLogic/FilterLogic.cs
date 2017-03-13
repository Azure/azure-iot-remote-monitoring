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
            var associatedJobs = new List<JobRepositoryModel>();
            if (!string.IsNullOrWhiteSpace(filter.Id))
            {
                associatedJobs = (await _jobRepository.QueryByFilterIdAsync(filter.Id)).ToList();
            }

            var savedFilter = await _filterRepository.SaveFilterAsync(new DeviceListFilter(filter), true);
            if (associatedJobs.Any())
            {
                associatedJobs.ForEach(j => j.FilterName = savedFilter.Name);
                var task = _jobRepository.UpdateAssociatedFilterNameAsync(associatedJobs);
            }

            return new Filter(savedFilter);
        }

        public async Task<IEnumerable<Filter>> GetRecentFiltersAsync(int max)
        {
            var filters = await _filterRepository.GetRecentFiltersAsync(max, true);
            return filters.Select(filter => new Filter(filter));
        }

        public async Task<Filter> GetFilterAsync(string filterId)
        {
            var filter = await _filterRepository.GetFilterAsync(filterId);
            if (filter == null) throw new FilterNotFoundException(filterId);
            var jobs = await _jobRepository.QueryByFilterIdAsync(filterId);
            return new Filter(filter) { AssociatedJobsCount = jobs.Count() };
        }

        // We define a constant "UnsavedFilterName" to replace this logic to generate
        // a default filter name in frontend. But this code will be kept here until
        // we surely do not need this logic.
        public async Task<string> GetAvailableFilterNameAsync(string filterName = "NewFilter")
        {
            for (int i = 1; i <= MaxRetryCount; ++i)
            {
                string availableName = string.Format("{0}{1}", filterName, i);
                if (!await _filterRepository.CheckFilterNameAsync(availableName))
                {
                    return availableName;
                }
            }
            return filterName + DateTime.Now.ToString("yyyy-MM-dd");
        }

        public string GenerateAdvancedClause(IEnumerable<Clause> clauses)
        {
            return new DeviceListFilter { Clauses = clauses?.ToList() }.GetSQLCondition();
        }

        public async Task<bool> DeleteFilterAsync(string filterId, bool force = false)
        {
            var associatedJobs = (await _jobRepository.QueryByFilterIdAsync(filterId)).ToList();
            if (associatedJobs.Any())
            {
                if (force)
                {
                    associatedJobs.ForEach(j => j.FilterName = string.Empty);
                    var task = _jobRepository.UpdateAssociatedFilterNameAsync(associatedJobs);
                }
                else
                {
                    return false;
                }
            }

            return await _filterRepository.DeleteFilterAsync(filterId);
        }

        public async Task<IEnumerable<Filter>> GetFilterList(int skip, int take)
        {
            var filters = await _filterRepository.GetFilterListAsync(skip, take, true);
            return filters.Select(f => new Filter(f));
        }

        public async Task<IEnumerable<Clause>> GetSuggestClauses(int skip, int take)
        {
            return await _filterRepository.GetSuggestClausesAsync(skip, take);
        }

        public async Task<int> DeleteSuggestClausesAsync(IEnumerable<Clause> clauses)
        {
            return await _filterRepository.DeleteSuggestClausesAsync(clauses);
        }
    }
}
