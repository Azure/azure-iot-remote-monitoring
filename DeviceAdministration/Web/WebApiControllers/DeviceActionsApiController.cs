using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web.Http;
using Microsoft.Azure.Devices.Applications.RemoteMonitoring.DeviceAdmin.Infrastructure.BusinessLogic;
using Microsoft.Azure.Devices.Applications.RemoteMonitoring.DeviceAdmin.Infrastructure.Models;
using Microsoft.Azure.Devices.Applications.RemoteMonitoring.DeviceAdmin.Web.DataTables;
using Microsoft.Azure.Devices.Applications.RemoteMonitoring.DeviceAdmin.Web.Security;
using Newtonsoft.Json.Linq;

namespace Microsoft.Azure.Devices.Applications.RemoteMonitoring.DeviceAdmin.Web.WebApiControllers
{
    [RoutePrefix("api/v1/actions")]
    public class DeviceActionsApiController : WebApiControllerBase
    {
        private IActionMappingLogic _actionMappingLogic;

        public DeviceActionsApiController(IActionMappingLogic actionMappingLogic)
        {
            this._actionMappingLogic = actionMappingLogic;
        }

        [HttpGet]
        [Route("")]
        [WebApiRequirePermission(Permission.ViewActions)]
        public async Task<HttpResponseMessage> GetDeviceActionsAsync()
        {
            return await GetServiceResponseAsync(async () => (await _actionMappingLogic.GetAllMappingsAsync()));
        }

        // POST: api/v1/actions/list
        // This endpoint is used by the jQuery DataTables grid to get data (and accepts an unusual data format based on that grid)
        [HttpPost]
        [Route("list")]
        [WebApiRequirePermission(Permission.ViewActions)]
        public async Task<HttpResponseMessage> GetDeviceActionsAsDataTablesResponseAsync()
        {
            return await GetServiceResponseAsync<DataTablesResponse<ActionMappingExtended>>(async () =>
            {
                List<ActionMappingExtended> queryResult = await _actionMappingLogic.GetAllMappingsAsync();

                var dataTablesResponse = new DataTablesResponse<ActionMappingExtended>()
                {
                    RecordsTotal = queryResult.Count,
                    RecordsFiltered = queryResult.Count,
                    Data = queryResult.ToArray()
                };

                return dataTablesResponse;

            }, false);
        }

        [HttpPut]
        [Route("update")]
        [WebApiRequirePermission(Permission.AssignAction)]
        public async Task<HttpResponseMessage> UpdateActionAsync(string ruleOutput, string actionId)
        {
            var actionMapping = new ActionMapping();
            actionMapping.RuleOutput = ruleOutput;
            actionMapping.ActionId = actionId;

            return await GetServiceResponseAsync(async () =>
            {
                await _actionMappingLogic.SaveMappingAsync(actionMapping);
            });
        }

        [HttpGet]
        [Route("ruleoutputs/{ruleoutput}")]
        [WebApiRequirePermission(Permission.ViewActions)]
        public async Task<HttpResponseMessage> GetActionIdFromRuleOutputAsync(string ruleOutput)
        {
            return await GetServiceResponseAsync(async () => (await _actionMappingLogic.GetActionIdFromRuleOutputAsync(ruleOutput)));
        }

        [HttpGet]
        [Route("ruleoutputs")]
        [WebApiRequirePermission(Permission.ViewActions)]
        public async Task<HttpResponseMessage> GetAvailableRuleOutputsAsync()
        {
            return await GetServiceResponseAsync(async () => await _actionMappingLogic.GetAvailableRuleOutputsAsync());
        }
    }
}