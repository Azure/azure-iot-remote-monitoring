using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Azure.Devices.Applications.RemoteMonitoring.DeviceAdmin.Infrastructure.Models;

namespace Microsoft.Azure.Devices.Applications.RemoteMonitoring.DeviceAdmin.Infrastructure.Repository
{
    /// <summary>
    /// Class for working with persistence of Device Info, Twin
    /// (Tag and Property) and Method list defined for all devices.
    /// </summary>
    public interface INameCacheRepository
    {
        Task<IEnumerable<NameCacheEntity>> GetNameListAsync(NameCacheEntityType entityType);
        Task<bool> AddNameAsync(NameCacheEntityType entityType, NameCacheEntity entity);
        Task<bool> DeleteNameAsync(NameCacheEntityType entityType, string name);
    }
}
