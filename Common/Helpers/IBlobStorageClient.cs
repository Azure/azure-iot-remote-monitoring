using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Blob;

namespace Microsoft.Azure.Devices.Applications.RemoteMonitoring.Common.Helpers
{
    public interface IBlobStorageClient
    {
        Task<CloudBlobContainer> BuildBlobContainerAsync();
        DateTime? ExtractBlobItemDate(IListBlobItem blobItem);
        Task<IEnumerable<IListBlobItem>> LoadBlobItemsAsync(
            Func<BlobContinuationToken, Task<BlobResultSegment>> segmentLoader);

        Task UploadFromByteArrayAsync(byte[] buffer, int index, int count, AccessCondition accessCondition,
            BlobRequestOptions options, OperationContext operationContext);

        Task<byte[]> GetBlobData();
        Task<string> GetBlobEtag();
        Task UploadTextAsync(string data, string format, string dateString, string timeString);
    }
}