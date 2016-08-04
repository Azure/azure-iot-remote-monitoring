using System;
using System.IO;

namespace Microsoft.Azure.Devices.Applications.RemoteMonitoring.Common.Models
{
    public class BlobContents
    {
        public Stream Data { get; set; }
        public DateTime? LastModifiedTime { get; set; }}
}