using System;
using System.Collections.Generic;
using System.IO;
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

        public async Task<CloudBlockBlob> UploadFromStreamAsync(string blobName, string contentType, Stream stream, AccessCondition condition, BlobRequestOptions options, OperationContext context)
        {
            await CreateCloudBlobContainerAsync();

            var blob = await CreateCloudBlockBlobAsync(blobName);
            await blob.UploadFromStreamAsync(stream, condition, options, context);
            blob.Properties.ContentType = contentType;
            await blob.SetPropertiesAsync();
            return blob;
        }

        public async Task<CloudBlockBlob> GetBlob(string blobName)
        {
            await CreateCloudBlobContainerAsync();
            return _container.GetBlockBlobReference(blobName);
        }

        public async Task<List<ICloudBlob>> ListBlobs(string blobPrefix, bool useFlatBlobListing = true)
        {
            await CreateCloudBlobContainerAsync();

            var blobs = new List<ICloudBlob>();
            foreach (ICloudBlob blob in _container.ListBlobs(blobPrefix, useFlatBlobListing))
            {
                blobs.Add(blob);
            }
            return blobs;
        }

        public async Task<CloudBlockBlob> MoveBlob(string sourceName, string targetName)
        {
            await CreateCloudBlobContainerAsync();

            var sourceBlob = _container.GetBlockBlobReference(sourceName);
            var targetBlob = _container.GetBlockBlobReference(targetName);
            if (targetBlob.Exists())
            {
                await sourceBlob.FetchAttributesAsync();
                // create a new name if conflict with existing icon by appending ticks.
                if (string.IsNullOrEmpty(sourceBlob.Properties.ContentMD5) || !sourceBlob.Properties.ContentMD5.Equals(targetBlob.Properties.ContentMD5))
                {
                    DateTimeOffset timeOffset = sourceBlob.Properties.LastModified ?? DateTime.Now;
                    string newTargetName = string.Format("{0}_{1}", targetName, timeOffset.Ticks.ToString());
                    targetBlob = _container.GetBlockBlobReference(newTargetName);
                    await targetBlob.StartCopyAsync(sourceBlob);
                }
            }
            else
            {
                await targetBlob.StartCopyAsync(sourceBlob);
            }

            var task = sourceBlob.DeleteAsync();
            return targetBlob;
        }

        public async Task<bool> DeleteBlob(string blobName)
        {
            await CreateCloudBlobContainerAsync();

            var blob = _container.GetBlockBlobReference(blobName);
            return await blob.DeleteIfExistsAsync();
        }

        public async Task<string> GetContainerUri()
        {
            await CreateCloudBlobContainerAsync();

            return _container.StorageUri.PrimaryUri.AbsoluteUri;
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

        public async Task CreateAccessPolicyIfNotExist(BlobContainerPublicAccessType publicAccessType, string policyName, SharedAccessBlobPolicy policy)
        {
            if (_container == null && _containerName != null)
            {
                _container = _blobClient.GetContainerReference(_containerName);
                await _container.CreateIfNotExistsAsync(publicAccessType, null, null);

                var currentPermissions = _container.GetPermissions();
                var policies = currentPermissions.SharedAccessPolicies;
                if (!policies.ContainsKey(policyName))
                {
                    policies.Add(policyName, policy);
                    _container.SetPermissions(currentPermissions);
                }
            }
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