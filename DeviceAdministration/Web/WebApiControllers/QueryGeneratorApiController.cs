using Microsoft.Azure.Devices.Applications.RemoteMonitoring.DeviceAdmin.Web.Security;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web.Http;

namespace Microsoft.Azure.Devices.Applications.RemoteMonitoring.DeviceAdmin.Web.WebApiControllers
{
    [RoutePrefix("api/v1/generatequery")]
    public class QueryGeneratorApiController : WebApiControllerBase
    {
        public QueryGeneratorApiController()
        {
        }

        [HttpGet]
        [Route("")]
        [WebApiRequirePermission(Permission.ViewDevices)]
        public async Task<HttpResponseMessage> GenerateQuery()
        {
            //TODO: mock code
            return await GetServiceResponseAsync<string>(async () =>
            {
                return await Task.FromResult("select * from devices");
            });
        }
    }
}
