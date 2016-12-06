using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Azure.Devices.Applications.RemoteMonitoring.DeviceAdmin.Infrastructure.Models;

namespace Microsoft.Azure.Devices.Applications.RemoteMonitoring.DeviceAdmin.Infrastructure.Repository
{
    public interface IDeviceListFilterRepository
    {
        /// <summary>
        ///  Check if the named filter already exists (true) or not (false).
        /// </summary>
        /// <param name="name">unique name of the filter</param>
        /// <returns></returns>
        Task<bool> CheckFilterNameAsync(string name);

        /// <summary>
        /// update the timestamp when the named filter is executed.
        /// </summary>
        /// <param name="name"></param>
        /// <returns>true if updated</returns>
        Task<bool> TouchFilterAsync(string name);

        /// <summary>
        /// Add a new filter if not present, otherwise will update
        /// existing filter based on the name of the filter.
        /// </summary>
        /// <param name="filter"></param>
        /// <param name="force">true to force override existing filter</param>
        /// <returns>true if succeed</returns>
        Task<bool> SaveFilterAsync(DeviceListFilter filter, bool force);

        /// <summary>
        /// Get the filter by name.
        /// </summary>
        /// <param name="filterName"></param>
        /// <returns></returns>
        Task<DeviceListFilter> GetFilterAsync(string filterName);

        /// <summary>
        /// Return recenty queries executed recently, sorted by timestamp.
        /// </summary>
        /// <returns>a set of queries</returns>
        Task<IEnumerable<DeviceListFilter>> GetRecentFiltersAsync(int Max=20);

        /// <summary>
        /// Delete the saved filter by name.
        /// </summary>
        /// <param name="filterName"></param>
        /// <returns>true if succeed</returns>
        Task<bool> DeleteFilterAsync(string filterName);

        /// <summary>
        /// Get suggestion list of filter names
        /// </summary>
        /// <returns></returns>
        Task<IEnumerable<string>> GetFilterListAsync();
    }
}
