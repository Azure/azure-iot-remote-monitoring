using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Azure.Devices.Applications.RemoteMonitoring.DeviceAdmin.Infrastructure.Models;

namespace Microsoft.Azure.Devices.Applications.RemoteMonitoring.DeviceAdmin.Infrastructure.Repository
{
    public interface IDeviceListQueryRepository
    {
        /// <summary>
        ///  Check if the named query already exists (true) or not (false).
        /// </summary>
        /// <param name="name">unique name of the query</param>
        /// <returns></returns>
        Task<bool> CheckQueryNameAsync(string name);

        /// <summary>
        /// update the timestamp when the named query is executed.
        /// </summary>
        /// <param name="name"></param>
        /// <returns>true if updated</returns>
        Task<bool> TouchQueryAsync(string name);

        /// <summary>
        /// Add a new query if not present, otherwise will update
        /// existing query based on the name of the query.
        /// </summary>
        /// <param name="query"></param>
        /// <param name="force">true to force override existing query</param>
        /// <returns>true if succeed</returns>
        Task<bool> SaveQueryAsync(DeviceListQuery query, bool force);

        /// <summary>
        /// Get the query by name.
        /// </summary>
        /// <param name="queryName"></param>
        /// <returns></returns>
        Task<DeviceListQuery> GetQueryAsync(string queryName);

        /// <summary>
        /// Return recenty queries executed recently, sorted by timestamp.
        /// </summary>
        /// <returns>a set of queries</returns>
        Task<IEnumerable<DeviceListQuery>> GetRecentQueriesAsync(int Max=20);

        /// <summary>
        /// Delete the saved query by name.
        /// </summary>
        /// <param name="name"></param>
        /// <returns>true if succeed</returns>
        Task<bool> DeleteQueryAsync(string name);

        /// <summary>
        /// Get suggestion list of query names
        /// </summary>
        /// <returns></returns>
        Task<IEnumerable<string>> GetQueryNameListAsync();
    }
}
