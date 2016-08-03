using System.Collections.Generic;
using System.Threading.Tasks;

namespace Microsoft.Azure.Devices.Applications.RemoteMonitoring.DeviceAdmin.Infrastructure.Repository
{
    /// <summary>
    /// Represents a repository for actions in response to rules (actions are currently logic apps)
    /// </summary>
    public interface IActionRepository
    {
        Task<bool> AddActionEndpoint(string actionId, string endpoint);

        Task<List<string>> GetAllActionIdsAsync();

        Task<bool> ExecuteLogicAppAsync(string actionId, string deviceId, string measurementName, double measuredValue);
    }
}
