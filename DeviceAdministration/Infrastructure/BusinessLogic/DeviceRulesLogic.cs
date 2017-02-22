using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Azure.Devices.Applications.RemoteMonitoring.Common.Models;
using Microsoft.Azure.Devices.Applications.RemoteMonitoring.DeviceAdmin.Infrastructure.Models;
using Microsoft.Azure.Devices.Applications.RemoteMonitoring.DeviceAdmin.Infrastructure.Repository;

namespace Microsoft.Azure.Devices.Applications.RemoteMonitoring.DeviceAdmin.Infrastructure.BusinessLogic
{
    /// <summary>
    /// Logic class for retrieving, manipulating and persisting Device Rules
    /// </summary>
    public class DeviceRulesLogic : IDeviceRulesLogic
    {
        private readonly IDeviceRulesRepository _deviceRulesRepository;
        private readonly IActionMappingLogic _actionMappingLogic;

        public DeviceRulesLogic(IDeviceRulesRepository deviceRulesRepository, IActionMappingLogic actionMappingLogic)
        {
            _deviceRulesRepository = deviceRulesRepository;
            _actionMappingLogic = actionMappingLogic;
        }

        /// <summary>
        /// Retrieve the full list of Device Rules
        /// </summary>
        /// <returns></returns>
        public async Task<List<DeviceRule>> GetAllRulesAsync()
        {
            return await _deviceRulesRepository.GetAllRulesAsync();
        }

        /// <summary>
        /// Retrieve an existing rule for editing. If none is found then a default, bare-bones rule is returned for creating new
        /// A new rule is not persisted until it is saved. Distinct Rules are defined by the combination key of deviceID and ruleId
        /// 
        /// Use this method if you are not sure the desired rule exists
        /// </summary>
        /// <param name="deviceId"></param>
        /// <param name="ruleId"></param>
        /// <returns></returns>
        public async Task<DeviceRule> GetDeviceRuleOrDefaultAsync(string deviceId, string ruleId)
        {
            List<DeviceRule> rulesForDevice = await _deviceRulesRepository.GetAllRulesForDeviceAsync(deviceId);
            foreach (DeviceRule rule in rulesForDevice)
            {
                if (rule.RuleId == ruleId)
                {
                    return rule;
                }
            }

            var createdRule = new DeviceRule();
            createdRule.InitializeNewRule(deviceId);
            return createdRule;
        }

        /// <summary>
        /// Retrieve an existing rule for a device/ruleId pair. If a rule does not exist
        /// it will return null. This method is best used when you know the rule exists.
        /// </summary>
        /// <param name="deviceId"></param>
        /// <param name="ruleId"></param>
        /// <returns></returns>
        public async Task<DeviceRule> GetDeviceRuleAsync(string deviceId, string ruleId)
        {
            return await _deviceRulesRepository.GetDeviceRuleAsync(deviceId, ruleId);
        }

        /// <summary>
        /// Save a rule to the data store. This method should be used for new rules as well as updating existing rules
        /// </summary>
        /// <param name="updatedRule"></param>
        /// <returns></returns>
        public async Task<TableStorageResponse<DeviceRule>> SaveDeviceRuleAsync(DeviceRule updatedRule)
        {
            //Enforce single instance of a rule for a data field for a given device
            List<DeviceRule> foundForDevice = await _deviceRulesRepository.GetAllRulesForDeviceAsync(updatedRule.DeviceID);
            foreach (DeviceRule rule in foundForDevice)
            {
                if (rule.DataField == updatedRule.DataField && rule.RuleId != updatedRule.RuleId)
                {
                    var response = new TableStorageResponse<DeviceRule>();
                    response.Entity = rule;
                    response.Status = TableStorageResponseStatus.DuplicateInsert;

                    return response;
                }
            }

            return await _deviceRulesRepository.SaveDeviceRuleAsync(updatedRule);
        }

        /// <summary>
        /// Generate a new rule with bare-bones configuration. This new rule can then be conigured and sent
        /// back through the SaveDeviceRuleAsync method to persist.
        /// </summary>
        /// <param name="deviceId"></param>
        /// <returns></returns>
        public async Task<DeviceRule> GetNewRuleAsync(string deviceId)
        {
            return await Task.Run(() =>
            {
                var rule = new DeviceRule();
                rule.InitializeNewRule(deviceId);

                return rule;
            });
        }

        /// <summary>
        /// Updated the enabled state of a given rule. This method does not update any other properties on the rule
        /// </summary>
        /// <param name="deviceId"></param>
        /// <param name="ruleId"></param>
        /// <param name="enabled"></param>
        /// <returns></returns>
        public async Task<TableStorageResponse<DeviceRule>> UpdateDeviceRuleEnabledStateAsync(string deviceId, string ruleId, bool enabled)
        {
            DeviceRule found = await _deviceRulesRepository.GetDeviceRuleAsync(deviceId, ruleId);
            if (found == null)
            {
                var response = new TableStorageResponse<DeviceRule>();
                response.Entity = found;
                response.Status = TableStorageResponseStatus.NotFound;

                return response;
            }

            found.EnabledState = enabled;

            return await _deviceRulesRepository.SaveDeviceRuleAsync(found);
        }

        public async Task<Dictionary<string, List<string>>> GetAvailableFieldsForDeviceRuleAsync(string deviceId, string ruleId)
        {
            List<string> availableDataFields = DeviceRuleDataFields.GetListOfAvailableDataFields();
            List<string> operators = new List<string>() { ">" };
            List<string> ruleOutputs = await _actionMappingLogic.GetAvailableRuleOutputsAsync();

            Dictionary<string, List<string>> result = new Dictionary<string, List<string>>();
            result.Add("availableDataFields", availableDataFields);
            result.Add("availableOperators", operators);
            result.Add("availableRuleOutputs", ruleOutputs);

            return result;
        }

        public async Task<bool> CanNewRuleBeCreatedForDeviceAsync(string deviceId)
        {
            List<DeviceRule> existingRules = await _deviceRulesRepository.GetAllRulesForDeviceAsync(deviceId);
            List<string> availableDataFields = DeviceRuleDataFields.GetListOfAvailableDataFields();

            return existingRules.Count != availableDataFields.Count;
        }

        public async Task BootstrapDefaultRulesAsync(List<string> existingDeviceIds)
        {
            foreach (var deviceId in existingDeviceIds)
            {
                DeviceRule temperatureRule = await GetNewRuleAsync(deviceId);
                temperatureRule.DataField = DeviceRuleDataFields.Temperature;
                temperatureRule.RuleOutput = "AlarmTemp";
                temperatureRule.Threshold = 60.0d;
                await SaveDeviceRuleAsync(temperatureRule);

                DeviceRule humidityRule = await GetNewRuleAsync(deviceId);
                humidityRule.DataField = DeviceRuleDataFields.Humidity;
                humidityRule.RuleOutput = "AlarmHumidity";
                humidityRule.Threshold = 48.0d;
                await SaveDeviceRuleAsync(humidityRule);
            }
        }

        public async Task<TableStorageResponse<DeviceRule>> DeleteDeviceRuleAsync(string deviceId, string ruleId)
        {
            DeviceRule found = await _deviceRulesRepository.GetDeviceRuleAsync(deviceId, ruleId);
            if (found == null)
            {
                var response = new TableStorageResponse<DeviceRule>();
                response.Entity = found;
                response.Status = TableStorageResponseStatus.NotFound;

                return response;
            }

            return await _deviceRulesRepository.DeleteDeviceRuleAsync(found);
        }

        public async Task<bool> RemoveAllRulesForDeviceAsync(string deviceId)
        {
            bool result = true;

            List<DeviceRule> deviceRules = await _deviceRulesRepository.GetAllRulesForDeviceAsync(deviceId);
            foreach (DeviceRule rule in deviceRules)
            {
                TableStorageResponse<DeviceRule> response = await _deviceRulesRepository.DeleteDeviceRuleAsync(rule);
                if (response.Status != TableStorageResponseStatus.Successful)
                {
                    //Do nothing, just report that it failed. The client can then take other steps if needed/desired
                    result = false;
                }
            }

            return result;
        }
    }
}
