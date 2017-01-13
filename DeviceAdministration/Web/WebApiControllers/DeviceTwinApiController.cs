using System;
using System.Collections.Generic;
using System.Net;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web.Http;
using Microsoft.Azure.Devices.Applications.RemoteMonitoring.Common.Extensions;
using Microsoft.Azure.Devices.Applications.RemoteMonitoring.DeviceAdmin.Infrastructure.BusinessLogic;
using Microsoft.Azure.Devices.Applications.RemoteMonitoring.DeviceAdmin.Infrastructure.Repository;
using Microsoft.Azure.Devices.Applications.RemoteMonitoring.DeviceAdmin.Web.Security;
using Microsoft.Azure.Devices.Shared;
using Microsoft.Azure.Devices.Applications.RemoteMonitoring.DeviceAdmin.Web.Models;

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
            IEnumerable<KeyValuePair<string, TwinCollectionExtension.TwinValue>> flattenReportedTwin = twin.Properties.Reported.AsEnumerableFlatten("reported.").Where(t => !t.Key.IsReservedTwinName());
            IEnumerable<KeyValuePair<string, TwinCollectionExtension.TwinValue>> flattenTwin = twin.Properties.Desired.AsEnumerableFlatten("desired.").Where(t => !t.Key.IsReservedTwinName());
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
            IEnumerable<KeyValuePair<string, TwinCollectionExtension.TwinValue>> flattenTwin = twin.Tags.AsEnumerableFlatten("tags.").Where(t => !t.Key.IsReservedTwinName());
            return await GetServiceResponseAsync<IEnumerable<KeyValuePair<string, TwinCollectionExtension.TwinValue>>>(async () =>
            {
                return await Task.FromResult(flattenTwin);
            });
        }

        [HttpPut]
        [Route("{deviceId}/twin/desired")]
        [WebApiRequirePermission(Permission.ViewDevices)]
        public async Task UpdateDeviceTwinDesiredProps(string deviceId, IEnumerable<PropertyViewModel> newtwin)
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
                setTwinProperties(twin, updatetwin.Properties.Desired, key);
                var addnametask = _nameCacheLogic.AddNameAsync(twin.Key);
            }
            await _deviceManager.UpdateTwinAsync(deviceId, updatetwin);
        }

        [HttpPut]
        [Route("{deviceId}/twin/tag")]
        [WebApiRequirePermission(Permission.ViewDevices)]
        public async Task UpdateDeviceTwinTags(string deviceId, IEnumerable<PropertyViewModel> newtwin)
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
                setTwinProperties(twin, updatetwin.Tags, key);
                var addnametask = _nameCacheLogic.AddNameAsync(twin.Key);
            }
            await _deviceManager.UpdateTwinAsync(deviceId, updatetwin);
        }

        private void setTwinProperties(PropertyViewModel twin, TwinCollection prop, string key)
        {
            if (twin.IsDeleted)
            {
                prop.Set(key, null);
            }
            else
            {
                switch (twin.DataType)
                {
                    case Infrastructure.Models.TwinDataType.String:
                        string valueString = twin.Value.Value.ToString();
                        prop.Set(key, valueString);
                        break;
                    case Infrastructure.Models.TwinDataType.Number:
                        int valueInt;
                        float valuefloat;
                        if (int.TryParse(twin.Value.Value.ToString(), out valueInt))
                        {
                            prop.Set(key, valueInt);
                        }
                        else if (float.TryParse(twin.Value.Value.ToString(), out valuefloat))
                        {
                            prop.Set(key, valuefloat);
                        }
                        else
                        {
                            prop.Set(key, twin.Value.Value as string);
                        }
                        break;
                    case Infrastructure.Models.TwinDataType.Boolean:
                        bool valueBool;
                        if (bool.TryParse(twin.Value.Value.ToString(), out valueBool))
                        {
                            prop.Set(key, valueBool);
                        }
                        else
                        {
                            prop.Set(key, twin.Value.Value as string);
                        }
                        break;
                }
            }
        }
    }
}
