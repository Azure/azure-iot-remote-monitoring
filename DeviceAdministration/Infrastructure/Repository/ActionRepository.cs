using Newtonsoft.Json;
using System.Collections.Generic;
using System.Diagnostics;
using System.Net.Http;
using System.Threading.Tasks;

namespace Microsoft.Azure.Devices.Applications.RemoteMonitoring.DeviceAdmin.Infrastructure.Repository
{
    /// <summary>
    /// Repository storing available actions for rules.
    /// </summary>
    public class ActionRepository : IActionRepository
    {
        private readonly HttpMessageHandler _handler;

        public ActionRepository(HttpMessageHandler handler = null)
        {
            _handler = handler;
        }

        // Currently this dictionary is not editable in the app
        private Dictionary<string,string> actionIds = new Dictionary<string, string>()
         {
            { "Send Message", "" },
            { "Raise Alarm", "" }
      };

        public async Task<bool> AddActionEndpoint(string actionId, string endpoint)
        {
            return await Task.Run(() =>
            {
                if (actionIds.ContainsKey(actionId) && !string.IsNullOrEmpty(endpoint))
                {
                    actionIds[actionId] = endpoint;
                    return true;
                }
                else
                {
                    return false;
                }
            });
        }

        public async Task<List<string>> GetAllActionIdsAsync()
        {
            return await Task.Run(() => { return new List<string>(actionIds.Keys); });
        }

        public async Task<bool> ExecuteLogicAppAsync(string actionId, string deviceId, string measurementName, double measuredValue)
        {
            if(actionIds.ContainsKey(actionId) && !string.IsNullOrEmpty(actionIds[actionId]))
            {
                return await Task.Run(async () =>
                {
                    using (var client = _handler == null ? new HttpClient() : new HttpClient(_handler))
                    {
                        var response = await client.PostAsync(actionIds[actionId],
                            new StringContent(JsonConvert.SerializeObject(
                                new
                                {
                                    deviceId = deviceId,
                                    measurementName = measurementName,
                                    measuredValue = measuredValue
                                }),
                                 System.Text.Encoding.UTF8,
                                 "application/json"));
                        return response.IsSuccessStatusCode;
                    }
                });
            }
            else 
            {
                Trace.TraceWarning("ExecuteLogicAppAsync no event endpoint defined for actionId '{0}'", actionId); 
                return false;
            }
            
        }
    }
}
