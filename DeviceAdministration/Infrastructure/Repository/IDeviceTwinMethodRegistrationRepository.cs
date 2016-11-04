using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Azure.Devices.Applications.RemoteMonitoring.DeviceAdmin.Infrastructure.Models;

namespace Microsoft.Azure.Devices.Applications.RemoteMonitoring.DeviceAdmin.Infrastructure.Repository
{
    /// <summary>
    /// Class for working with persistence of Device Twin (Tag and Property)
    /// and Method list defined for all devices.
    /// </summary>
    public interface IDeviceTwinMethodRegistrationRepository
    {
        Task<IEnumerable<string>> GetAllDeviceTagNamesAsync();
        Task<bool> AddDeviceTagNameAsync(string name);
        Task<bool> DeleteDeviceTagNameAsync(string name);
        Task<IEnumerable<string>> GetAllDevicePropertyNamesAsync();
        Task<bool> AddDevicePropertyNameAsync(string name);
        Task<bool> DeleteDevicePropertyNameAsync(string name);
        Task<IEnumerable<DeviceMethod>> GetAllDeviceMethodsAsync();
        Task<bool> AddDeviceMethodAsync(DeviceMethod method);
        Task<bool> DeleteDeviceMethodAsync(DeviceMethod method);
    }
}
