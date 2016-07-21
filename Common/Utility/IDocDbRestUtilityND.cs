using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Azure.Devices.Applications.RemoteMonitoring.Common.Models;
using Newtonsoft.Json.Linq;

namespace Microsoft.Azure.Devices.Applications.RemoteMonitoring.Common.Utility
{
    public interface IDocDbRestUtilityND
    {
        Task InitializeDatabase();
        Task InitializeCollection();
        Task<DocDbRestQueryResult> QueryCollectionAsync(
            string queryString, Dictionary<string, Object> queryParams, int pageSize = -1, string continuationToken = null);
        Task<JObject> SaveNewDocumentAsync(dynamic document);
        Task<DeviceND> SaveNewDocumentAsyncND(DeviceND document);
        Task<JObject> UpdateDocumentAsync(dynamic updatedDocument);
        Task<DeviceND> UpdateDocumentAsyncND(DeviceND updatedDocument);
        Task DeleteDocumentAsync(dynamic document);
    }
}