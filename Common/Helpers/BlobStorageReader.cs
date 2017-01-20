using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Caching;
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
            foreach (var blockBlob in _blobs.OfType<CloudBlockBlob>())
            {
                yield return new BlobContents
                {
                    Data = ReadBlockWithCache(blockBlob),
                    LastModifiedTime = blockBlob.Properties.LastModified?.LocalDateTime
                };
            }
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        static private MemoryCache _blobCache = new MemoryCache("blobCache");

        static private MemoryStream ReadBlockWithCache(CloudBlob blob)
        {
            var stream = new MemoryStream();
            stream = _blobCache.AddOrGetExisting(
                blob.Uri.ToString(),
                stream,
                new CacheItemPolicy
                {
                    SlidingExpiration = TimeSpan.FromHours(2),
                    RemovedCallback = OnItemRemoved
                }) as MemoryStream
                ?? stream;

            lock (stream)
            {
                var length = blob.Properties.Length - stream.Length;
                if (length > 0)
                {
                    try
                    {
                        blob.DownloadRangeToStream(stream, stream.Length, length);
                    }
                    catch
                    {
                        // Nothing to do since caller will try to read periodically
                    }
                }

                return new MemoryStream(stream.GetBuffer(), 0, (int)stream.Length, false);
            }
        }

        static void OnItemRemoved(CacheEntryRemovedArguments arg)
        {
            var stream = arg.CacheItem.Value as MemoryStream;
            if (stream != null)
            {
                stream.Dispose();
            }
        }
    }
}