using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Azure.Devices.Applications.RemoteMonitoring.Common.Models;

namespace Microsoft.Azure.Devices.Applications.RemoteMonitoring.Common.Repository
{
    public interface IVirtualDeviceStorage
    {
        /// <summary>
        /// Gets a list of stored virtual devices
        /// </summary>
        /// <returns>List of InitialDeviceConfig used for managing virtual devices</returns>
        Task<List<InitialDeviceConfig>> GetDeviceListAsync();

        /// <summary>
        /// Gets a specific virtual device from storage based on deviceId
        /// </summary>
        /// <param name="deviceId">The deviceId to search for</param>
        /// <returns>InitialDeviceConfig for deviceId or null if not found</returns>
        Task<InitialDeviceConfig> GetDeviceAsync(string deviceId);

        /// <summary>
        /// Adds or updates a virtual device in storage
        /// </summary>
        /// <param name="deviceConfig">The device config to add or update</param>
        /// <returns>thows if fails</returns>
        Task AddOrUpdateDeviceAsync(InitialDeviceConfig deviceConfig);

        /// <summary>
        /// Deletes a virtual device from storage
        /// </summary>
        /// <param name="deviceId">The deviceId to search for</param>
        /// <returns>true if successfully deleted, false if not found, throws if delete fails</returns>
        Task<bool> RemoveDeviceAsync(string deviceId);
    }
}
