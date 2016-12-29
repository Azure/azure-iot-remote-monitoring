using System.Collections.Generic;
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
    [RoutePrefix("api/v1/devices")]
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
        [Route("{deviceId}/icons")]
        [WebApiRequirePermission(Permission.ViewDevices)]
        public async Task<HttpResponseMessage> GetDeviceIcons(string deviceId)
        {
            return await GetServiceResponseAsync<IEnumerable<DeviceIcon>>(async () =>
            {
                return await _deviceIconRepository.GetIcons(deviceId);
            });
        }

        [HttpPost]
        [Route("{deviceId}/icons")]
        [WebApiRequirePermission(Permission.EditDeviceMetadata)]
        public async Task<HttpResponseMessage> UploadIcon(string deviceId)
        {
            ValidateArgumentNotNullOrWhitespace("deviceId", deviceId);

            HttpPostedFile file = HttpContext.Current.Request.Files[0];
            return await GetServiceResponseAsync<DeviceIcon>(async () =>
            {
                return await _deviceIconRepository.AddIcon(deviceId, file.FileName, file.InputStream);
            });
        }

        [HttpPut]
        [Route("{deviceId}/icons/{name}/{actionType}")]
        [WebApiRequirePermission(Permission.EditDeviceMetadata)]
        public async Task<HttpResponseMessage> SaveIcon(string deviceId, IconActionType actionType, string name)
        {
            return await GetServiceResponseAsync<Twin>(async () =>
            {
                Twin twin = await this._deviceManager.GetTwinAsync(deviceId);
                switch (actionType)
                {
                    case IconActionType.Upload:
                        var savedIcon = await _deviceIconRepository.SaveIcon(deviceId, name);
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
        [Route("{deviceId}/icons/{name}/{applied}")]
        [WebApiRequirePermission(Permission.EditDeviceMetadata)]
        public async Task<HttpResponseMessage> GetIcon(string deviceId, string name, bool? applied)
        {
            return await Task.Run(async () =>
            {
                var icon = await _deviceIconRepository.GetIcon(deviceId, name, applied.HasValue ? applied.Value : true);
                HttpResponseMessage result = new HttpResponseMessage(HttpStatusCode.OK);
                result.Content = new ByteArrayContent(icon.ImageStream.ToArray());
                var mediaType = MimeMapping.GetMimeMapping(string.IsNullOrEmpty(icon.Extension) ? "image/png" : icon.Extension);
                result.Content.Headers.ContentType = new MediaTypeHeaderValue(mediaType);
                return result;
            });
        }
    }
}