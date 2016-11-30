using Microsoft.Azure.Devices.Applications.RemoteMonitoring.DeviceAdmin.Infrastructure.Repository;
using Microsoft.Azure.Devices.Applications.RemoteMonitoring.DeviceAdmin.Web.Security;
using Microsoft.Azure.Devices.Applications.RemoteMonitoring.Common.Extensions;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web.Http;
using System.Collections;
using System.Collections.Generic;

namespace Microsoft.Azure.Devices.Applications.RemoteMonitoring.DeviceAdmin.Web.WebApiControllers
{
    [RoutePrefix("api/v1/devices")]
    public class DeviceTwinApiController : WebApiControllerBase
    {
        private IIoTHubDeviceManager _deviceManager;
        public DeviceTwinApiController(IIoTHubDeviceManager deviceManager)
        {
            this._deviceManager = deviceManager;
        }

        [HttpGet]
        [Route("{deviceId}/twin/desired")]
        [WebApiRequirePermission(Permission.ViewDevices)]
        public async Task<HttpResponseMessage> GetDeviceTwinDesired(string deviceId)
        {
            var twin = await this._deviceManager.GetTwinAsync(deviceId);
            IEnumerable<KeyValuePair<string,TwinCollectionExtension.TwinValue>> flattenTwin = twin.Properties.Desired.AsEnumerableFlatten("desired.");
            return await GetServiceResponseAsync<IEnumerable<KeyValuePair<string, TwinCollectionExtension.TwinValue>>>(async () =>
            {
                return await Task.FromResult(flattenTwin);
            });
        }

        [HttpGet]
        [Route("{deviceId}/twin/tag")]
        [WebApiRequirePermission(Permission.ViewDevices)]
        public async Task<HttpResponseMessage> GetDeviceTwinTag(string deviceId)
        {
            var twin = await this._deviceManager.GetTwinAsync(deviceId);
            IEnumerable<KeyValuePair<string, TwinCollectionExtension.TwinValue>> flattenTwin = twin.Tags.AsEnumerableFlatten("tags.");
            return await GetServiceResponseAsync<IEnumerable<KeyValuePair<string, TwinCollectionExtension.TwinValue>>>(async () =>
            {
                return await Task.FromResult(flattenTwin);
            });
        }

        [HttpPut]
        [Route("{deviceId}/twin/desired")]
        [WebApiRequirePermission(Permission.ViewDevices)]
        public async Task<HttpResponseMessage> UpdateDeviceTwinDesiredProps(string deviceId, IEnumerable<KeyValuePair<string, TwinCollectionExtension.TwinValue>> twin )
        {
            //var twin = new Twin(deviceId);
            return await GetServiceResponseAsync<IEnumerable<KeyValuePair<string, TwinCollectionExtension.TwinValue>>>(async () =>
            {
                return await Task.FromResult(twin);
            });
        }

        [HttpPut]
        [Route("{deviceId}/twin/tag")]
        [WebApiRequirePermission(Permission.ViewDevices)]
        public async Task<HttpResponseMessage> UpdateDeviceTwinTags(string deviceId, IEnumerable<KeyValuePair<string, TwinCollectionExtension.TwinValue>> twin)
        {
            //var twin = new Twin(deviceId);
            return await GetServiceResponseAsync<IEnumerable<KeyValuePair<string, TwinCollectionExtension.TwinValue>>>(async () =>
            {
                return await Task.FromResult(twin);
            });
        }
    }
}
