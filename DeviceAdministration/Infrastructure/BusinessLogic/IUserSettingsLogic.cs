using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Azure.Devices.Applications.RemoteMonitoring.DeviceAdmin.Infrastructure.Models;

namespace Microsoft.Azure.Devices.Applications.RemoteMonitoring.DeviceAdmin.Infrastructure.BusinessLogic
{
    public interface IUserSettingsLogic
    {
        Task<IEnumerable<DeviceListColumns>> GetDeviceListColumnsAsync(string userId);
        Task<IEnumerable<DeviceListColumns>> GetGlobalDeviceListColumnsAsync();
        Task<bool> SetDeviceListColumnsAsync(string userId, IEnumerable<DeviceListColumns> columns, bool saveAsGlobal = false);
    }
}