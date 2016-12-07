using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Azure.Devices.Applications.RemoteMonitoring.DeviceAdmin.Infrastructure.Models;

namespace Microsoft.Azure.Devices.Applications.RemoteMonitoring.DeviceAdmin.Infrastructure.BusinessLogic
{
    public interface IFilterLogic
    {
        Task<bool> SaveFilterAsync(Filter filter);
        Task<IEnumerable<Filter>> GetRecentFiltersAsync(int max);
        Task<Filter> GetFilterAsync(string filterId);
        Task<string> GetAvailableFilterNameAsync(string filterName);
        Task<bool> DeleteFilterAsync(string filterId);
        string GenerateAdvancedClause(IEnumerable<Clause> filters);
        Task<IEnumerable<Filter>> GetFilterList(int skip, int take);
    }
}
