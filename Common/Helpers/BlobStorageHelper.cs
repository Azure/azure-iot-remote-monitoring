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
    public static class BlobStorageHelper
    {
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
        public static async Task<CloudBlobContainer> BuildBlobContainerAsync(
            string connectionString,
            string containerName)
        {
            if (string.IsNullOrEmpty(connectionString))
            {
                throw new ArgumentException(
                    "connectionString is a null reference or empty string.",
                    "connectionString");
            }

            if (object.ReferenceEquals(containerName, null))
            {
                throw new ArgumentNullException(containerName);
            }

            CloudStorageAccount storageAccount;
            if (!CloudStorageAccount.TryParse(connectionString, out storageAccount))
            {
                throw new ArgumentException(
                    "connectionString is not a valid Cloud Storage Account connection string.", "connectionString");
            }

            CloudBlobClient blobClient = storageAccount.CreateCloudBlobClient();

            CloudBlobContainer container = blobClient.GetContainerReference(containerName);
            await container.CreateIfNotExistsAsync();

            return container;
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
        public static DateTime? ExtractBlobItemDate(IListBlobItem blobItem)
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
        public static async Task<IEnumerable<IListBlobItem>> LoadBlobItemsAsync(
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
