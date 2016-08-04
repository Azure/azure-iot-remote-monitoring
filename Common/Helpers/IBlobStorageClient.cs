using System;
using System.Threading.Tasks;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Blob;

namespace Microsoft.Azure.Devices.Applications.RemoteMonitoring.Common.Helpers
{
    public interface IBlobStorageClient
    {
        Task UploadFromByteArrayAsync(string blobName, byte[] buffer, int index, int count,
            AccessCondition accessCondition,
            BlobRequestOptions options, OperationContext operationContext);

        Task<byte[]> GetBlobData(string blobName);
        Task<string> GetBlobEtag(string blobName);
        Task UploadTextAsync(string blobName, string data);
        Task<IBlobStorageReader> GetReader(string blobPrefix, DateTime? minTime = null);
    }
}