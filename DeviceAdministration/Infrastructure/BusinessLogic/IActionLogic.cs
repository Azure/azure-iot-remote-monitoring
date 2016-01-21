using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Azure.Devices.Applications.RemoteMonitoring.Common.Configurations;

namespace Microsoft.Azure.Devices.Applications.RemoteMonitoring.DeviceAdmin.Infrastructure.BusinessLogic
{
    public interface IActionLogic
    {
        Task<List<string>> GetAllActionIdsAsync();

        Task<bool> ExecuteLogicAppAsync(IConfigurationProvider configurationProvider, Guid eventToken, string actionId, string deviceId, string measurementName, double measuredValue);
    }
}
