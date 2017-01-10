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
        private readonly List<DeviceListColumns> defaultColumns = new List<DeviceListColumns>() {
                    new DeviceListColumns() { Name = "tags.HubEnabledState", Alias = Strings.StatusHeader },
                    new DeviceListColumns() { Name = "deviceId", Alias = Strings.DeviceIdHeader },
                    new DeviceListColumns() { Name = "reported.System.Manufacturer", Alias = Strings.ManufactureHeader },
                    new DeviceListColumns() { Name = "reported.System.ModelNumber", Alias = Strings.ModelNumberHeader },
                    new DeviceListColumns() { Name = "reported.System.SerialNumber", Alias = Strings.SerialNumberHeader },
                    new DeviceListColumns() { Name = "reported.System.FirmwareVersion", Alias = Strings.FirmwareHeader },
                    new DeviceListColumns() { Name = "reported.System.Platform", Alias = Strings.PlatformHeader },
                    new DeviceListColumns() { Name = "reported.System.Processor", Alias = Strings.ProcessorHeader },
                    new DeviceListColumns() { Name = "reported.System.InstalledRAM", Alias = Strings.InstalledRamHeader }};

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
                var columns =  await _userSettingsLogic.GetDeviceListColumnsAsync(userId);

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
                return await _userSettingsLogic.GetGlobalDeviceListColumnsAsync();
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
