using System.Net.Http;
using System.Threading.Tasks;
using System.Web.Http;
using Microsoft.Azure.Devices.Applications.RemoteMonitoring.Common.Models;
using Microsoft.Azure.Devices.Applications.RemoteMonitoring.DeviceAdmin.Infrastructure.BusinessLogic;
using Microsoft.Azure.Devices.Applications.RemoteMonitoring.DeviceAdmin.Web.Security;

namespace Microsoft.Azure.Devices.Applications.RemoteMonitoring.DeviceAdmin.Web.WebApiControllers
{
    [RoutePrefix("api/v1/keys")]
    public class KeyApiController : WebApiControllerBase
    {
        private IKeyLogic _keyLogic;

        public KeyApiController(IKeyLogic keyLogic)
        {
            _keyLogic = keyLogic;
        }

        [HttpGet]
        [Route("")]
        [WebApiRequirePermission(Permission.ViewDeviceSecurityKeys)]
        public async Task<HttpResponseMessage> GetKeysAsync()
        {
            return await GetServiceResponseAsync<SecurityKeys>(async () =>
            {
                return await _keyLogic.GetKeysAsync();
            });
        }

    }
}
