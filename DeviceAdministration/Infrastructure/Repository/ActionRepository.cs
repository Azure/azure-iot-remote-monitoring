using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading.Tasks;

using Microsoft.Azure.Devices.Applications.RemoteMonitoring.Common.Configurations;
using Microsoft.Crm.Sdk.Helper;

namespace Microsoft.Azure.Devices.Applications.RemoteMonitoring.DeviceAdmin.Infrastructure.Repository
{
    /// <summary>
    /// Repository storing available actions for rules.
    /// </summary>
    public class ActionRepository : IActionRepository
    {
        // Currently this list is not editable in the app
        private List<string> _actionIds = new List<string>()
        {
            "Send Message",
            "Raise Alarm"
        };

        public async Task<List<string>> GetAllActionIdsAsync()
        {
            return await Task.Run(() => { return _actionIds; });
        }

        public async Task<bool> ExecuteLogicAppAsync(IConfigurationProvider configurationProvider, Guid eventToken, string actionId, string deviceId, string measurementName, double measuredValue)
        {
            Debug.WriteLine("Writing alert to CRM");

            await Task.Run(() =>
            {
                CrmActionProcessor.CreateServiceAlert(configurationProvider, eventToken, deviceId, actionId);
            });
            return false;
        }
    }
}
