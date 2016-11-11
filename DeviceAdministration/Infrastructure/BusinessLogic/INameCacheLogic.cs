using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Azure.Devices.Applications.RemoteMonitoring.Common.Models.Commands;
using Microsoft.Azure.Devices.Applications.RemoteMonitoring.DeviceAdmin.Infrastructure.Models;

namespace Microsoft.Azure.Devices.Applications.RemoteMonitoring.DeviceAdmin.Infrastructure.BusinessLogic
{
    public interface INameCacheLogic
    {
        Task<IEnumerable<NameCacheEntity>> GetNameListAsync(NameCacheEntityType type);
        Task<bool> AddNameAsync(string name);
        Task<bool> AddMethodAsync(Command method);
        Task<bool> DeleteNameAsync(string name);
        Task<bool> DeleteMethodAsync(string name);
    }
}
