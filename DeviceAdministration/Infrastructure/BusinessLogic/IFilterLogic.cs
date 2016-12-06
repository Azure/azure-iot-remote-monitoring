using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Azure.Devices.Applications.RemoteMonitoring.DeviceAdmin.Infrastructure.Models;

namespace Microsoft.Azure.Devices.Applications.RemoteMonitoring.DeviceAdmin.Infrastructure.BusinessLogic
{
    public interface IFilterLogic
    {
        Task<bool> AddFilterAsync(Filter filter);
        Task<IEnumerable<Filter>> GetRecentFiltersAsync(int max);
        Task<Filter> GetFilterAsync(string filterName);
        Task<string> GetAvailableFilterNameAsync(string filterName);
        Task<bool> DeleteFilterAsync(string filterName);
        string GenerateAdvancedClause(IEnumerable<Clause> filters);
        Task<IEnumerable<string>> GetFilterList();
    }
}
