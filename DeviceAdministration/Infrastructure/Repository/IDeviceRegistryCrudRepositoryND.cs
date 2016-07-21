using System.Threading.Tasks;
using Microsoft.Azure.Devices.Applications.RemoteMonitoring.Common.Models;

namespace Microsoft.Azure.Devices.Applications.RemoteMonitoring.DeviceAdmin.Infrastructure.Repository
{
    public interface IDeviceRegistryCrudRepositoryND
    {
        /// <summary>
        /// Adds a device asynchronously.
        /// </summary>
        /// <param name="device">The device.</param>
        /// <returns></returns>
        Task<dynamic> AddDeviceAsync(dynamic device);

        /// <summary>
        /// Adds a device asynchronously.
        /// </summary>
        /// <param name="device">The device.</param>
        /// <returns></returns>
        Task<DeviceND> AddDeviceAsyncND(DeviceND device);

        /// <summary>
        /// Removes a device asynchronously.
        /// </summary>
        /// <param name="deviceId">The device identifier.</param>
        /// <returns></returns>
        Task RemoveDeviceAsync(string deviceId);

        /// <summary>
        /// Gets a device asynchronously.
        /// </summary>
        /// <param name="deviceId">The device identifier.</param>
        /// <returns></returns>
        Task<dynamic> GetDeviceAsync(string deviceId);
        Task<DeviceND> GetDeviceAsyncND(string deviceId);

        /// <summary>
        /// Updates a device asynchronously.
        /// </summary>
        /// <param name="device">The device.</param>
        /// <returns></returns>
        Task<dynamic> UpdateDeviceAsync(dynamic device);

        /// <summary>
        /// Updates a device enabled/diabled status asynchronously.
        /// </summary>
        /// <param name="deviceId">The device identifier.</param>
        /// <param name="isEnabled">if set to <c>true</c> [is enabled].</param>
        /// <returns></returns>
        Task<dynamic> UpdateDeviceEnabledStatusAsync(string deviceId, bool isEnabled);
        Task<DeviceND> UpdateDeviceEnabledStatusAsyncND(string deviceId, bool isEnabled);
    }
}