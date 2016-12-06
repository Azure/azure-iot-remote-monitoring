using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web.Http;
using Microsoft.Azure.Devices.Applications.RemoteMonitoring.DeviceAdmin.Infrastructure.BusinessLogic;
using Microsoft.Azure.Devices.Applications.RemoteMonitoring.DeviceAdmin.Infrastructure.Models;
using Microsoft.Azure.Devices.Applications.RemoteMonitoring.DeviceAdmin.Web.Security;

namespace Microsoft.Azure.Devices.Applications.RemoteMonitoring.DeviceAdmin.Web.WebApiControllers
{
    [RoutePrefix("api/v1/filters")]
    public class FilterApiController : WebApiControllerBase
    {
        private IFilterLogic _filterLogic;
        public FilterApiController(IFilterLogic filterLogic)
        {
            _filterLogic = filterLogic;
        }

        [HttpGet]
        [Route("")]
        [WebApiRequirePermission(Permission.ViewDevices)]
        //api/v1/filters
        public async Task<HttpResponseMessage> GetRecentFilters(int max=3)
        {
            return await GetServiceResponseAsync<IEnumerable<Filter>>(async () =>
            {
                return await _filterLogic.GetRecentFiltersAsync(max);
            });
        }

        [HttpGet]
        [Route("{filterName}")]
        [WebApiRequirePermission(Permission.ViewDevices)]
        //api/v1/filters/{filterName}
        public async Task<HttpResponseMessage> GetFilter(string filterName)
        {
            return await GetServiceResponseAsync<Filter>(async () =>
            {
                return await _filterLogic.GetFilterAsync(filterName);
            });
        }

        [HttpPost]
        [Route("")]
        [WebApiRequirePermission(Permission.ViewDevices)]
        public async Task<HttpResponseMessage> AddQuery(Filter filter)
        {
            return await GetServiceResponseAsync<bool>(async () =>
            {
                return await _filterLogic.AddFilterAsync(filter);
            });
        }
        
        [HttpDelete]
        [Route("{filterName}")]
        [WebApiRequirePermission(Permission.ViewDevices)]
        public async Task<HttpResponseMessage> DeleteFilter(string filterName)
        {
            return await GetServiceResponseAsync<bool>(async () =>
            {
                return await _filterLogic.DeleteFilterAsync(filterName);
            });
        }

        [HttpGet]
        [Route("~/api/v1/defaultFilterName/{prefix}")]
        [WebApiRequirePermission(Permission.ViewDevices)]
        //api/v1/defaultFilterName/{filterNamePrefix}
        public async Task<HttpResponseMessage> GetDefaultFilterName(string prefix)
        {
            return await GetServiceResponseAsync<string>(async () =>
            {
                return await _filterLogic.GetAvailableFilterNameAsync(prefix);
            });
        }

        [HttpPost]
        [Route("~/api/v1/generateAdvanceClause")]
        [WebApiRequirePermission(Permission.ViewDevices)]
        public async Task<HttpResponseMessage> GenerateAdvanceClause(Filter filter)
        {
            return await GetServiceResponseAsync<string>(async () =>
            {
                return await Task.FromResult(_filterLogic.GenerateAdvancedClause(filter.Clauses));
            });
        }

        [HttpGet]
        [Route("~/api/v1/filterList")]
        [WebApiRequirePermission(Permission.ViewDevices)]
        public async Task<HttpResponseMessage> GetFilterList()
        {
            return await GetServiceResponseAsync<IEnumerable<string>>(async () =>
            {
                return await _filterLogic.GetFilterList();
            });
        }

        [HttpGet]
        [Route("{filterName}")]
        [WebApiRequirePermission(Permission.ViewDevices)]
        //api/v1/filters/{filterName}
        public async Task<HttpResponseMessage> GetMatchingDeviceCounts(string filterName, string methodName)
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
