using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Azure.Devices.Applications.RemoteMonitoring.DeviceAdmin.Infrastructure.Models;

namespace Microsoft.Azure.Devices.Applications.RemoteMonitoring.DeviceAdmin.Infrastructure.BusinessLogic
{
    public interface IFilterLogic
    {
        Task<Filter> SaveFilterAsync(Filter filter);
        Task<IEnumerable<Filter>> GetRecentFiltersAsync(int max);
        Task<Filter> GetFilterAsync(string filterId);
        Task<string> GetAvailableFilterNameAsync(string filterName);
        Task<bool> DeleteFilterAsync(string filterId, bool force);
        string GenerateAdvancedClause(IEnumerable<Clause> filters);
        Task<IEnumerable<Filter>> GetFilterList(int skip, int take);
        Task<IEnumerable<Clause>> GetSuggestClauses(int skip, int take);
        Task<int> DeleteSuggestClausesAsync(IEnumerable<Clause> clauses);
    }
}
