using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Azure.Devices.Applications.RemoteMonitoring.Common.Models.Commands;
using Microsoft.Azure.Devices.Applications.RemoteMonitoring.DeviceAdmin.Infrastructure.Models;

namespace Microsoft.Azure.Devices.Applications.RemoteMonitoring.DeviceAdmin.Infrastructure.BusinessLogic
{
    public interface INameCacheLogic
    {
        string PREFIX_REPORTED { get; }
        string PREFIX_DESIRED { get; }
        string PREFIX_TAGS { get; }

        Task<IEnumerable<NameCacheEntity>> GetNameListAsync(NameCacheEntityType type);
        Task<bool> AddNameAsync(string name);
        Task AddShortNamesAsync(NameCacheEntityType type, IEnumerable<string> shortNames);
        Task<bool> AddMethodAsync(Command method);
        Task<bool> DeleteNameAsync(string name);
        Task<bool> DeleteMethodAsync(string name);
    }
}
