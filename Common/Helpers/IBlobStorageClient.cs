using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Blob;
using System.Collections.Generic;

namespace Microsoft.Azure.Devices.Applications.RemoteMonitoring.Common.Helpers
{
    public interface IBlobStorageClient
    {
        Task UploadFromByteArrayAsync(string blobName, byte[] buffer, int index, int count,
            AccessCondition accessCondition,
            BlobRequestOptions options, OperationContext operationContext);
        Task<CloudBlockBlob> UploadFromStreamAsync(string blobName, string contentType, Stream stream, AccessCondition condition, BlobRequestOptions options, OperationContext context);
        Task<CloudBlockBlob> GetBlob(string blobName);
        Task<List<ICloudBlob>> ListBlobs(string blobPrefix, bool useFlatBlobListing);
        Task<CloudBlockBlob> MoveBlob(string sourceName, string targetName);
        Task<bool> DeleteBlob(string blobName);
        Task<string> GetContainerUri();
        Task<byte[]> GetBlobData(string blobName);
        Task<string> GetBlobEtag(string blobName);
        Task UploadTextAsync(string blobName, string data);
        Task<IBlobStorageReader> GetReader(string blobPrefix, DateTime? minTime = null);
        Task CreateAccessPolicyIfNotExist(BlobContainerPublicAccessType publicAccessType,string policyName, SharedAccessBlobPolicy policy);
    }
}