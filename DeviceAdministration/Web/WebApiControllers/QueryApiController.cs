using Microsoft.Azure.Devices.Applications.RemoteMonitoring.DeviceAdmin.Infrastructure.Models;
using Microsoft.Azure.Devices.Applications.RemoteMonitoring.DeviceAdmin.Web.Security;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web.Http;

namespace Microsoft.Azure.Devices.Applications.RemoteMonitoring.DeviceAdmin.Web.WebApiControllers
{
    [RoutePrefix("api/v1/queries")]
    public class QueryApiController : WebApiControllerBase
    {
        public QueryApiController()
        {
        }

        [HttpGet]
        [Route("{queryName}")]
        [WebApiRequirePermission(Permission.ViewDevices)]
        //api/v1/queries/{queryName}
        public async Task<HttpResponseMessage> GetQuery(string queryName)
        {
            //TODO: mock code
            var queries = new List<Query>();
            queries.Add(new Query() { Name= "SampleQuery1", QueryString = "DeviceState = \"normal\"", IsTemporary = false });
            queries.Add(new Query() { Name = "SampleQuery2", QueryString = "reported.ModelNumber = \"MD-2\"" });

            return await GetServiceResponseAsync<IEnumerable<Query>>(async () =>
            {
                return await Task.FromResult(queries);
            });
        }

        [HttpPost]
        [Route("")]
        [WebApiRequirePermission(Permission.ViewDevices)]
        public async Task<HttpResponseMessage> AddQuery(Query query)
        {
            //TODO: mock code
            return await GetServiceResponseAsync<Query>(async () =>
            {
                return await Task.FromResult(query);
            });
        }
        
        [HttpDelete]
        [Route("{queryName}")]
        [WebApiRequirePermission(Permission.ViewDevices)]
        public async Task<HttpResponseMessage> DeleteQuery(string queryName)
        {
            //TODO: mock code
            return await GetServiceResponseAsync(async () =>
            {
                return await Task.FromResult(true);
            });
        }

        [HttpGet]
        [Route("{queryName}")]
        [WebApiRequirePermission(Permission.ViewDevices)]
        //api/v1/queries/{queryName}
        public async Task<HttpResponseMessage> GetMatchingDeviceCounts(string queryName, string methodName)
        {
            //TODO: mock code
            var result = new MatchingDevices();
            result.MatchedCount = 10;
            result.UnMatchCount = 1;

            return await GetServiceResponseAsync<MatchingDevices>(async () =>
            {
                return await Task.FromResult(result);
            });
        }

    }
}
