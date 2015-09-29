using System.Threading.Tasks;

namespace Microsoft.Azure.Devices.Applications.RemoteMonitoring.DeviceAdmin.Infrastructure.Repository
{
    public interface IDeviceRegistryCrudRepository
    {
        /// <summary>
        /// Adds a device asynchronously.
        /// </summary>
        /// <param name="device">The device.</param>
        /// <returns></returns>
        Task<dynamic> AddDeviceAsync(dynamic device);

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
    }
}
