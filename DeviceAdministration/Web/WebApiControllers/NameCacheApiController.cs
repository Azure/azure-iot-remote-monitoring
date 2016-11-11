using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web.Http;
using Microsoft.Azure.Devices.Applications.RemoteMonitoring.DeviceAdmin.Infrastructure.BusinessLogic;
using Microsoft.Azure.Devices.Applications.RemoteMonitoring.DeviceAdmin.Infrastructure.Models;
using Microsoft.Azure.Devices.Applications.RemoteMonitoring.DeviceAdmin.Web.Security;

namespace Microsoft.Azure.Devices.Applications.RemoteMonitoring.DeviceAdmin.Web.WebApiControllers
{
    [RoutePrefix("api/v1/namecache")]
    public class NameCacheApiController : WebApiControllerBase
    {
        private INameCacheLogic _nameCacheLogic;

        public NameCacheApiController(INameCacheLogic nameCacheLogic)
        {
            _nameCacheLogic = nameCacheLogic;
        }

        [HttpGet]
        [Route("list/{type}")]
        [WebApiRequirePermission(Permission.ViewDevices)]
        // GET: api/v1/namecache/list/1
        public async Task<HttpResponseMessage> GetNameList(NameCacheEntityType type)
        {
            return await GetServiceResponseAsync<IEnumerable<NameCacheEntity>>(async () =>
            {
                return await _nameCacheLogic.GetNameListAsync(type);
            });
        }
    }
}
