using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web.Http;
using Microsoft.Azure.Devices.Applications.RemoteMonitoring.Common.Extensions;
using Microsoft.Azure.Devices.Applications.RemoteMonitoring.DeviceAdmin.Infrastructure.BusinessLogic;
using Microsoft.Azure.Devices.Applications.RemoteMonitoring.DeviceAdmin.Infrastructure.Repository;
using Microsoft.Azure.Devices.Applications.RemoteMonitoring.DeviceAdmin.Web.Security;
using Microsoft.Azure.Devices.Shared;

namespace Microsoft.Azure.Devices.Applications.RemoteMonitoring.DeviceAdmin.Web.WebApiControllers
{
    [RoutePrefix("api/v1/devices")]
    public class DeviceTwinApiController : WebApiControllerBase
    {
        private IIoTHubDeviceManager _deviceManager;
        private INameCacheLogic _nameCacheLogic;

        public DeviceTwinApiController(IIoTHubDeviceManager deviceManager, INameCacheLogic nameCacheLogic)
        {
            this._deviceManager = deviceManager;
            this._nameCacheLogic = nameCacheLogic;
        }

        [HttpGet]
        [Route("{deviceId}/twin/desired")]
        [WebApiRequirePermission(Permission.ViewDevices)]
        public async Task<HttpResponseMessage> GetDeviceTwinDesired(string deviceId)
        {
            var twin = await this._deviceManager.GetTwinAsync(deviceId);
            IEnumerable<KeyValuePair<string, TwinCollectionExtension.TwinValue>> flattenReportedTwin = twin.Properties.Reported.AsEnumerableFlatten("reported.");
            IEnumerable<KeyValuePair<string, TwinCollectionExtension.TwinValue>> flattenTwin = twin.Properties.Desired.AsEnumerableFlatten("desired.");
            return await GetServiceResponseAsync<dynamic>(async () =>
            {
                return await Task.FromResult(new { desired = flattenTwin, reported = flattenReportedTwin });
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
        public async Task UpdateDeviceTwinDesiredProps(string deviceId, IEnumerable<KeyValuePair<string, TwinCollectionExtension.TwinValue>> newtwin)
        {

            Twin updatetwin = new Twin();
            updatetwin.ETag = "*";
            foreach (var twin in newtwin)
            {
                if (String.IsNullOrEmpty(twin.Key))
                {
                    continue;
                }
                var key = twin.Key;
                if (key.ToLower().StartsWith("desired."))
                {
                    key = key.Substring(8);
                }
                updatetwin.Properties.Desired.Set(key, twin.Value.Value.ToString());
                await _nameCacheLogic.AddNameAsync(twin.Key);
            }
            await _deviceManager.UpdateTwinAsync(deviceId, updatetwin);
        }

        [HttpPut]
        [Route("{deviceId}/twin/tag")]
        [WebApiRequirePermission(Permission.ViewDevices)]
        public async Task UpdateDeviceTwinTags(string deviceId, IEnumerable<KeyValuePair<string, TwinCollectionExtension.TwinValue>> newtwin)
        {
            Twin updatetwin = new Twin();
            updatetwin.ETag = "*";
            foreach (var twin in newtwin)
            {
                if (String.IsNullOrEmpty(twin.Key))
                {
                    continue;
                }
                var key = twin.Key;
                if (key.ToLower().StartsWith("tags."))
                {
                    key = key.Substring(5);
                }
                updatetwin.Tags.Set(key, twin.Value.Value.ToString());
                await _nameCacheLogic.AddNameAsync(twin.Key);
            }
            await _deviceManager.UpdateTwinAsync(deviceId, updatetwin);
        }
    }
}
