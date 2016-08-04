using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Blob;

namespace Microsoft.Azure.Devices.Applications.RemoteMonitoring.Common.Helpers
{
    /// <summary>
    ///     Helper methods, related to blob storage.
    /// </summary>
    public class BlobStorageClient : IBlobStorageClient
    {
        private readonly CloudBlobClient _blobClient;
        private readonly string _containerName;
        private CloudBlobContainer _container;
        private readonly CloudStorageAccount _storageAccount;

        public BlobStorageClient(string connectionString, string containerName)
        {
            _storageAccount = CloudStorageAccount.Parse(connectionString);
            _blobClient = _storageAccount.CreateCloudBlobClient();
            _containerName = containerName;
        }

        public async Task UploadFromByteArrayAsync(string blobName, byte[] buffer, int index, int count,
            AccessCondition accessCondition, BlobRequestOptions options, OperationContext operationContext)
        {
            var blob = await CreateCloudBlockBlobAsync(blobName);
            await blob.UploadFromByteArrayAsync(
                buffer,
                index,
                count,
                accessCondition,
                options,
                operationContext);
        }

        public async Task<byte[]> GetBlobData(string blobName)
        {
            var blob = await CreateCloudBlockBlobAsync(blobName);
            var exists = await blob.ExistsAsync();
            if (exists)
            {
                await blob.FetchAttributesAsync();
                var blobLength = blob.Properties.Length;

                if (blobLength > 0)
                {
                    var existingBytes = new byte[blobLength];
                    await blob.DownloadToByteArrayAsync(existingBytes, 0);
                    return existingBytes;
                }
            }
            return null;
        }

        public async Task<string> GetBlobEtag(string blobName)
        {
            var blob = await CreateCloudBlockBlobAsync(blobName);
            return blob.Properties.ETag;
        }

        public async Task UploadTextAsync(string blobName, string data)
        {
            var blob = await CreateCloudBlockBlobAsync(blobName);
            await blob.UploadTextAsync(data);
        }

        public async Task<IBlobStorageReader> GetReader(string prefix, DateTime? minTime = null)
        {
            await CreateCloudBlobContainerAsync();

            var blobs = await LoadBlobItemsAsync(async token =>
            {
                return await _container.ListBlobsSegmentedAsync(
                    prefix,
                    true,
                    BlobListingDetails.None,
                    null,
                    token,
                    null,
                    null);
            });

            if (blobs != null)
            {
                blobs = blobs.OrderByDescending(t => ExtractBlobItemDate(t));
                if (minTime != null)
                {
                    blobs = blobs.Where(t => FilterLessThanTime(t, minTime.Value));
                }
            }

            return new BlobStorageReader(blobs);
        }

        private async Task CreateCloudBlobContainerAsync()
        {
            if (_container == null && _containerName != null)
            {
                _container = _blobClient.GetContainerReference(_containerName);
                await _container.CreateIfNotExistsAsync();
            }
        }

        private async Task<CloudBlockBlob> CreateCloudBlockBlobAsync(string blobName)
        {
            CloudBlockBlob blob;
            if (blobName != null)
            {
                await CreateCloudBlobContainerAsync();
                blob = _container.GetBlockBlobReference(blobName);
                return blob;
            }
            return null;
        }

        private bool FilterLessThanTime(IListBlobItem blobItem, DateTime minTime)
        {
            CloudBlockBlob blockBlob;
            if ((blockBlob = blobItem as CloudBlockBlob) != null)
            {
                if (blockBlob.Properties?.LastModified != null &&
                    (blockBlob.Properties.LastModified.Value.LocalDateTime >= minTime))
                {
                    return true;
                }
            }
            return false;
        }

        /// <summary>
        ///     Exctract's a blob item's last modified date.
        /// </summary>
        /// <param name="blobItem">
        ///     The blob item, for which to extract a last modified date.
        /// </param>
        /// <returns>
        ///     blobItem's last modified date, or null, of such could not be
        ///     extracted.
        /// </returns>
        private DateTime? ExtractBlobItemDate(IListBlobItem blobItem)
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
        ///     Load's a blob listing's items.
        /// </summary>
        /// <param name="segmentLoader">
        ///     A func for getting the blob listing's next segment.
        /// </param>
        /// <returns>
        ///     A concattenation of all the blob listing's resulting segments.
        /// </returns>
        private async Task<IEnumerable<IListBlobItem>> LoadBlobItemsAsync(
            Func<BlobContinuationToken, Task<BlobResultSegment>> segmentLoader)
        {
            if (segmentLoader == null)
            {
                throw new ArgumentNullException("segmentLoader");
            }

            IEnumerable<IListBlobItem> blobItems = new IListBlobItem[0];

            var segment = await segmentLoader(null);
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