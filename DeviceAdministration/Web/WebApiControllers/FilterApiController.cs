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
        public async Task<HttpResponseMessage> GetRecentFilters(int max = 10)
        {
            return await GetServiceResponseAsync<IEnumerable<Filter>>(async () =>
            {
                return await _filterLogic.GetRecentFiltersAsync(max);
            });
        }

        [HttpGet]
        [Route("{filterId}")]
        [WebApiRequirePermission(Permission.ViewDevices)]
        //api/v1/filters/{filterId}
        public async Task<HttpResponseMessage> GetFilter(string filterId)
        {
            ValidateArgumentNotNullOrWhitespace("filterId", filterId);

            return await GetServiceResponseAsync<Filter>(async () =>
            {
                return await _filterLogic.GetFilterAsync(filterId);
            });
        }

        [HttpPost]
        [Route("")]
        [WebApiRequirePermission(Permission.ViewDevices)]
        public async Task<HttpResponseMessage> SaveFilter(Filter filter)
        {
            return await GetServiceResponseAsync<Filter>(async () =>
            {
                return await _filterLogic.SaveFilterAsync(filter);
            });
        }
        
        [HttpDelete]
        [Route("{filterId}/{force}")]
        [WebApiRequirePermission(Permission.ViewDevices)]
        public async Task<HttpResponseMessage> DeleteFilter(string filterId, bool force = false)
        {
            ValidateArgumentNotNullOrWhitespace("filterId", filterId);

            return await GetServiceResponseAsync<bool>(async () =>
            {
                return await _filterLogic.DeleteFilterAsync(filterId, force);
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
        public async Task<HttpResponseMessage> GetFilterList([FromUri]int skip = 0, [FromUri]int take = 1000)
        {
            return await GetServiceResponseAsync<IEnumerable<Filter>>(async () =>
            {
                return await _filterLogic.GetFilterList(skip, take);
            });
        }

        [HttpGet]
        [Route("~/api/v1/suggestedClauses")]
        [WebApiRequirePermission(Permission.ViewDevices)]
        public async Task<HttpResponseMessage> GetSuggestClauses([FromUri]int skip = 0, [FromUri]int take = 15)
        {
            return await GetServiceResponseAsync<IEnumerable<Clause>>(async () =>
            {
                return await _filterLogic.GetSuggestClauses(skip, take);
            });
        }

        [HttpDelete]
        [Route("~/api/v1/suggestedClauses")]
        [WebApiRequirePermission(Permission.DeleteSuggestedClauses)]
        public async Task<HttpResponseMessage> DeleteSuggestClauses([FromBody]IEnumerable<Clause> clauses)
        {
            return await GetServiceResponseAsync<int>(async () =>
            {
                return await _filterLogic.DeleteSuggestClausesAsync(clauses);
            });
        }
    }
}
