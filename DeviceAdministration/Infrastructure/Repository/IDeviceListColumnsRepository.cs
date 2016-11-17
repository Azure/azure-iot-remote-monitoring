using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Azure.Devices.Applications.RemoteMonitoring.DeviceAdmin.Infrastructure.Models;

namespace Microsoft.Azure.Devices.Applications.RemoteMonitoring.DeviceAdmin.Infrastructure.Repository
{
    public interface IDeviceListColumnsRepository
    {
        Task<bool> SaveAsync(string userId, IEnumerable<DeviceListColumns> columns);

        Task<IEnumerable<DeviceListColumns>> GetAsync(string userId);
    }
}
