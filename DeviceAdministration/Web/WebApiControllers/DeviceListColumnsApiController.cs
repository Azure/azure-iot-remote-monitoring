using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web.Http;
using GlobalResources;
using Microsoft.Azure.Devices.Applications.RemoteMonitoring.DeviceAdmin.Infrastructure.BusinessLogic;
using Microsoft.Azure.Devices.Applications.RemoteMonitoring.DeviceAdmin.Infrastructure.Models;
using Microsoft.Azure.Devices.Applications.RemoteMonitoring.DeviceAdmin.Web.Security;

namespace Microsoft.Azure.Devices.Applications.RemoteMonitoring.DeviceAdmin.Web.WebApiControllers
{
    [RoutePrefix("api/v1/deviceListColumns")]
    public class DeviceListColumnsApiController : WebApiControllerBase
    {
        private readonly IUserSettingsLogic _userSettingsLogic;
        private readonly List<DeviceListColumns> defaultColumns = new List<DeviceListColumns>()
        {
            new DeviceListColumns { Name = "tags.HubEnabledState", Alias = Strings.StatusHeader.ToUpperInvariant() },
            new DeviceListColumns { Name = "deviceId", Alias = Strings.DeviceIdHeader.ToUpperInvariant() },
            new DeviceListColumns { Name = "reported.System.Manufacturer", Alias = Strings.ManufactureHeader.ToUpperInvariant() },
            new DeviceListColumns { Name = "reported.System.FirmwareVersion", Alias = Strings.FirmwareHeader.ToUpperInvariant() },
            new DeviceListColumns { Name = "tags.Building", Alias = Strings.BuildingHeader.ToUpperInvariant() },
            new DeviceListColumns { Name = "reported.Config.TemperatureMeanValue", Alias = Strings.TemperatureHeader.ToUpperInvariant() },
            new DeviceListColumns { Name = "reported.Method.UpdateFirmware.Status", Alias = Strings.FwStatusHeader.ToUpperInvariant() }
        };

        public DeviceListColumnsApiController(IUserSettingsLogic userSettingsLogic)
        {
            _userSettingsLogic = userSettingsLogic;
        }

        [HttpGet]
        [Route("")]
        [WebApiRequirePermission(Permission.ViewDevices)]
        public async Task<HttpResponseMessage> GetDeviceListColumns()
        {
            var userId = PrincipalHelper.GetEmailAddress(User);

            return await GetServiceResponseAsync<IEnumerable<DeviceListColumns>>(async () =>
            {
                var columns = await _userSettingsLogic.GetDeviceListColumnsAsync(userId);

                if (columns == null || columns.Count() == 0)
                {
                    columns = defaultColumns;
                    await _userSettingsLogic.SetDeviceListColumnsAsync(userId, columns);
                }

                return columns;
            });
        }

        [HttpGet]
        [Route("global")]
        [WebApiRequirePermission(Permission.ViewDevices)]
        public async Task<HttpResponseMessage> GetGlobalDeviceListColumns()
        {
            return await GetServiceResponseAsync<IEnumerable<DeviceListColumns>>(async () =>
            {
                var columns = await _userSettingsLogic.GetGlobalDeviceListColumnsAsync();

                if (columns == null)
                {
                    columns = defaultColumns;
                }

                return columns;
            });
        }

        [HttpPut]
        [Route("")]
        [WebApiRequirePermission(Permission.ViewDevices)]
        public async Task<HttpResponseMessage> UpdateDeviceListColumns([FromBody] IEnumerable<DeviceListColumns> deviceListColumns, bool saveAsGlobal = false)
        {
            var userId = PrincipalHelper.GetEmailAddress(User);

            return await GetServiceResponseAsync<bool>(async () =>
            {
                return await _userSettingsLogic.SetDeviceListColumnsAsync(userId, deviceListColumns, saveAsGlobal);
            });
        }

    }
}
