﻿using System.Threading.Tasks;
using Microsoft.Azure.Devices.Applications.RemoteMonitoring.Common.Models;

namespace Microsoft.Azure.Devices.Applications.RemoteMonitoring.DeviceAdmin.Infrastructure.Repository
{
    /// <summary>
    /// Interface to expose methods that can be called against the underlying identity repository
    /// </summary>
    public interface IIotHubRepository
    {
        Task<Azure.Devices.Device> GetIotHubDeviceAsync(string deviceId);
        Task<Common.Models.Device> AddDeviceAsync(Common.Models.Device device, SecurityKeys securityKeys);
        Task<bool> TryAddDeviceAsync(Azure.Devices.Device oldIotHubDevice);
        Task RemoveDeviceAsync(string deviceId);
        Task<bool> TryRemoveDeviceAsync(string deviceId);
        Task UpdateDeviceEnabledStatusAsync(string deviceId, bool isEnabled);
        Task SendCommand(string deviceId, CommandHistory command);
        Task<SecurityKeys> GetDeviceKeysAsync(string id);
    }
}
