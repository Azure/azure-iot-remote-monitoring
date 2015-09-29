using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Azure.Devices.Applications.RemoteMonitoring.DeviceAdmin.Infrastructure.Repository;

namespace Microsoft.Azure.Devices.Applications.RemoteMonitoring.DeviceAdmin.Infrastructure.BusinessLogic
{
    public class ActionLogic : IActionLogic
    {
        private readonly IActionRepository _actionRepository;

        public ActionLogic(IActionRepository actionRepository)
        {
            _actionRepository = actionRepository;
        }

        public async Task<List<string>> GetAllActionIdsAsync()
        {
            return await _actionRepository.GetAllActionIdsAsync();
        }

        public async Task<bool> ExecuteLogicAppAsync(string actionId, string deviceId, string measurementName, double measuredValue)
        {
            if (string.IsNullOrWhiteSpace(actionId))
            {
                throw new ArgumentException("actionId cannot be null or whitespace");
            }

            if (string.IsNullOrWhiteSpace(deviceId))
            {
                throw new ArgumentException("deviceId cannot be null or whitespace");
            }

            // check that the actionId is valid!
            var validActionIds = await GetAllActionIdsAsync();
            if (!validActionIds.Contains(actionId))
            {
                throw new ArgumentException("actionId must be a valid ActionId value");
            }

            return await _actionRepository.ExecuteLogicAppAsync(actionId, deviceId, measurementName, measuredValue);
        }
    }
}
