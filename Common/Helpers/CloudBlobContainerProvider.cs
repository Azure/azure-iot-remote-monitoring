using System.Threading.Tasks;
using Microsoft.WindowsAzure.Storage.Blob;
using Microsoft.WindowsAzure.Storage.Table;

namespace Microsoft.Azure.Devices.Applications.RemoteMonitoring.Common.Helpers
{
    public class CloudBlobContainerProvider : ICloudBlobContainerProvider
    {
        private readonly CloudBlobContainer _container;

        public CloudBlobContainerProvider(CloudBlobClient blobClient, string containerName)
        {
            _container = blobClient.GetContainerReference(containerName);
        }

        public async Task<CloudBlobContainer> GetCloudBlobContainerAsync()
        {
            await _container.CreateIfNotExistsAsync();
            return _container;
        }
    }
}