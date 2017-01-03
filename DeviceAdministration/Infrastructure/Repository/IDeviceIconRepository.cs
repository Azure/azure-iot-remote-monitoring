using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using Microsoft.Azure.Devices.Applications.RemoteMonitoring.DeviceAdmin.Infrastructure.Models;

namespace Microsoft.Azure.Devices.Applications.RemoteMonitoring.DeviceAdmin.Infrastructure.Repository
{
    public interface IDeviceIconRepository
    {
        Task<DeviceIcon> AddIcon(string deviceId, string fileName, Stream fileStream);
        Task<DeviceIcon> GetIcon(string deviceId, string name);
        Task<IEnumerable<DeviceIcon>> GetIcons(string deviceId, int skip, int take);
        Task<DeviceIcon> SaveIcon(string deviceId, string name);
        Task<bool> DeleteIcon(string deviceId, string name);
    }
}
