using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;

namespace Microsoft.Azure.Devices.Applications.RemoteMonitoring.Common.Utility
{
    public interface IDocDbRestUtility
    {
        Task InitializeDatabase();
        Task InitializeCollection();
        Task<DocDbRestQueryResult> QueryCollectionAsync(
            string queryString, Dictionary<string, Object> queryParams, int pageSize = -1, string continuationToken = null);
        Task<JObject> SaveNewDocumentAsync(dynamic document);
        Task<JObject> UpdateDocumentAsync(dynamic updatedDocument);
        Task DeleteDocumentAsync(dynamic document);
    }
}