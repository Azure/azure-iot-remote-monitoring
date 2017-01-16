using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using Microsoft.Azure.Devices.Applications.RemoteMonitoring.DeviceAdmin.Infrastructure.Models;

namespace Microsoft.Azure.Devices.Applications.RemoteMonitoring.DeviceAdmin.Infrastructure.Repository
{
    public interface IDeviceIconRepository
    {
        Task<DeviceIcon> AddIcon(string fileName, Stream fileStream);
        Task<DeviceIcon> GetIcon(string name);
        Task<DeviceIconResult> GetIcons(int skip, int take);
        Task<DeviceIcon> SaveIcon(string name);
        Task<bool> DeleteIcon(string name);
        Task<string> GetIconStorageUriPrefix();
    }
}
