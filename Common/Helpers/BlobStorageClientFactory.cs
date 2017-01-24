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
            return _blobStorageClient ??
                   (_blobStorageClient = new BlobStorageClient(storageConnectionString, containerName));
        }
    }
}