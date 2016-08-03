using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Azure.Devices.Applications.RemoteMonitoring.Common.Models;
using Newtonsoft.Json.Linq;

namespace Microsoft.Azure.Devices.Applications.RemoteMonitoring.Common.Utility
{
    public interface IDocDbRestUtility
    {
        Task InitializeDatabase();
        Task InitializeCollection();
        Task<DocDbRestQueryResult> QueryCollectionAsync(
            string queryString, Dictionary<string, Object> queryParams, int pageSize = -1, string continuationToken = null);
        Task<JObject> SaveNewDocumentAsync<T>(T document);
        Task<JObject> UpdateDocumentAsync<T>(T updatedDocument);
        Task DeleteDocumentAsync<T>(T document);
    }
}