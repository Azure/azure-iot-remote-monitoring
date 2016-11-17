using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web.Http;
using Microsoft.Azure.Devices.Applications.RemoteMonitoring.DeviceAdmin.Infrastructure.BusinessLogic;
using Microsoft.Azure.Devices.Applications.RemoteMonitoring.DeviceAdmin.Infrastructure.Models;
using Microsoft.Azure.Devices.Applications.RemoteMonitoring.DeviceAdmin.Web.Security;

namespace Microsoft.Azure.Devices.Applications.RemoteMonitoring.DeviceAdmin.Web.WebApiControllers
{
    [RoutePrefix("api/v1/queries")]
    public class QueryApiController : WebApiControllerBase
    {
        private IQueryLogic _queryLogic;
        public QueryApiController(IQueryLogic queryLogic)
        {
            _queryLogic = queryLogic;
        }

        [HttpGet]
        [Route("")]
        [WebApiRequirePermission(Permission.ViewDevices)]
        //api/v1/queries
        public async Task<HttpResponseMessage> GetRecentQueries(int max=3)
        {
            return await GetServiceResponseAsync<IEnumerable<Query>>(async () =>
            {
                return await _queryLogic.GetRecentQueriesAsync(max);
            });
        }

        [HttpGet]
        [Route("{queryName}")]
        [WebApiRequirePermission(Permission.ViewDevices)]
        //api/v1/queries/{queryName}
        public async Task<HttpResponseMessage> GetQuery(string queryName)
        {
            return await GetServiceResponseAsync<Query>(async () =>
            {
                return await _queryLogic.GetQueryAsync(queryName);
            });
        }

        [HttpPost]
        [Route("")]
        [WebApiRequirePermission(Permission.ViewDevices)]
        public async Task<HttpResponseMessage> AddQuery(Query query)
        {
            return await GetServiceResponseAsync<bool>(async () =>
            {
                return await _queryLogic.AddQueryAsync(query);
            });
        }
        
        [HttpDelete]
        [Route("{queryName}")]
        [WebApiRequirePermission(Permission.ViewDevices)]
        public async Task<HttpResponseMessage> DeleteQuery(string queryName)
        {
            return await GetServiceResponseAsync<bool>(async () =>
            {
                return await _queryLogic.DeleteQueryAsync(queryName);
            });
        }

        [HttpGet]
        [Route("~/api/v1/availableQueryName/{queryNamePrefix}")]
        [WebApiRequirePermission(Permission.ViewDevices)]
        //api/v1/availableQueryName/{queryNamePrefix}
        public async Task<HttpResponseMessage> GetAvailableQueryName(string queryNamePrefix)
        {
            return await GetServiceResponseAsync<string>(async () =>
            {
                return await _queryLogic.GetAvailableQueryNameAsync(queryNamePrefix);
            });
        }

        [HttpPost]
        [Route("~/api/v1/generateSql")]
        [WebApiRequirePermission(Permission.ViewDevices)]
        public async Task<HttpResponseMessage> GenerateSql(Query query)
        {
            return await GetServiceResponseAsync<string>(async () =>
            {
                return await Task.FromResult(_queryLogic.GenerateSql(query.Filters));
            });
        }

        [HttpGet]
        [Route("~/api/v1/queryList")]
        [WebApiRequirePermission(Permission.ViewDevices)]
        public async Task<HttpResponseMessage> GetQueryList()
        {
            return await GetServiceResponseAsync<IEnumerable<string>>(async () =>
            {
                return await _queryLogic.GetQueryNameList();
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
