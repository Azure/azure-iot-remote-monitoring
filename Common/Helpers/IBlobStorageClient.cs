using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Blob;

namespace Microsoft.Azure.Devices.Applications.RemoteMonitoring.Common.Helpers
{
    public interface IBlobStorageClient
    {
        Task UploadFromByteArrayAsync(byte[] buffer, int index, int count, AccessCondition accessCondition,
            BlobRequestOptions options, OperationContext operationContext);
        Task<byte[]> GetBlobData();
        Task<string> GetBlobEtag();
        Task UploadTextAsync(string data);
        Task<IBlobStorageReader> GetReader(string blobPrefix, DateTime? minTime = null);
    }
}