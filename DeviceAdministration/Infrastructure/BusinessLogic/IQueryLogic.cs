using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Azure.Devices.Applications.RemoteMonitoring.DeviceAdmin.Infrastructure.Models;

namespace Microsoft.Azure.Devices.Applications.RemoteMonitoring.DeviceAdmin.Infrastructure.BusinessLogic
{
    public interface IQueryLogic
    {
        Task<bool> AddQueryAsync(Query query);
        Task<IEnumerable<Query>> GetRecentQueriesAsync(int max);
        Task<Query> GetQueryAsync(string queryName);
        Task<string> GetAvailableQueryNameAsync(string queryName);
        Task<bool> DeleteQueryAsync(string queryName);
        string GenerateSql(IEnumerable<FilterInfo> filters);
        Task<IEnumerable<string>> GetQueryNameList();
    }
}
