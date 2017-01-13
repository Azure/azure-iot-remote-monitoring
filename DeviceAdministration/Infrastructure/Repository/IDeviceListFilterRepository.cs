using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Azure.Devices.Applications.RemoteMonitoring.DeviceAdmin.Infrastructure.Models;

namespace Microsoft.Azure.Devices.Applications.RemoteMonitoring.DeviceAdmin.Infrastructure.Repository
{
    public interface IDeviceListFilterRepository
    {
        /// <summary>
        /// Initialze default filter to get all devices.
        /// </summary>
        /// <returns></returns>
        Task InitializeDefaultFilter();

        /// <summary>
        ///  Check if the named filter already exists (true) or not (false).
        /// </summary>
        /// <param name="name">unique name of the filter</param>
        /// <returns></returns>
        Task<bool> CheckFilterNameAsync(string name);

        /// <summary>
        /// update the timestamp when the filter is executed.
        /// </summary>
        /// <param name="id"></param>
        /// <returns>true if updated</returns>
        Task<bool> TouchFilterAsync(string id);

        /// <summary>
        /// Add a new filter if not present, otherwise will update
        /// existing filter based on the name of the filter.
        /// </summary>
        /// <param name="filter"></param>
        /// <param name="force">true to force override existing filter</param>
        /// <returns>true if succeed</returns>
        Task<DeviceListFilter> SaveFilterAsync(DeviceListFilter filter, bool force);

        /// <summary>
        /// Get the filter by id.
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        Task<DeviceListFilter> GetFilterAsync(string id);

        /// <summary>
        /// Return recenty queries executed recently, sorted by timestamp.
        /// </summary>
        /// <returns>a set of queries</returns>
        Task<IEnumerable<DeviceListFilter>> GetRecentFiltersAsync(int Max, bool excludeTemporary);

        /// <summary>
        /// Delete the saved filter by name.
        /// </summary>
        /// <param name="id"></param>
        /// <returns>true if succeed</returns>
        Task<bool> DeleteFilterAsync(string id);

        /// <summary>
        /// Get suggestion list of filter names
        /// </summary>
        /// <returns></returns>
        Task<IEnumerable<DeviceListFilter>> GetFilterListAsync(int skip, int take, bool excludeTemporary);

        /// <summary>
        /// Save suggestion list of clauses
        /// </summary>
        /// <param name="clauses"></param>
        /// <returns>Count of saved clauses</returns>
        Task<int> SaveSuggestClausesAsync(IEnumerable<Clause> clauses);

        /// <summary>
        /// Get suggestion list of clauses extracted from filters
        /// </summary>
        /// <param name="skip"></param>
        /// <param name="take"></param>
        /// <returns></returns>
        Task<IEnumerable<Clause>> GetSuggestClausesAsync(int skip, int take);

        /// <summary>
        /// Delete the suggestion list of clauses that have been persistented in storage.
        /// </summary>
        /// <param name="clauses"></param>
        /// <returns>Count of deleted clauses</returns>
        Task<int> DeleteSuggestClausesAsync(IEnumerable<Clause> clauses);
    }
}
