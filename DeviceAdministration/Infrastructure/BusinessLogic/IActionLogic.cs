using System.Collections.Generic;
using System.Threading.Tasks;

namespace Microsoft.Azure.Devices.Applications.RemoteMonitoring.DeviceAdmin.Infrastructure.BusinessLogic
{
    public interface IActionLogic
    {
        Task<List<string>> GetAllActionIdsAsync();

        Task<bool> ExecuteLogicAppAsync(string actionId, string deviceId, string measurementName, double measuredValue);
    }
}
