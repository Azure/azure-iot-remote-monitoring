using System;
using System.IO;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using Microsoft.Azure.Devices.Applications.RemoteMonitoring.Common.Configurations;
using Microsoft.Azure.Documents;
using Microsoft.Azure.Documents.Client;
using Newtonsoft.Json;

namespace Microsoft.Azure.Devices.Applications.RemoteMonitoring.Common.Helpers
{
    /// <summary>
    /// Wrapper over document db client.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class DocumentDBClient<T> : IDocumentDBClient<T>, IDisposable where T : new()
    {
        private bool _initialized;
        private readonly string _databaseId;
        private readonly string _collectionName;
        private readonly DocumentClient _client;
        private readonly object _initializeLock = new Object();

        /// <summary>
        /// Creates a new instance of <see cref="DocumentDBClient"/>
        /// </summary>
        /// <param name="configurationProvider"></param>
        public DocumentDBClient(IConfigurationProvider configurationProvider)
        {
            string endpointUrl = configurationProvider.GetConfigurationSettingValue("docdb.EndpointUrl");
            string primaryAuthorizationKey = configurationProvider.GetConfigurationSettingValue("docdb.PrimaryAuthorizationKey");

            _client = new DocumentClient(new Uri(endpointUrl), primaryAuthorizationKey);
            _databaseId = configurationProvider.GetConfigurationSettingValue("docdb.DatabaseId");
            _collectionName = configurationProvider.GetConfigurationSettingValue("docdb.DocumentCollectionId");
        }

        /// <summary>
        /// Gets a document by its id.
        /// </summary>
        /// <param name="id">The id of the document to get</param>
        public async Task<T> GetAsync(string id)
        {
            await InitializeDatabaseIfRequired();
            var response = await _client.ReadDocumentAsync(UriFactory.CreateDocumentUri(_databaseId, _collectionName, id));
            return await Deserialize(response.Resource);

        }

        /// <summary>
        /// Returns a <see cref="IQueryable{T}"/> that can be used to query db.
        /// </summary>
        public async Task<IQueryable<T>> QueryAsync()
        {
            await InitializeDatabaseIfRequired();
            return _client.CreateDocumentQuery<T>(UriFactory.CreateDocumentCollectionUri(_databaseId, _collectionName));
        }

        /// <summary>
        /// Saves a document to the the db.
        /// </summary>
        /// <param name="data">The data of the document to save.</param>
        public async Task<T> SaveAsync(T data)
        {
            await InitializeDatabaseIfRequired();
            var response = await _client.UpsertDocumentAsync(UriFactory.CreateDocumentCollectionUri(_databaseId, _collectionName), data);
            return await Deserialize(response.Resource);
        }

        /// <summary>
        /// Deletes a document from the db.
        /// </summary>
        /// <param name="id">The id of the document to delete</param>
        public async Task DeleteAsync(string id)
        {
            await InitializeDatabaseIfRequired();
            await _client.DeleteDocumentAsync(UriFactory.CreateDocumentUri(_databaseId, _collectionName, id));
        }

        private async Task InitializeDatabaseIfRequired()
        {
            if (!_initialized)
            {
                await InitializeDatabase();
                await InitializeCollection();
                _initialized = true;
            }
        }

        private async Task InitializeDatabase()
        {
            try
            {
                await _client.ReadDatabaseAsync(UriFactory.CreateDatabaseUri(_databaseId));
            }
            catch (DocumentClientException dce)
            {
                if (dce.StatusCode == HttpStatusCode.NotFound)
                {
                    await _client.CreateDatabaseAsync(new Database { Id = _databaseId });
                    return;
                }

                throw;
            }
        }

        private async Task InitializeCollection()
        {
            try
            {
                await _client.ReadDocumentCollectionAsync(UriFactory.CreateDocumentCollectionUri(_databaseId, _collectionName));
            }
            catch (DocumentClientException dce)
            {
                if (dce.StatusCode == HttpStatusCode.NotFound)
                {
                    await _client.CreateDocumentCollectionAsync(
                        UriFactory.CreateDatabaseUri(_databaseId),
                        new DocumentCollection { Id = _collectionName });
                    return;
                }

                throw;
            }
        }

        private async Task<T> Deserialize(Document document)
        {
            using (var documentStream = new MemoryStream())
            using (var reader = new StreamReader(documentStream))
            {
                document.SaveTo(documentStream);
                documentStream.Position = 0;
                var rawDocumentData = await reader.ReadToEndAsync();
                return JsonConvert.DeserializeObject<T>(rawDocumentData);
            }
        }

        #region IDisposable Support
        private bool disposedValue = false; // To detect redundant calls

        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                    _client.Dispose();
                }

                // TODO: free unmanaged resources (unmanaged objects) and override a finalizer below.
                // TODO: set large fields to null.

                disposedValue = true;
            }
        }

        // TODO: override a finalizer only if Dispose(bool disposing) above has code to free unmanaged resources.
        // ~DocumentDBClient() {
        //   // Do not change this code. Put cleanup code in Dispose(bool disposing) above.
        //   Dispose(false);
        // }

        // This code added to correctly implement the disposable pattern.
        public void Dispose()
        {
            // Do not change this code. Put cleanup code in Dispose(bool disposing) above.
            Dispose(true);
            // TODO: uncomment the following line if the finalizer is overridden above.
            // GC.SuppressFinalize(this);
        }
        #endregion
    }
}
