using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using System.Web;
using System.Web.Http;
using Microsoft.Azure.Devices.Applications.RemoteMonitoring.DeviceAdmin.Infrastructure.Models;
using Microsoft.Azure.Devices.Applications.RemoteMonitoring.DeviceAdmin.Infrastructure.Repository;
using Microsoft.Azure.Devices.Applications.RemoteMonitoring.DeviceAdmin.Web.Security;
using Microsoft.Azure.Devices.Applications.RemoteMonitoring.DeviceAdmin.Web.Models;
using Microsoft.Azure.Devices.Shared;

namespace Microsoft.Azure.Devices.Applications.RemoteMonitoring.DeviceAdmin.Web.WebApiControllers
{
    [RoutePrefix("api/v1/icons")]
    public class DeviceIconApiController : WebApiControllerBase
    {
        private readonly IDeviceIconRepository _deviceIconRepository;
        private readonly IIoTHubDeviceManager _deviceManager;

        public DeviceIconApiController(IIoTHubDeviceManager deviceManager, IDeviceIconRepository deviceIconRepository)
        {
            this._deviceIconRepository = deviceIconRepository;
            this._deviceManager = deviceManager;
        }

        [HttpGet]
        [Route("")]
        [WebApiRequirePermission(Permission.ViewDevices)]
        public async Task<HttpResponseMessage> GetIcons([FromUri]int skip = 0, [FromUri]int take = 10)
        {
            return await GetServiceResponseAsync<DeviceIconResult>(async () =>
            {
                return await _deviceIconRepository.GetIcons(skip, take);
            });
        }

        [HttpPost]
        [Route("")]
        [WebApiRequirePermission(Permission.EditDeviceMetadata)]
        public async Task<HttpResponseMessage> UploadIcon()
        {
            HttpPostedFile file = HttpContext.Current.Request.Files[0];
            return await GetServiceResponseAsync<DeviceIcon>(async () =>
            {
                return await _deviceIconRepository.AddIcon(file.FileName, file.InputStream);
            });
        }

        [HttpPut]
        [Route("{name}/{deviceId}/{actionType}")]
        [WebApiRequirePermission(Permission.EditDeviceMetadata)]
        public async Task<HttpResponseMessage> SaveIcon(string name, string deviceId, IconActionType actionType)
        {
            ValidateArgumentNotNullOrWhitespace("deviceId", deviceId);

            return await GetServiceResponseAsync<Twin>(async () =>
            {
                Twin twin = await this._deviceManager.GetTwinAsync(deviceId);
                switch (actionType)
                {
                    case IconActionType.Upload:
                        var savedIcon = await _deviceIconRepository.SaveIcon(name);
                        twin.Tags[Constants.DeviceIconTagName] = savedIcon.Name;
                        break;
                    case IconActionType.Apply:
                        twin.Tags[Constants.DeviceIconTagName] = name;
                        break;
                    case IconActionType.Remove:
                        twin.Tags[Constants.DeviceIconTagName] = null;
                        break;
                }

                await this._deviceManager.UpdateTwinAsync(deviceId, twin);
                return twin;
            });
        }

        [HttpGet]
        [Route("{deviceId}")]
        [WebApiRequirePermission(Permission.ViewDevices)]
        public async Task<HttpResponseMessage> GetIcon(string deviceId)
        {
            return await GetServiceResponseAsync<DeviceIcon>(async () =>
            {
                Twin twin = await _deviceManager.GetTwinAsync(deviceId);
                if (twin.Tags.Contains(Constants.DeviceIconTagName))
                {
                    return await _deviceIconRepository.GetIcon(twin.Tags[Constants.DeviceIconTagName].Value);
                }
                else
                {
                    return null;
                }
            });
        }

        [HttpDelete]
        [Route("{name}")]
        [WebApiRequirePermission(Permission.EditDeviceMetadata)]
        public async Task<HttpResponseMessage> DeleteIcon(string name)
        {
            return await GetServiceResponseAsync<bool>(async () =>
            {
                var filter = new DeviceListFilter()
                {
                    Clauses = new List<Clause>(),
                };

                filter.Clauses.Add(new Clause
                {
                    ColumnName = Constants.DeviceIconFullTagName,
                    ClauseType = ClauseType.EQ,
                    ClauseValue = name,
                });

                var devices = await _deviceManager.QueryDevicesAsync(filter);
                if (devices.Any())
                {
                    throw new Exception("The device icon has been used by devices and can not be deleted.");
                }

                return await _deviceIconRepository.DeleteIcon(name);
            });
        }
    }
}