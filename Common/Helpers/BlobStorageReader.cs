using System.Collections;
using System.Collections.Generic;
using System.IO;
using Microsoft.WindowsAzure.Storage.Blob;

namespace Microsoft.Azure.Devices.Applications.RemoteMonitoring.Common.Helpers
{
    public interface IBlobStorageReader : IEnumerable<Stream>
    {
        
    }

    internal class BlobStorageReader : IBlobStorageReader
    {
        private readonly IEnumerable<IListBlobItem> _blobs;

        public BlobStorageReader(IEnumerable<IListBlobItem> blobs)
        {
            this._blobs = blobs;
        }


        public IEnumerator<Stream> GetEnumerator()
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
                yield return stream;
            }
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
    }
}
