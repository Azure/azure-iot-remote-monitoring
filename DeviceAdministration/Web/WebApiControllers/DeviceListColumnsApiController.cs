using System.Net.Http;
using System.Threading.Tasks;
using System.Web.Http;
using Microsoft.Azure.Devices.Applications.RemoteMonitoring.Common.Models;
using Microsoft.Azure.Devices.Applications.RemoteMonitoring.DeviceAdmin.Infrastructure.BusinessLogic;
using Microsoft.Azure.Devices.Applications.RemoteMonitoring.DeviceAdmin.Infrastructure.Models;
using Microsoft.Azure.Devices.Applications.RemoteMonitoring.DeviceAdmin.Web.Security;
using System.Collections.Generic;

namespace Microsoft.Azure.Devices.Applications.RemoteMonitoring.DeviceAdmin.Web.WebApiControllers
{
    [RoutePrefix("api/v1/deviceListColumns")]
    public class DeviceListColumnsApiController : WebApiControllerBase
    {
        public DeviceListColumnsApiController()
        {
        }

        [HttpGet]
        [Route("")]
        [WebApiRequirePermission(Permission.ViewDevices)]
        public async Task<HttpResponseMessage> GetDeviceListColumns()
        {
            // TODO: get user related to this call
            var deviceListColumns = new List<DeviceListColumns>();
            deviceListColumns.Add(new DeviceListColumns() { Name = "Status" });
            deviceListColumns.Add(new DeviceListColumns() { Name = "DeviceId" });
            deviceListColumns.Add(new DeviceListColumns() { Name = "reported.Manufacture", Alias = "Manufacture" });
            deviceListColumns.Add(new DeviceListColumns() { Name = "reported.Model", Alias = "Model" });

            return await GetServiceResponseAsync<IEnumerable<DeviceListColumns>>(async () =>
            {
                return await Task.FromResult(deviceListColumns);
            });
        }

        [HttpPut]
        [Route("")]
        [WebApiRequirePermission(Permission.ViewDevices)]
        public async Task<HttpResponseMessage> UpdateDeviceListColumns()
        {
            // TODO: read DeviceListColumns from payload
            var deviceListColumns = new List<DeviceListColumns>();

            return await GetServiceResponseAsync<IEnumerable<DeviceListColumns>>(async () =>
            {
                return await Task.FromResult(deviceListColumns);
            });
        }

    }
}
