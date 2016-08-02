using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Azure.Devices.Applications.RemoteMonitoring.DeviceAdmin.Infrastructure.Models;

namespace Microsoft.Azure.Devices.Applications.RemoteMonitoring.DeviceAdmin.Infrastructure.BusinessLogic
{
    public interface IActionMappingLogic
    {
        Task<bool> IsInitializationNeededAsync();
        Task<bool> InitializeDataIfNecessaryAsync();
        Task<List<ActionMappingExtended>> GetAllMappingsAsync();
        Task<string> GetActionIdFromRuleOutputAsync(string ruleOutput);
        Task SaveMappingAsync(ActionMapping action);
        Task<List<string>> GetAvailableRuleOutputsAsync();
    }
}
