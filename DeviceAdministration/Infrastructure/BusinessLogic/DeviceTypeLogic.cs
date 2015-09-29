using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Azure.Devices.Applications.RemoteMonitoring.DeviceAdmin.Infrastructure.Models;
using Microsoft.Azure.Devices.Applications.RemoteMonitoring.DeviceAdmin.Infrastructure.Repository;

namespace Microsoft.Azure.Devices.Applications.RemoteMonitoring.DeviceAdmin.Infrastructure.BusinessLogic
{
    /// <summary>
    /// Business logic around different types of devices
    /// </summary>
    public class DeviceTypeLogic : IDeviceTypeLogic
    {
        private readonly IDeviceTypeRepository _deviceTypeRepository;

        public DeviceTypeLogic(IDeviceTypeRepository deviceTypeRepository)
        {
            _deviceTypeRepository = deviceTypeRepository;
        }

        /// <summary>
        /// Returns a list of all the device types available
        /// </summary>
        /// <returns></returns>
        public async Task<List<DeviceType>> GetAllDeviceTypesAsync()
        {
            return await _deviceTypeRepository.GetAllDeviceTypesAsync();
        }

        /// <summary>
        /// Returns the infromation for a single device type
        /// </summary>
        /// <param name="deviceTypeId"></param>
        /// <returns></returns>
        public async Task<DeviceType> GetDeviceTypeAsync(int deviceTypeId)
        {
            return await _deviceTypeRepository.GetDeviceTypeAsync(deviceTypeId);
        }
    }
}
