using Microsoft.Azure.Devices.Applications.RemoteMonitoring.DeviceAdmin.Web.Security;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web.Http;

namespace Microsoft.Azure.Devices.Applications.RemoteMonitoring.DeviceAdmin.Web.WebApiControllers
{
    [RoutePrefix("api/v1/querynames")]
    public class QueryNameApiController : WebApiControllerBase
    {
        public QueryNameApiController()
        {
        }

        [HttpGet]
        [Route("all")]
        [WebApiRequirePermission(Permission.ViewDevices)]
        public async Task<HttpResponseMessage> GetQueryNames()
        {
            //TODO: mock code
            var queries = new List<string>();
            queries.Add("SampleQuery1");
            queries.Add("SampleQuery2");
            queries.Add("SampleQuery3");
            queries.Add("SampleQuery4");

            return await GetServiceResponseAsync<IEnumerable<string>>(async () =>
            {
                return await Task.FromResult(queries);
            });
        }

        [HttpGet]
        [Route("recent")]
        [WebApiRequirePermission(Permission.ViewDevices)]
        public async Task<HttpResponseMessage> GetRecentQueryNames([FromUri] int top = 3)
        {
            //TODO: mock code
            var queries = new List<string>();
            queries.Add("SampleQuery1");
            queries.Add("SampleQuery2");

            return await GetServiceResponseAsync<IEnumerable<string>>(async () =>
            {
                return await Task.FromResult(queries);
            });
        }
    }
}
