namespace Microsoft.Azure.Devices.Applications.RemoteMonitoring.Common.Helpers
{
    public class BlobStorageClientFactory : IBlobStorageClientFactory
    {
        private IBlobStorageClient _blobStorageClient;

        public BlobStorageClientFactory() : this(null)
        {
        }

        public BlobStorageClientFactory(IBlobStorageClient customClient)
        {
            _blobStorageClient = customClient;
        }

        public IBlobStorageClient CreateClient(string storageConnectionString, string containerName)
        {
            if (_blobStorageClient == null)
            {
                _blobStorageClient = new BlobStorageClient(storageConnectionString, containerName);
            }
            return _blobStorageClient;
        }
    }
}