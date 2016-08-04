using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using Microsoft.WindowsAzure.Storage.Blob;

namespace Microsoft.Azure.Devices.Applications.RemoteMonitoring.Common.Helpers
{
    public interface IBlobStorageReader : IEnumerable<Tuple<Stream, DateTime?>>
    {
    }

    internal class BlobStorageReader : IBlobStorageReader
    {
        private readonly IEnumerable<IListBlobItem> _blobs;

        public BlobStorageReader(IEnumerable<IListBlobItem> blobs)
        {
            _blobs = blobs;
        }


        public IEnumerator<Tuple<Stream, DateTime?>> GetEnumerator()
        {
            CloudBlockBlob blockBlob;
            foreach (var blob in _blobs)
            {
                if ((blockBlob = blob as CloudBlockBlob) == null)
                {
                    continue;
                }
                var stream = new MemoryStream();
                blockBlob.DownloadToStream(stream);
                yield return
                    new Tuple<Stream, DateTime?>(stream, blockBlob.Properties.LastModified?.LocalDateTime);
            }
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
    }
}