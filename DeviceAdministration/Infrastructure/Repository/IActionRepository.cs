using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Azure.Devices.Applications.RemoteMonitoring.Common.Configurations;

namespace Microsoft.Azure.Devices.Applications.RemoteMonitoring.DeviceAdmin.Infrastructure.Repository
{
    /// <summary>
    /// Represents a repository for actions in response to rules (actions are currently logic apps)
    /// </summary>
    public interface IActionRepository
    {
        Task<List<string>> GetAllActionIdsAsync();

        Task<bool> ExecuteLogicAppAsync(IConfigurationProvider configurationProvider, Guid eventToken, string actionId, string deviceId, string measurementName, double measuredValue);
    }
}
