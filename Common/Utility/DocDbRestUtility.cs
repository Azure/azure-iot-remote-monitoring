using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.Net;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Azure.Devices.Applications.RemoteMonitoring.Common.Configurations;
using Microsoft.Azure.Devices.Applications.RemoteMonitoring.Common.DeviceSchema;
using Newtonsoft.Json.Linq;
using Microsoft.Azure.Devices.Applications.RemoteMonitoring.Common.Helpers;

namespace Microsoft.Azure.Devices.Applications.RemoteMonitoring.Common.Utility
{
    public class DocDbRestUtility : IDocDbRestUtility
    {
        //DocDB Rest documentation: https://msdn.microsoft.com/en-us/library/azure/dn781481.aspx

        private readonly string _docDbEndpoint;
        private readonly string _docDbKey;
        private readonly string _dbName;
        private string _dbId;
        private readonly string _collectionName;
        private string _collectionId;

        private const string DATE_HEADER_KEY = "x-ms-date";
        private const string VERSION_HEADER_KEY = "x-ms-version";
        private const string ACCEPT_HEADER_KEY = "Accept";
        private const string CONTENT_TYPE_HEADER_KEY = "Content-Type";
        private const string AUTHORIZATION_HEADER_KEY = "authorization";
        private const string CONTINUATION_HEADER_KEY = "x-ms-continuation";
        private const string MAX_ITEMS_HEADER_KEY = "x-ms-max-item-count";
        private const string IS_QUERY_HEADER_KEY = "x-ms-documentdb-isquery";

        private const string APPLICATION_JSON = "application/json";
        private const string APPLICATION_QUERY_JSON = "application/query+json";
        private const string X_MS_VERSION = "2015-08-06";

        private const string ITEM_COUNT_RESPONSE_HEADER_KEY = "x-ms-item-count";

        private const string POST_VERB = "POST";
        private const string PUT_VERB = "PUT";
        private const string GET_VERB = "GET";
        private const string DELETE_VERB = "DELETE";

        public DocDbRestUtility(IConfigurationProvider configProvider)
        {
            if (configProvider == null)
            {
                throw new ArgumentNullException("configProvider");
            }

            _docDbEndpoint = configProvider.GetConfigurationSettingValue("docdb.EndpointUrl");
            _docDbKey = configProvider.GetConfigurationSettingValue("docdb.PrimaryAuthorizationKey");
            _dbName = configProvider.GetConfigurationSettingValue("docdb.DatabaseId");
            _collectionName = configProvider.GetConfigurationSettingValue("docdb.DocumentCollectionId");
        }

        public async Task InitializeDatabase()
        {
<<<<<<< HEAD
            string endpoint = string.Format("{0}dbs", _docDbEndpoint);
            string queryString = "SELECT * FROM dbs db WHERE (db.id = @id)";
            var queryParams = new Dictionary<string, object>();
            queryParams.Add("@id", _dbName);
=======
            IEnumerable databases;
            string topResponse;

            string endpoint = string.Format("{0}dbs", _docDbEndpoint);
            using (WebClient client = BuildWebClient())
            {
                client.Headers.Add(AUTHORIZATION_HEADER_KEY, GetAuthorizationToken("GET", DATABASE_RESOURCE_TYPE, ""));
                topResponse = await AzureRetryHelper.OperationWithBasicRetryAsync<string>(async () =>
                    await client.DownloadStringTaskAsync(endpoint));
            }

            object topJson = JObject.Parse(topResponse);
>>>>>>> Coordinate header and signature timestamps to avoid HTTP 401 errors.

            DocDbRestQueryResult result = await QueryDocDbInternal(endpoint, queryString, queryParams, DocDbResourceType.Database, "");
            IEnumerable databases = result.ResultSet as IEnumerable;

            if (databases != null)
            {
                foreach (object database in databases)
                {
                    if (database != null)
                    {
                        object id = ReflectionHelper.GetNamedPropertyValue(database, "id", true, false);

                        if ((id != null) && (id.ToString() == this._dbName))
                        {
                            object rid = ReflectionHelper.GetNamedPropertyValue(database, "_rid", true, false);

                            if (rid != null)
                            {
                                this._dbId = rid.ToString();
                            }
                        }
                    }
                }
            }

            if (string.IsNullOrWhiteSpace(_dbId))
            {
                await CreateDatabase();
                await CreateDeviceCollection();
            }
        }

        private async Task CreateDatabase()
        {
<<<<<<< HEAD
            string endpoint = string.Format("{0}dbs", _docDbEndpoint);
            JObject body = new JObject();
            body.Add("id", _dbName);
            string response = await PerformRestCallAsync(endpoint, POST_VERB, DocDbResourceType.Database, "", body.ToString());

            JObject json = JObject.Parse(response);

            _collectionId = ReflectionHelper.GetNamedPropertyValue(json, "_rid", true, false).ToString();
=======
            string response;

            string endpoint = string.Format("{0}dbs", _docDbEndpoint);
            JObject body = new JObject();
            body.Add("id", _dbName);
            using (WebClient client = BuildWebClient())
            {
                client.Headers.Add(AUTHORIZATION_HEADER_KEY, GetAuthorizationToken("POST", DATABASE_RESOURCE_TYPE, ""));
                response = await AzureRetryHelper.OperationWithBasicRetryAsync<string>(async () =>
                    await client.UploadStringTaskAsync(endpoint, "POST", body.ToString())); 

                object json = JObject.Parse(response);

                _dbId =
                ReflectionHelper.GetNamedPropertyValue(
                    json,
                    "_rid",
                    true,
                    false).ToString();
            }
>>>>>>> Coordinate header and signature timestamps to avoid HTTP 401 errors.
        }

        public async Task InitializeDeviceCollection()
        {
<<<<<<< HEAD
            string endpoint = string.Format("{0}dbs/{1}/colls", _docDbEndpoint, _dbId);
            string queryString = "SELECT * FROM colls c WHERE (c.id = @id)";
            var queryParams = new Dictionary<string, object>();
            queryParams.Add("@id", _collectionName);
=======
            IEnumerable collections;
            string topResponse;

            string endpoint = string.Format("{0}dbs/{1}/colls", _docDbEndpoint, _dbId);
            using (WebClient client = BuildWebClient())
            {
                client.Headers.Add(AUTHORIZATION_HEADER_KEY, GetAuthorizationToken("GET", COLLECTION_RESOURCE_TYPE, _dbId));
                topResponse = await AzureRetryHelper.OperationWithBasicRetryAsync<string>(async () => 
                    await client.DownloadStringTaskAsync(endpoint));
            }

            object topJson = JObject.Parse(topResponse);
>>>>>>> Coordinate header and signature timestamps to avoid HTTP 401 errors.

            DocDbRestQueryResult result = await QueryDocDbInternal(endpoint, queryString, queryParams, DocDbResourceType.Collection, _dbId);
            IEnumerable collections = result.ResultSet as IEnumerable;

            if (collections != null)
            {
                foreach (object col in collections)
                {
                    object id = ReflectionHelper.GetNamedPropertyValue(col, "id", true, false);

                    if ((id != null) && (id.ToString() == this._collectionName))
                    {
                        object rid = ReflectionHelper.GetNamedPropertyValue(col, "_rid", true, false);

                        if (rid != null)
                        {
                            this._collectionId = rid.ToString();
                            return;
                        }
                    }
                }
            }

            if (string.IsNullOrWhiteSpace(_collectionId))
            {
                await CreateDeviceCollection();
            }
        }

        private async Task CreateDeviceCollection()
        {
<<<<<<< HEAD
=======
            string response;
<<<<<<< HEAD
            var utcNow = DateTime.UtcNow;
>>>>>>> Coordinate header and signature timestamps to avoid HTTP 401 errors.
=======

>>>>>>> Revert "Coordinate header and signature timestamps to avoid HTTP 401 errors."
            string endpoint = string.Format("{0}dbs/{1}/colls", _docDbEndpoint, _dbId);
            var body = new JObject();
            body.Add("id", _collectionName);
<<<<<<< HEAD
<<<<<<< HEAD
            string response = await PerformRestCallAsync(endpoint, POST_VERB, DocDbResourceType.Collection, _dbId, body.ToString());

            JObject json = JObject.Parse(response);

            _collectionId = ReflectionHelper.GetNamedPropertyValue(json, "_rid", true, false).ToString();
=======
            using (WebClient client = BuildWebClient(utcNow))
=======
            using (WebClient client = BuildWebClient())
>>>>>>> Revert "Coordinate header and signature timestamps to avoid HTTP 401 errors."
            {
                client.Headers.Add(AUTHORIZATION_HEADER_KEY, GetAuthorizationToken("POST", COLLECTION_RESOURCE_TYPE, _dbId));
                response = await AzureRetryHelper.OperationWithBasicRetryAsync<string>(async () =>
                    await client.UploadStringTaskAsync(endpoint, "POST", body.ToString())); 

                object json = JObject.Parse(response);

                _collectionId =
                ReflectionHelper.GetNamedPropertyValue(
                    json,
                    "_rid",
                    true,
                    false).ToString();
            }
>>>>>>> Coordinate header and signature timestamps to avoid HTTP 401 errors.
        }

        /// <summary>
        /// Queries the device collection
        /// https://msdn.microsoft.com/en-us/library/azure/dn783363.aspx
        /// </summary>
        /// <param name="queryString"></param>
        /// <param name="queryParameters"></param>
        /// <returns>One page of device results, with metadata</returns>
        public async Task<DocDbRestQueryResult> QueryDeviceManagementCollectionAsync(
            string queryString, Dictionary<string, Object> queryParams, int pageSize = -1, string continuationToken = null)
        {
<<<<<<< HEAD
<<<<<<< HEAD
            if (string.IsNullOrWhiteSpace(queryString))
            {
                throw new ArgumentException("queryString is null or whitespace");
            }
            string endpoint = string.Format("{0}dbs/{1}/colls/{2}/docs", _docDbEndpoint, _dbId, _collectionId);
            return await QueryDocDbInternal(endpoint, queryString, queryParams, DocDbResourceType.Document, _collectionId, pageSize, continuationToken);
        }
=======
            var utcNow = DateTime.UtcNow;
            WebClient client = BuildWebClient(utcNow);
=======
            WebClient client = BuildWebClient();
>>>>>>> Revert "Coordinate header and signature timestamps to avoid HTTP 401 errors."
            client.Headers.Set("Content-Type", "application/query+json");
            client.Headers.Add(AUTHORIZATION_HEADER_KEY, GetAuthorizationTokenForDeviceManagementCollectionQuery("POST"));
            client.Headers.Add("x-ms-documentdb-isquery", "true");
>>>>>>> Coordinate header and signature timestamps to avoid HTTP 401 errors.

        private async Task<DocDbRestQueryResult> QueryDocDbInternal(string endpoint, string queryString, Dictionary<string, Object> queryParams, 
            DocDbResourceType resourceType, string resourceId, int pageSize = -1, string continuationToken = null)
        {
            if (string.IsNullOrWhiteSpace(endpoint))
            {
                throw new ArgumentException("endpoint is null or whitespace");
            }

            if (string.IsNullOrWhiteSpace(queryString))
            {
                throw new ArgumentException("queryString is null or whitespace");
            }

            using (WebClient client = new WebClient())
            {
                client.Encoding = System.Text.Encoding.UTF8;
                client.Headers.Add(CONTENT_TYPE_HEADER_KEY, APPLICATION_QUERY_JSON);
                client.Headers.Add(ACCEPT_HEADER_KEY, APPLICATION_JSON);
                client.Headers.Add(VERSION_HEADER_KEY, X_MS_VERSION);

                // https://msdn.microsoft.com/en-us/library/azure/dn783368.aspx
                // The date of the request, as specified in RFC 1123. The date format is expressed in
                // Coordinated Universal Time (UTC), for example. Fri, 08 Apr 2015 03:52:31 GMT.
                string formattedTimeString = DateTime.UtcNow.ToString("R", CultureInfo.InvariantCulture).ToLowerInvariant();
                client.Headers.Add(DATE_HEADER_KEY, formattedTimeString);
                client.Headers.Add(AUTHORIZATION_HEADER_KEY, GetAuthorizationToken(POST_VERB, DocDbResourceTypeHelper.GetResourceTypeString(resourceType), resourceId, formattedTimeString));
                client.Headers.Add(IS_QUERY_HEADER_KEY, "true");

                if (pageSize >= 0)
                {
                    client.Headers.Add(MAX_ITEMS_HEADER_KEY, pageSize.ToString());
                }
                if (continuationToken != null && continuationToken.Length > 0)
                {
                    client.Headers.Add(CONTINUATION_HEADER_KEY, continuationToken);
                }

                var body = new JObject();
                body.Add("query", queryString);
                if (queryParams != null && queryParams.Count > 0)
                {
                    var paramsArray = new JArray();
                    foreach (string key in queryParams.Keys)
                    {
                        var param = new JObject();
                        param.Add("name", key);
                        param.Add("value", JToken.FromObject(queryParams[key]));
                        paramsArray.Add(param);
                    }
                    body.Add("parameters", paramsArray);
                }

                var result = new DocDbRestQueryResult();
                string response = await AzureRetryHelper.OperationWithBasicRetryAsync<string>(async () => await client.UploadStringTaskAsync(endpoint, POST_VERB, body.ToString()));
                JObject responseJobj = JObject.Parse(response);
                JToken jsonResultSet = responseJobj.GetValue(DocDbResourceTypeHelper.GetResultSetKey(resourceType));
                if (jsonResultSet != null)
                {
                    result.ResultSet = (JArray)jsonResultSet;
                }

                WebHeaderCollection responseHeaders = client.ResponseHeaders;

                string count = responseHeaders[ITEM_COUNT_RESPONSE_HEADER_KEY];
                if (!string.IsNullOrEmpty(count))
                {
                    result.TotalResults = int.Parse(count);
                }
                result.ContinuationToken = responseHeaders[CONTINUATION_HEADER_KEY];

                return result; 
            }
        }

        public async Task<JObject> SaveNewDeviceAsync(dynamic device)
        {
<<<<<<< HEAD
<<<<<<< HEAD
            if (device == null)
            {
                throw new ArgumentNullException("device");
            }
=======
            var utcNow = DateTime.UtcNow;
            WebClient client = BuildWebClient(utcNow);
            client.Headers.Add(AUTHORIZATION_HEADER_KEY, GetAuthorizationTokenForDeviceManagementCollectionQuery("POST", utcNow));
>>>>>>> Coordinate header and signature timestamps to avoid HTTP 401 errors.
=======
            WebClient client = BuildWebClient();
            client.Headers.Add(AUTHORIZATION_HEADER_KEY, GetAuthorizationTokenForDeviceManagementCollectionQuery("POST"));
>>>>>>> Revert "Coordinate header and signature timestamps to avoid HTTP 401 errors."

            string endpoint = string.Format("{0}dbs/{1}/colls/{2}/docs", _docDbEndpoint, _dbId, _collectionId);
            if (device.id == null)
            {
                device.id = Guid.NewGuid().ToString();
            }
            string response = await PerformRestCallAsync(endpoint, POST_VERB, DocDbResourceType.Document, _collectionId, device.ToString());

            return JObject.Parse(response);
        }

        /// <summary>
        /// Update the record for an existing device.
        /// </summary>
        /// <param name="updatedDevice"></param>
        /// <returns></returns>
        public async Task<JObject> UpdateDeviceAsync(dynamic updatedDevice)
        {
<<<<<<< HEAD
            if (updatedDevice == null)
            {
                throw new ArgumentNullException("updatedDevice");
            }
=======
            string rid = DeviceSchemaHelper.GetDocDbRid(updatedDevice);
<<<<<<< HEAD
            var utcNow = DateTime.UtcNow;
            WebClient client = BuildWebClient(utcNow);
            client.Headers.Add(AUTHORIZATION_HEADER_KEY, GetAuthorizationToken("PUT", DOCUMENTS_RESOURCE_TYPE, rid, utcNow));
>>>>>>> Coordinate header and signature timestamps to avoid HTTP 401 errors.
=======

            WebClient client = BuildWebClient();
            client.Headers.Add(AUTHORIZATION_HEADER_KEY, GetAuthorizationToken("PUT", DOCUMENTS_RESOURCE_TYPE, rid));
>>>>>>> Revert "Coordinate header and signature timestamps to avoid HTTP 401 errors."

            string rid = DeviceSchemaHelper.GetDocDbRid(updatedDevice);
            string endpoint = string.Format("{0}dbs/{1}/colls/{2}/docs/{3}", _docDbEndpoint, _dbId, _collectionId, rid);
            string response = await PerformRestCallAsync(endpoint, PUT_VERB, DocDbResourceType.Document, rid, updatedDevice.ToString());

            return JObject.Parse(response);
        }

        /// <summary>
        /// Remove a device from the DocumentDB. If it succeeds the method will return asynchronously.
        /// If it fails for any reason it will let any exception thrown bubble up.
        /// </summary>
        /// <param name="device"></param>
        /// <returns></returns>
        public async Task DeleteDeviceAsync(dynamic device)
        {
<<<<<<< HEAD
            if (device == null)
            {
                throw new ArgumentNullException("device");
            }
=======
            string rid = DeviceSchemaHelper.GetDocDbRid(device);
<<<<<<< HEAD
            var utcNow = DateTime.UtcNow;
            WebClient client = BuildWebClient(utcNow);
            client.Headers.Add(AUTHORIZATION_HEADER_KEY, GetAuthorizationToken("DELETE", DOCUMENTS_RESOURCE_TYPE, rid, utcNow));
>>>>>>> Coordinate header and signature timestamps to avoid HTTP 401 errors.
=======

            WebClient client = BuildWebClient();
            client.Headers.Add(AUTHORIZATION_HEADER_KEY, GetAuthorizationToken("DELETE", DOCUMENTS_RESOURCE_TYPE, rid));
>>>>>>> Revert "Coordinate header and signature timestamps to avoid HTTP 401 errors."

            string rid = DeviceSchemaHelper.GetDocDbRid(device);
            string endpoint = string.Format("{0}dbs/{1}/colls/{2}/docs/{3}", _docDbEndpoint, _dbId, _collectionId, rid);
<<<<<<< HEAD
            await PerformRestCallAsync(endpoint, DELETE_VERB, DocDbResourceType.Document, rid, "");
        }

        private async Task<string> PerformRestCallAsync(string endpoint, string httpVerb, DocDbResourceType resourceType, string resourceId, string body)
        {
            using (WebClient webClient = new WebClient())
            {
                webClient.Encoding = System.Text.Encoding.UTF8;
                webClient.Headers.Add(CONTENT_TYPE_HEADER_KEY, APPLICATION_JSON);
                webClient.Headers.Add(ACCEPT_HEADER_KEY, APPLICATION_JSON);
                webClient.Headers.Add(VERSION_HEADER_KEY, X_MS_VERSION);

                // https://msdn.microsoft.com/en-us/library/azure/dn783368.aspx
                // The date of the request, as specified in RFC 1123. The date format is expressed in
                // Coordinated Universal Time (UTC), for example. Fri, 08 Apr 2015 03:52:31 GMT.
                string formattedTimeString = DateTime.UtcNow.ToString("R", CultureInfo.InvariantCulture).ToLowerInvariant();
                webClient.Headers.Add(DATE_HEADER_KEY, formattedTimeString);
                webClient.Headers.Add(AUTHORIZATION_HEADER_KEY, GetAuthorizationToken(httpVerb, DocDbResourceTypeHelper.GetResourceTypeString(resourceType), resourceId, formattedTimeString));

                return await AzureRetryHelper.OperationWithBasicRetryAsync<string>(async () => await webClient.UploadStringTaskAsync(endpoint, httpVerb, body));
            }
=======

            await AzureRetryHelper.OperationWithBasicRetryAsync(async() =>
                await client.UploadStringTaskAsync(endpoint, "DELETE", ""));
        }

        /// <summary>
        /// Builds the necessary headers and adds them to the WebClient that will be used for the request. This does
        /// NOT include the required Authorization header, which may be different for various requests and must be
        /// handled by the calling method before making the request
        /// </summary>
        /// <param name="webClient">Required: The WebClient that will be used for the request. 
        /// The headers will be added to this client</param>
        /// <param name="pageItemCount">Optional: If the request will be made in pages this is the
        /// number of items per page</param>
        /// <param name="continuationToken">Optional: If the request will be made in pages, and you have a continuation token 
        /// from a previous page, this will ensure the next page begins at the right place</param>
        private WebClient BuildWebClient()
        {
            var webClient = new WebClient();
            webClient.Encoding = System.Text.Encoding.UTF8;
            webClient.Headers.Add("Content-Type", APPLICATION_JSON);
            webClient.Headers.Add("Accept", APPLICATION_JSON);
            webClient.Headers.Add("x-ms-version", X_MS_VERSION);

            // The date of the request, as specified in RFC 1123. The date format is expressed in
            // Coordinated Universal Time (UTC), for example. Fri, 08 Apr 2015 03:52:31 GMT.
            webClient.Headers.Add("x-ms-date", DateTime.UtcNow.ToString("R", CultureInfo.InvariantCulture));

            return webClient;
        }

        /// <summary>
        /// This is a helper method that will pass through to getAuthorizationToken with the correct values
        /// for a query to the Device Management Collection.
        /// </summary>
        /// <param name="requestVerb">GET, PUT, POST, DELETE, etc</param>
        /// <returns></returns>
        private string GetAuthorizationTokenForDeviceManagementCollectionQuery(string requestVerb)
        {
<<<<<<< HEAD
            return GetAuthorizationToken(requestVerb, DOCUMENTS_RESOURCE_TYPE, _collectionId, utcNow);
>>>>>>> Coordinate header and signature timestamps to avoid HTTP 401 errors.
=======
            return GetAuthorizationToken(requestVerb, DOCUMENTS_RESOURCE_TYPE, _collectionId);
>>>>>>> Revert "Coordinate header and signature timestamps to avoid HTTP 401 errors."
        }

        /// <summary>
        /// Build up the authorization string that should be put in the authorization header on the WebClient.
        /// This header is required for all requests
        /// </summary>
        /// <param name="requestVerb">GET, PUT, POST, DELETE, etc</param>
        /// <param name="resourceType">The type of resource you are accessing -- colls for
        /// collections, for example</param>
        /// <param name="resourceId">The resource ID provided in the Azure portal. It should be a
        /// very short hash-looking string similar to jNHDTMVaDgB=</param>
        /// <returns></returns>
        [SuppressMessage(
            "Microsoft.Globalization", 
            "CA1308:NormalizeStringsToUppercase",
            Justification = "Token signatures are base on lower-case strings.")]
<<<<<<< HEAD
<<<<<<< HEAD
        private string GetAuthorizationToken(string requestVerb, string resourceType, string resourceId, string formattedTimeString)
        {
=======
        private string GetAuthorizationToken(string requestVerb, string resourceType, string resourceId, DateTime utcNow)
=======
        private string GetAuthorizationToken(string requestVerb, string resourceType, string resourceId)
>>>>>>> Revert "Coordinate header and signature timestamps to avoid HTTP 401 errors."
        {
            // https://msdn.microsoft.com/en-us/library/azure/dn783368.aspx
            // The date portion of the string is the date and time the message was sent
            // (in "HTTP-date" format as defined by RFC 7231 Date/Time Formats) e.g. Tue, 15 Nov 1994 08:12:31 GMT.
            string dateString = DateTime.UtcNow.ToString("r", CultureInfo.InvariantCulture);

>>>>>>> Coordinate header and signature timestamps to avoid HTTP 401 errors.
            string signatureRaw = 
                string.Format(CultureInfo.InvariantCulture, "{0}\n{1}\n{2}\n{3}\n\n", requestVerb, resourceType, resourceId, formattedTimeString)
                .ToLowerInvariant();

            byte[] sigBytes = Encoding.UTF8.GetBytes(signatureRaw);
            byte[] keyBytes = Convert.FromBase64String(_docDbKey);

            byte[] hashBytes;
            using (HashAlgorithm algo = new HMACSHA256(keyBytes))
            {
                hashBytes = algo.ComputeHash(sigBytes);
            }

            string hashString = Convert.ToBase64String(hashBytes);

            return Uri.EscapeDataString(string.Format(CultureInfo.InvariantCulture, "type=master&ver=1.0&sig={0}", hashString));
        }
    }
}
