using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web.Http;
using Microsoft.Azure.Devices.Applications.RemoteMonitoring.DeviceAdmin.Infrastructure.BusinessLogic;
using Microsoft.Azure.Devices.Applications.RemoteMonitoring.DeviceAdmin.Infrastructure.Models;
using Microsoft.Azure.Devices.Applications.RemoteMonitoring.DeviceAdmin.Web.Security;

namespace Microsoft.Azure.Devices.Applications.RemoteMonitoring.DeviceAdmin.Web.WebApiControllers
{
    [RoutePrefix("api/v1/devicetypes")]
    public class DeviceTypesController : WebApiControllerBase
    {
        private IDeviceTypeLogic _deviceTypeLogic;

        public DeviceTypesController(IDeviceTypeLogic deviceTypeLogic)
        {
            _deviceTypeLogic = deviceTypeLogic;
        }

        [HttpGet]
        [Route("")]
        [WebApiRequirePermission(Permission.ViewDevices)]
        // GET: api/v1/devicetypes
        public async Task<HttpResponseMessage> GetAllDeviceTypes()
        {
            return await GetServiceResponseAsync<List<DeviceType>>(async () =>
            {
                return await _deviceTypeLogic.GetAllDeviceTypesAsync();
            });
        }

        [HttpGet]
        [Route("{deviceTypeId}")]
        [WebApiRequirePermission(Permission.ViewDevices)]
        // GET: api/v1/devicetypes/5
        public async Task<HttpResponseMessage> GetDeviceType(int deviceTypeId)
        {
            return await GetServiceResponseAsync<DeviceType>(async () =>
            {
                return await _deviceTypeLogic.GetDeviceTypeAsync(deviceTypeId);
            });
        }
    }
}
