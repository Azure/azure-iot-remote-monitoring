using System.Net.Http;
using System.Threading.Tasks;
using System.Web.Http;
using Microsoft.Azure.Devices.Applications.RemoteMonitoring.DeviceAdmin.Infrastructure.BusinessLogic;
using Microsoft.Azure.Devices.Applications.RemoteMonitoring.DeviceAdmin.Infrastructure.Models;
using Microsoft.Azure.Devices.Applications.RemoteMonitoring.DeviceAdmin.Web.DataTables;
using Microsoft.Azure.Devices.Applications.RemoteMonitoring.DeviceAdmin.Web.Security;
using Newtonsoft.Json.Linq;
using Microsoft.Azure.Devices.Applications.RemoteMonitoring.Common.Models;

namespace Microsoft.Azure.Devices.Applications.RemoteMonitoring.DeviceAdmin.Web.WebApiControllers
{
    [RoutePrefix("api/v1/devicerules")]
    public class DeviceRulesApiController : WebApiControllerBase
    {
        private readonly IDeviceRulesLogic _deviceRulesLogic;

        public DeviceRulesApiController(IDeviceRulesLogic deviceRulesLogic)
        {
            this._deviceRulesLogic = deviceRulesLogic;
        }

        // GET: api/v1/devicerules
        //
        // This endpoint is used for apps and other platforms to get a list of device rules (whereas endpoint below is used by jQuery DataTables grid)
        //
        // See, for example: http://stackoverflow.com/questions/9981330/how-to-pass-an-array-of-integers-to-a-asp-net-web-api-rest-service
        // Example: api/v1/devicerules
        [HttpGet]
        [Route("")]
        [WebApiRequirePermission(Permission.ViewRules)]
        public async Task<HttpResponseMessage> GetDeviceRulesAsync()
        {
            return await GetServiceResponseAsync(async () => (await _deviceRulesLogic.GetAllRulesAsync()));
        }

        // POST: api/v1/devicerules/list
        // This endpoint is used by the jQuery DataTables grid to get data (and accepts an unusual data format based on that grid)
        [HttpPost]
        [Route("list")]
        [WebApiRequirePermission(Permission.ViewRules)]
        public async Task<HttpResponseMessage> GetDeviceRulesAsDataTablesResponseAsync()
        {
            return await GetServiceResponseAsync<DataTablesResponse<DeviceRule>>(async () =>
            {
                var queryResult = await _deviceRulesLogic.GetAllRulesAsync();

                var dataTablesResponse = new DataTablesResponse<DeviceRule>()
                {
                    RecordsTotal = queryResult.Count,
                    RecordsFiltered = queryResult.Count,
                    Data = queryResult.ToArray()
                };

                return dataTablesResponse;

            }, false);
        }

        // GET: api/v1/devicerules/{id}/{ruleId}
        //
        // This endpoint is used for apps and other platforms to get a specific device rule based on the deviceId and the dataField
        //
        // See, for example: http://stackoverflow.com/questions/9981330/how-to-pass-an-array-of-integers-to-a-asp-net-web-api-rest-service
        // Example: api/v1/devicerules/4/123
        [HttpGet]
        [Route("{deviceId}/{ruleId}")]
        [WebApiRequirePermission(Permission.ViewRules)]
        public async Task<HttpResponseMessage> GetDeviceRuleOrDefaultAsync(string deviceId, string ruleId)
        {
            return await GetServiceResponseAsync(async () => await _deviceRulesLogic.GetDeviceRuleOrDefaultAsync(deviceId, ruleId));
        }

        // GET: api/v1/devicerules/{id}/{ruleId}/availableFields
        //
        // This endpoint is used for apps and other platforms to get available fields for editing a device rule
        //
        // See, for example: http://stackoverflow.com/questions/9981330/how-to-pass-an-array-of-integers-to-a-asp-net-web-api-rest-service
        // Example: api/v1/devicerules/4/123/availableFields
        [HttpGet]
        [Route("{deviceId}/{ruleId}/availableFields")]
        [WebApiRequirePermission(Permission.ViewRules)]
        public async Task<HttpResponseMessage> GetAvailableFieldsForDeviceRuleAsync(string deviceId, string ruleId)
        {
            return await GetServiceResponseAsync(async () => await _deviceRulesLogic.GetAvailableFieldsForDeviceRuleAsync(deviceId, ruleId));
        }

        // POST: api/v1/devicerules
        [HttpPost]
        [Route("")]
        [WebApiRequirePermission(Permission.EditRules)]
        public async Task<HttpResponseMessage> SaveDeviceRuleAsync(DeviceRule updatedRule)
        {
            return await GetServiceResponseAsync<TableStorageResponse<DeviceRule>>(async () =>
            {
                return await _deviceRulesLogic.SaveDeviceRuleAsync(updatedRule);
            });
        }

        // GET: api/v1/devicerules/{id}
        //
        // This endpoint is used for apps and other platforms to get a new, mostly empty rule for a given device.
        // The user must then update and save the rule for it to be persisted
        //
        // See, for example: http://stackoverflow.com/questions/9981330/how-to-pass-an-array-of-integers-to-a-asp-net-web-api-rest-service
        // Example: api/v1/devicerules/4
        [HttpGet]
        [Route("{deviceId}")]
        [WebApiRequirePermission(Permission.EditRules)]
        public async Task<HttpResponseMessage> GetNewRuleAsync(string deviceId)
        {
            return await GetServiceResponseAsync(async () => await _deviceRulesLogic.GetNewRuleAsync(deviceId));
        }

        /// <summary>
        /// Update the enabled state for the given rule. No other properties are modified
        /// 
        /// PUT: api/v1/devicerules/2345/123/true
        /// </summary>
        /// <param name="deviceId"></param>
        /// <param name="ruleId"></param>
        /// <param name="enabled"></param>
        /// <returns></returns>
        [HttpPut]
        [Route("{deviceId}/{ruleId}/{enabled}")]
        [WebApiRequirePermission(Permission.EditRules)]
        public async Task<HttpResponseMessage> UpdateRuleEnabledStateAsync(string deviceId, string ruleId, bool enabled)
        {
            return await GetServiceResponseAsync<TableStorageResponse<DeviceRule>>(async () =>
            {
                return await _deviceRulesLogic.UpdateDeviceRuleEnabledStateAsync(deviceId, ruleId, enabled);
            });
        }

        /// <summary>
        /// Delete the given rule for the given device
        /// 
        /// Delete: api/v1/devicerules/2345/123
        /// </summary>
        /// <param name="deviceId"></param>
        /// <param name="ruleId"></param>
        /// <returns></returns>
        [HttpDelete]
        [Route("{deviceId}/{ruleId}")]
        [WebApiRequirePermission(Permission.DeleteRules)]
        public async Task<HttpResponseMessage> DeleteRuleAsync(string deviceId, string ruleId)
        {
            return await GetServiceResponseAsync<TableStorageResponse<DeviceRule>>(async () =>
            {
                return await _deviceRulesLogic.DeleteDeviceRuleAsync(deviceId, ruleId);
            });
        }
    }
}