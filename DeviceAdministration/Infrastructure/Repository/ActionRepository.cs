using System.Collections.Generic;
using System.Diagnostics;
using System.Threading.Tasks;

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

        public async Task<bool> ExecuteLogicAppAsync(string actionId, string deviceId, string measurementName, double measuredValue)
        {
            Debug.WriteLine("ExecuteLogicAppAsync is not yet implemented");

            await Task.Run(() => { });
            return false;
        }
    }
}
