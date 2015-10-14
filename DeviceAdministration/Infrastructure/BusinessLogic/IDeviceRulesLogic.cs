using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Azure.Devices.Applications.RemoteMonitoring.DeviceAdmin.Infrastructure.Models;
using Microsoft.Azure.Devices.Applications.RemoteMonitoring.Common.Models;

namespace Microsoft.Azure.Devices.Applications.RemoteMonitoring.DeviceAdmin.Infrastructure.BusinessLogic
{
    /// <summary>
    /// Logic class for retrieving, manipulating and persisting Device Rules
    /// </summary>
    public interface IDeviceRulesLogic
    {
        Task<List<DeviceRule>> GetAllRulesAsync();
        Task<DeviceRule> GetDeviceRuleOrDefaultAsync(string deviceId, string ruleId);
        Task<DeviceRule> GetDeviceRuleAsync(string deviceId, string ruleId);
        Task<TableStorageResponse<DeviceRule>> SaveDeviceRuleAsync(DeviceRule updatedRule);
        Task<DeviceRule> GetNewRuleAsync(string deviceId);
        Task<TableStorageResponse<DeviceRule>> UpdateDeviceRuleEnabledStateAsync(string deviceId, string ruleId, bool enabled);
        Task<Dictionary<string, List<string>>> GetAvailableFieldsForDeviceRuleAsync(string deviceId, string ruleId);
        Task<bool> CanNewRuleBeCreatedForDeviceAsync(string deviceId);
        Task BootstrapDefaultRulesAsync(List<string> existingDeviceIds);
        Task<TableStorageResponse<DeviceRule>> DeleteDeviceRuleAsync(string deviceId, string ruleId);
        Task<bool> RemoveAllRulesForDeviceAsync(string deviceId);
    }
}
