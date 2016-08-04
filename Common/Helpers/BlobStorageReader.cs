using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using Microsoft.Azure.Devices.Applications.RemoteMonitoring.Common.Models;
using Microsoft.WindowsAzure.Storage.Blob;

namespace Microsoft.Azure.Devices.Applications.RemoteMonitoring.Common.Helpers
{
    public interface IBlobStorageReader : IEnumerable<BlobContents>
    {
    }

    internal class BlobStorageReader : IBlobStorageReader
    {
        private readonly IEnumerable<IListBlobItem> _blobs;

        public BlobStorageReader(IEnumerable<IListBlobItem> blobs)
        {
            _blobs = blobs;
        }


        public IEnumerator<BlobContents> GetEnumerator()
        {
            foreach (var blob in _blobs)
            {
                CloudBlockBlob blockBlob;
                if ((blockBlob = blob as CloudBlockBlob) == null)
                {
                    continue;
                }
                var stream = new MemoryStream();
                blockBlob.DownloadToStream(stream);
                yield return
                    new BlobContents
                    {
                        Data = stream,
                        LastModifiedTime = blockBlob.Properties.LastModified?.LocalDateTime
                    };
            }
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
    }
}