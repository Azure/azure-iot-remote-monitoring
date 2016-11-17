using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web.Http;
using GlobalResources;
using Microsoft.Azure.Devices.Applications.RemoteMonitoring.DeviceAdmin.Infrastructure.Models;
using Microsoft.Azure.Devices.Applications.RemoteMonitoring.DeviceAdmin.Infrastructure.Repository;
using Microsoft.Azure.Devices.Applications.RemoteMonitoring.DeviceAdmin.Web.Security;

namespace Microsoft.Azure.Devices.Applications.RemoteMonitoring.DeviceAdmin.Web.WebApiControllers
{
    [RoutePrefix("api/v1/deviceListColumns")]
    public class DeviceListColumnsApiController : WebApiControllerBase
    {
        private readonly IDeviceListColumnsRepository _deviceListColumnsRepository;
        private readonly List<DeviceListColumns> defaultColumns = new List<DeviceListColumns>() {
                    new DeviceListColumns() { Name = "tags.HubEnabledState", Alias = Strings.StatusHeader },
                    new DeviceListColumns() { Name = "deviceId", Alias = Strings.DeviceIdHeader },
                    new DeviceListColumns() { Name = "reported.Manufacturer", Alias = Strings.ManufactureHeader },
                    new DeviceListColumns() { Name = "reported.ModelNumber", Alias = Strings.ModelNumberHeader },
                    new DeviceListColumns() { Name = "reported.SerialNumber", Alias = Strings.SerialNumberHeader },
                    new DeviceListColumns() { Name = "reported.FirmwareVersion", Alias = Strings.FirmwareHeader },
                    new DeviceListColumns() { Name = "reported.Platform", Alias = Strings.PlatformHeader },
                    new DeviceListColumns() { Name = "reported.Processor", Alias = Strings.ProcessorHeader },
                    new DeviceListColumns() { Name = "reported.InstalledRAM", Alias = Strings.InstalledRamHeader }};

        public DeviceListColumnsApiController(IDeviceListColumnsRepository deviceListColumnsRepository)
        {
            _deviceListColumnsRepository = deviceListColumnsRepository;
        }

        [HttpGet]
        [Route("")]
        [WebApiRequirePermission(Permission.ViewDevices)]
        public async Task<HttpResponseMessage> GetDeviceListColumns()
        {
            var userId = PrincipalHelper.GetEmailAddress(User);

            return await GetServiceResponseAsync<IEnumerable<DeviceListColumns>>(async () =>
            {
                var columns =  await _deviceListColumnsRepository.GetAsync(userId);

                if (columns == null || columns.Count() == 0)
                {
                    columns = defaultColumns;
                    await _deviceListColumnsRepository.SaveAsync(userId, columns);
                }

                return columns;
            });
        }

        [HttpPut]
        [Route("")]
        [WebApiRequirePermission(Permission.ViewDevices)]
        public async Task<HttpResponseMessage> UpdateDeviceListColumns([FromBody] IEnumerable<DeviceListColumns> deviceListColumns)
        {
            var userId = PrincipalHelper.GetEmailAddress(User);

            return await GetServiceResponseAsync<bool>(async () =>
            {
                return await _deviceListColumnsRepository.SaveAsync(userId, deviceListColumns);
            });
        }

    }
}
