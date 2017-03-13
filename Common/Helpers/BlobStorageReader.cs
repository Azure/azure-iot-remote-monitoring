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

    class CacheItem
    {
        public MemoryStream Stream;
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
            var item = new CacheItem { Stream = new MemoryStream() };
            item = _blobCache.AddOrGetExisting(
                blob.Uri.ToString(),
                item,
                new CacheItemPolicy
                {
                    SlidingExpiration = TimeSpan.FromHours(2),
                    RemovedCallback = OnItemRemoved
                }) as CacheItem
                ?? item;

            lock (item)
            {
                var length = blob.Properties.Length - item.Stream.Length;
                if (length > 0)
                {
                    try
                    {
                        blob.DownloadRangeToStream(item.Stream, item.Stream.Length, length);
                    }
                    catch
                    {
                        // Nothing to do since caller will try to read periodically
                    }
                }

                return new MemoryStream(item.Stream.GetBuffer(), 0, (int)item.Stream.Length, false);
            }
        }

        static void OnItemRemoved(CacheEntryRemovedArguments arg)
        {
            var item = arg.CacheItem.Value as CacheItem;
            item?.Stream?.Dispose();
        }
    }
}