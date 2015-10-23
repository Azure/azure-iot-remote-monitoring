using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;

namespace Microsoft.Azure.Devices.Applications.RemoteMonitoring.Common.Utility
{
    public interface IDocDbRestUtility
    {
        Task InitializeDatabase();
        Task InitializeDeviceCollection();
        Task<DocDbRestQueryResult> QueryDeviceManagementCollectionAsync(
            string queryString, Dictionary<string, Object> queryParams, int pageSize = -1, string continuationToken = null);
        Task<JObject> SaveNewDeviceAsync(dynamic device);
        Task<JObject> UpdateDeviceAsync(dynamic updatedDevice);
        Task DeleteDeviceAsync(dynamic device);
    }
}
