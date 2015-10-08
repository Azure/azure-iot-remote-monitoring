using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Azure.Devices.Applications.RemoteMonitoring.DeviceAdmin.Infrastructure.Models;
using Microsoft.Azure.Devices.Applications.RemoteMonitoring.Common.Models;

namespace Microsoft.Azure.Devices.Applications.RemoteMonitoring.DeviceAdmin.Infrastructure.Repository
{
    /// <summary>
    /// Class for working with persistence of Device Rules data
    /// </summary>
    public interface IDeviceRulesRepository
    {
        Task<List<DeviceRule>> GetAllRulesAsync();
        Task<DeviceRule> GetDeviceRuleAsync(string deviceId, string ruleId);
        Task<List<DeviceRule>> GetAllRulesForDeviceAsync(string deviceId);
        Task<TableStorageResponse<DeviceRule>> SaveDeviceRuleAsync(DeviceRule updatedRule);
        Task<TableStorageResponse<DeviceRule>> DeleteDeviceRuleAsync(DeviceRule ruleToDelete);
    }
}
