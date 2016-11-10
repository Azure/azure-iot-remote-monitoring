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
        Task<IEnumerable<DeviceTwinMethodEntity>> GetNameListAsync(DeviceTwinMethodEntityType entityType);
        Task<bool> AddNameAsync(DeviceTwinMethodEntityType entityType, DeviceTwinMethodEntity entity);
        Task<bool> DeleteNameAsync(DeviceTwinMethodEntityType entityType, string name);
    }
}
