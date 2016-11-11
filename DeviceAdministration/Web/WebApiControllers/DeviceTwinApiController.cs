using Microsoft.Azure.Devices.Applications.RemoteMonitoring.DeviceAdmin.Web.Security;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web.Http;

namespace Microsoft.Azure.Devices.Applications.RemoteMonitoring.DeviceAdmin.Web.WebApiControllers
{
    [RoutePrefix("api/v1/devices")]
    public class DeviceTwinApiController : WebApiControllerBase
    {
        public DeviceTwinApiController()
        {
        }

        [HttpGet]
        [Route("{deviceId}/twin")]
        [WebApiRequirePermission(Permission.ViewDevices)]
        public async Task<HttpResponseMessage> GetDeviceTwin(string deviceId)
        {
            var twin = new Twin(deviceId);
            return await GetServiceResponseAsync<Twin>(async () =>
            {
                return await Task.FromResult(twin);
            });
        }

        [HttpPut]
        [Route("{deviceId}/twin")]
        [WebApiRequirePermission(Permission.ViewDevices)]
        public async Task<HttpResponseMessage> UpdateDeviceTwin(string deviceId)
        {
            var twin = new Twin(deviceId);
            return await GetServiceResponseAsync<Twin>(async () =>
            {
                return await Task.FromResult(twin);
            });
        }
    }
}
