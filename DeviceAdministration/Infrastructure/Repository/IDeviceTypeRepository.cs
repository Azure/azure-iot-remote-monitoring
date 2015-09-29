using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Azure.Devices.Applications.RemoteMonitoring.DeviceAdmin.Infrastructure.Models;

namespace Microsoft.Azure.Devices.Applications.RemoteMonitoring.DeviceAdmin.Infrastructure.Repository
{
    public interface IDeviceTypeRepository
    {
        Task<List<DeviceType>> GetAllDeviceTypesAsync();
        Task<DeviceType> GetDeviceTypeAsync(int deviceTypeId);
    }
}
