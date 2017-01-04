using System;
using System.IO;
using Microsoft.WindowsAzure.Storage.Blob;
using Newtonsoft.Json;

namespace Microsoft.Azure.Devices.Applications.RemoteMonitoring.DeviceAdmin.Infrastructure.Models
{
    public class DeviceIcon
    {
        public DeviceIcon(string name, ICloudBlob blob)
        {
            Name = name;
            BlobUrl = blob.StorageUri.PrimaryUri.AbsoluteUri;
            Size = blob.Properties.Length;
            LastModified = blob.Properties.LastModified;
            ETag = blob.Properties.ETag;
        }

        public string Name { get; set; }
        public string BlobUrl { get; internal set; }
        public string ETag { get; internal set; }
        public DateTimeOffset? LastModified { get; internal set; }
        public long Size { get; set; }

        [JsonIgnore]
        public MemoryStream ImageStream { get; internal set; }
    }
}