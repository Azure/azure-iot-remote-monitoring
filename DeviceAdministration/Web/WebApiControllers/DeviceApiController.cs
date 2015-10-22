using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web.Http;
using Microsoft.Azure.Devices.Applications.RemoteMonitoring.Common.DeviceSchema;
using Microsoft.Azure.Devices.Applications.RemoteMonitoring.Common.Models;
using Microsoft.Azure.Devices.Applications.RemoteMonitoring.DeviceAdmin.Infrastructure.BusinessLogic;
using Microsoft.Azure.Devices.Applications.RemoteMonitoring.DeviceAdmin.Infrastructure.Models;
using Microsoft.Azure.Devices.Applications.RemoteMonitoring.DeviceAdmin.Web.DataTables;
using Microsoft.Azure.Devices.Applications.RemoteMonitoring.DeviceAdmin.Web.Security;
using Newtonsoft.Json.Linq;

namespace Microsoft.Azure.Devices.Applications.RemoteMonitoring.DeviceAdmin.Web.WebApiControllers
{
    [RoutePrefix("api/v1/devices")]
    public class DeviceApiController : WebApiControllerBase
    {
        private IDeviceLogic _deviceLogic;

        public DeviceApiController(IDeviceLogic deviceLogic)
        {
            _deviceLogic = deviceLogic;
        }

        // POST: api/v1/devices/sample/5
        [HttpPost]
        [Route("sample/{count}")]
        [WebApiRequirePermission(Permission.AddDevices)]
        public async Task<HttpResponseMessage> GenerateSampleDevicesAsync(int count)
        {
            ValidatePositiveValue("count", count);

            return await GetServiceResponseAsync(async () =>
            {
                await _deviceLogic.GenerateNDevices(count);
                return true;
            });
        }

        // GET: api/v1/devices/5
        [HttpGet]
        [Route("{id}")]
        [WebApiRequirePermission(Permission.ViewDevices)]
        public async Task<HttpResponseMessage> GetDeviceAsync(string id)
        {
            ValidateArgumentNotNullOrWhitespace("id", id);

            return await GetServiceResponseAsync<dynamic>(async () =>
            {
                return await _deviceLogic.GetDeviceAsync(id);
            });
        }

        // GET: api/v1/devices
        //
        // This endpoint is used for apps and other platforms to get a list of devices (whereas endpoint below is used by jQuery DataTables grid)
        // Filter arrays use the following syntax: ?test=1&test=2&test=3 for [FromUri] int[] test
        //
        // See, for example: http://stackoverflow.com/questions/9981330/how-to-pass-an-array-of-integers-to-a-asp-net-web-api-rest-service
        // Example: api/v1/devices?filterColumn=DeviceID&filterType=StartsWithCaseSensitive&filterValue=000&filterColumn=FirmwareVersion&filterType=ContainsCaseInsensitive&filterValue=564
        [HttpGet]
        [Route("")]
        [WebApiRequirePermission(Permission.ViewDevices)]
        public async Task<HttpResponseMessage> GetDevicesAsync(
            [FromUri]string search = null,
            [FromUri]string sortColumn = null,
            [FromUri]QuerySortOrder sortOrder = QuerySortOrder.Ascending,
            [FromUri]int skip = 0,
            [FromUri]int take = 50,
            [FromUri]string[] filterColumn = null,
            [FromUri]FilterType[] filterType = null,
            [FromUri]string[] filterValue = null)
        {
            var filters = new List<FilterInfo>();
            if (filterColumn != null && filterType != null && filterValue != null)
            {
                // valid filters must send ALL three values--ignore unmatched extras
                int validFiltersCount = Math.Min(filterColumn.Length, Math.Min(filterType.Length, filterValue.Length));
                for (int i = 0; i < validFiltersCount; ++i)
                {
                    var f = new FilterInfo()
                    {
                        ColumnName = filterColumn[i],
                        FilterType = filterType[i],
                        FilterValue = filterValue[i]
                    };

                    filters.Add(f);
                }
            }

            var q = new DeviceListQuery()
            {
                SearchQuery = search,
                SortColumn = sortColumn,
                SortOrder = sortOrder,
                Skip = skip,
                Take = take,
                Filters = filters
            };

            return await GetServiceResponseAsync(async () => (await _deviceLogic.GetDevices(q)).Results);
        }

        // POST: api/v1/devices/list
        // This endpoint is used by the jQuery DataTables grid to get data (and accepts an unusual data format based on that grid)
        [HttpPost]
        [Route("list")]
        [WebApiRequirePermission(Permission.ViewDevices)]
        public async Task<HttpResponseMessage> GetDevices([FromBody]JObject requestData)
        {
            return await GetServiceResponseAsync<DataTablesResponse>(async () =>
            {
                var dataTableRequest = requestData.ToObject<DataTablesRequest>();
                var sortColumnIndex = dataTableRequest.SortColumns[0].ColumnIndex;

                var listQuery = new DeviceListQuery()
                {
                    SortOrder = dataTableRequest.SortColumns[0].SortOrder,
                    SortColumn = dataTableRequest.Columns[sortColumnIndex].Name,

                    SearchQuery = dataTableRequest.Search.Value,

                    Filters = dataTableRequest.Filters,

                    Skip = dataTableRequest.Start,
                    Take = dataTableRequest.Length
                };

                var queryResult = await _deviceLogic.GetDevices(listQuery);

                var dataTablesResponse = new DataTablesResponse()
                {
                    Draw = dataTableRequest.Draw,
                    RecordsTotal = queryResult.TotalDeviceCount,
                    RecordsFiltered = queryResult.TotalFilteredCount,
                    Data = queryResult.Results.ToArray()
                };

                return dataTablesResponse;

            }, false);
        }

        // DELETE: api/v1/devices/5
        [HttpDelete]
        [Route("{id}")]
        [WebApiRequirePermission(Permission.RemoveDevices)]
        public async Task<HttpResponseMessage> RemoveDeviceAsync(string id)
        {
            ValidateArgumentNotNullOrWhitespace("id", id);

            return await GetServiceResponseAsync(async () =>
            {
                await _deviceLogic.RemoveDeviceAsync(id);
                return true;
            });
        }

        // POST: api/v1/devices
        [HttpPost]
        [Route("")]
        [WebApiRequirePermission(Permission.AddDevices)]
        public async Task<HttpResponseMessage> AddDeviceAsync(dynamic device)
        {
            ValidateArgumentNotNull("device", device);

            return await GetServiceResponseAsync<DeviceWithKeys>(async () => 
            { 
                return await _deviceLogic.AddDeviceAsync(device);
            });
        }

        //PUT: api/v1/devices
        [HttpPut]
        [Route("")]
        [WebApiRequirePermission(Permission.EditDeviceMetadata)]
        public async Task<HttpResponseMessage> UpdateDeviceAsync(dynamic device)
        {
            ValidateArgumentNotNull("device", device);

            return await GetServiceResponseAsync<bool>(async () =>
            {
                await _deviceLogic.UpdateDeviceAsync(device);
                return true;
            });
        }

        //GET: api/v1/devices/5/hub-keys
        [HttpGet]
		[Route("{id}/hub-keys")]
        [WebApiRequirePermission(Permission.ViewDeviceSecurityKeys)]
        public async Task<HttpResponseMessage> GetDeviceKeysAsync(string id)
        {
            ValidateArgumentNotNullOrWhitespace("id", id);

            return await GetServiceResponseAsync<SecurityKeys>(async () =>
            {
                return await _deviceLogic.GetIoTHubKeysAsync(id);
            });
        }

        //PUT: api/v1/devices/5/enabledstatus
        [HttpPut]
        [Route("{deviceId}/enabledstatus")]
        [WebApiRequirePermission(Permission.DisableEnableDevices)]
        public async Task<HttpResponseMessage> UpdateDeviceEnabledStatus(string deviceId, [FromBody]JObject request)
        {
            bool isEnabled;

            ValidateArgumentNotNullOrWhitespace("deviceId", deviceId);

            if (request == null)
                return GetNullRequestErrorResponse<bool>();

            try
            {
                var property = request.Property("isEnabled");

                if (property == null) 
                {
                    return GetFormatErrorResponse<bool>("isEnabled", "bool");
                }

                isEnabled = request.Value<bool>("isEnabled");
            }
            catch (Exception)
            {
                return GetFormatErrorResponse<bool>("isEnabled", "bool");
            }

            return await GetServiceResponseAsync(async () =>
            {
                 await _deviceLogic.UpdateDeviceEnabledStatusAsync(deviceId, isEnabled);
                 return true;
            });
        }

        // POST: api/v1/devices/5/commands/{commandName}
        [HttpPost]
        [Route("{deviceId}/commands/{commandName}")]
        [WebApiRequirePermission(Permission.SendCommandToDevices)]
        public async Task<HttpResponseMessage> SendCommand(string deviceId, string commandName, [FromBody]dynamic parameters)
        {
            ValidateArgumentNotNullOrWhitespace("deviceId", deviceId);
            ValidateArgumentNotNullOrWhitespace("commandName", commandName);

            return await GetServiceResponseAsync(async () =>
            {
                await _deviceLogic.SendCommandAsync(deviceId, commandName, parameters);
                return true; 
            });
        }

        // the following is somewhat dangerous if called accidentally--only include it in Debug mode
        // note that users will need to disable CSRF if calling from a web browser
#if DEBUG
        // DELETE: api/v1/all-devices
        [HttpDelete]
        [Route("~/api/v1/all-devices")]
        [WebApiRequirePermission(Permission.RemoveDevices)]
        public async Task<HttpResponseMessage> DeleteAllDevices()
        {
            return await GetServiceResponseAsync(async () =>
            {
                // note that you could also hardcode a query to delete a subset of devices
                var query = new DeviceListQuery()
                {
                    Skip = 0,
                    Take = 1000,
                    SortColumn = "DeviceID",
                };

                var devices = await _deviceLogic.GetDevices(query);

                foreach(var d in devices.Results)
                {
                    if (d.DeviceProperties != null && d.DeviceProperties.DeviceID != null)
                    {
                        string deviceId = DeviceSchemaHelper.GetDeviceID(d);

                        // do this in serial so as not to overload anything
                        Debug.Write("DELETING DEVICE: " + deviceId + "...");
                        await _deviceLogic.RemoveDeviceAsync(deviceId);
                        Debug.WriteLine("  (Deleted)");
                    }
                }
                return true;
            });
        }
#endif

    }
}
