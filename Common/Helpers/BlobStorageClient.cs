using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Blob;

namespace Microsoft.Azure.Devices.Applications.RemoteMonitoring.Common.Helpers
{
    /// <summary>
    /// Helper methods, related to blob storage.
    /// </summary>
    public class BlobStorageClient : IBlobStorageClient
    {
        private CloudBlobContainer _container;
        private CloudBlockBlob _blob;
        private readonly CloudBlobClient _blobClient;
        private readonly string _containerName;
        private readonly string _blobName;

        public BlobStorageClient(string connectionString, string containerName, string blobName)
        {
            CloudStorageAccount storageAccount = CloudStorageAccount.Parse(connectionString);
            _blobClient = storageAccount.CreateCloudBlobClient();
            _containerName = containerName;
            _blobName = blobName;
        }
        private async Task<CloudBlobContainer> GetCloudBlobContainerAsync()
        {
            if (_container == null && _containerName != null)
            {
                _container = _blobClient.GetContainerReference(_containerName);
                await _container.CreateIfNotExistsAsync();
            }
            return _container;
        }

        private async Task<CloudBlockBlob> GetCloudBlockBlobAsync()
        {
            if (_blob == null && _blobName != null)
            {
                CloudBlobContainer container = await GetCloudBlobContainerAsync();
                _blob = container.GetBlockBlobReference(_blobName);
            }
            return _blob;
        }
        //private async Task<CloudBlockBlob> GetCloudBlockBlobAsync(string format, string dateString, string timeString)
        //{
        //    if (_blob == null && _blobName != null)
        //    {
        //        CloudBlobContainer container = await GetCloudBlobContainerAsync();
        //        _blob = container.GetBlockBlobReference(format, dateString, timeString, _blobName);
        //    }
        //    return _blob;
        //}

        public async Task UploadFromByteArrayAsync(byte[] buffer, int index, int count, AccessCondition accessCondition, BlobRequestOptions options, OperationContext operationContext)
        {
            CloudBlockBlob blob = await this.GetCloudBlockBlobAsync();
            await blob.UploadFromByteArrayAsync(
                buffer,
                index,
                count,
                accessCondition,
                options,
                operationContext);
        }
        
        public async Task<byte[]> GetBlobData()
        {
            CloudBlockBlob blob = await this.GetCloudBlockBlobAsync();
            bool exists = await blob.ExistsAsync();
            if (exists)
            {
                await blob.FetchAttributesAsync();
                long blobLength = blob.Properties.Length;

                if (blobLength > 0)
                {
                    byte[] existingBytes = new byte[blobLength];
                    await blob.DownloadToByteArrayAsync(existingBytes, 0);
                    return existingBytes;
                }
            }
            return null;
        }

        public async Task<string> GetBlobEtag()
        {
            CloudBlockBlob blob = await this.GetCloudBlockBlobAsync();
            return blob.Properties.ETag;
        }

        public async Task UploadTextAsync(string data, string format, string dateString, string timeString)
        {
            //CloudBlockBlob blob = await this.GetCloudBlockBlobAsync(format, dateString, timeString);
            CloudBlockBlob blob = await this.GetCloudBlockBlobAsync();
            await blob.UploadTextAsync(data);
        }

        /// <summary>
        /// Builds a CloudBlobContainer from provided settings.
        /// </summary>
        /// <param name="connectionString">
        /// A connection string for the Cloud Storage Account to which the 
        /// CloudBlobContainer will belong.
        /// </param>
        /// <param name="containerName">
        /// The CloudBlobContainer's container name.
        /// </param>
        /// <returns>
        /// A CloudBlobContainer, built from provided settings.
        /// </returns>
        public async Task<CloudBlobContainer> BuildBlobContainerAsync()
        {
            return await this.GetCloudBlobContainerAsync();
        }

        /// <summary>
        /// Exctract's a blob item's last modified date.
        /// </summary>
        /// <param name="blobItem">
        /// The blob item, for which to extract a last modified date.
        /// </param>
        /// <returns>
        /// blobItem's last modified date, or null, of such could not be 
        /// extracted.
        /// </returns>
        public DateTime? ExtractBlobItemDate(IListBlobItem blobItem)
        {
            if (blobItem == null)
            {
                throw new ArgumentNullException("blobItem");
            }

            BlobProperties blobProperties;
            CloudBlockBlob blockBlob;
            CloudPageBlob pageBlob;

            if ((blockBlob = blobItem as CloudBlockBlob) != null)
            {
                blobProperties = blockBlob.Properties;
            }
            else if ((pageBlob = blobItem as CloudPageBlob) != null)
            {
                blobProperties = pageBlob.Properties;
            }
            else
            {
                blobProperties = null;
            }

            if ((blobProperties != null) &&
                blobProperties.LastModified.HasValue)
            {
                return blobProperties.LastModified.Value.DateTime;
            }

            return null;
        }

        /// <summary>
        /// Load's a blob listing's items.
        /// </summary>
        /// <param name="segmentLoader">
        /// A func for getting the blob listing's next segment.
        /// </param>
        /// <returns>
        /// A concattenation of all the blob listing's resulting segments.
        /// </returns>
        public async Task<IEnumerable<IListBlobItem>> LoadBlobItemsAsync(
            Func<BlobContinuationToken, Task<BlobResultSegment>> segmentLoader)
        {
            if (segmentLoader == null)
            {
                throw new ArgumentNullException("segmentLoader");
            }

            IEnumerable<IListBlobItem> blobItems = new IListBlobItem[0];

            BlobResultSegment segment = await segmentLoader(null);
            while ((segment != null) &&
                (segment.Results != null))
            {
                blobItems = blobItems.Concat(segment.Results);

                if (segment.ContinuationToken == null)
                {
                    break;
                }

                segment = await segmentLoader(segment.ContinuationToken);
            }

            return blobItems;
        }
    }
}
