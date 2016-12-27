using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Azure.Devices.Applications.RemoteMonitoring.DeviceAdmin.Infrastructure.Models;
using Microsoft.Azure.Devices.Applications.RemoteMonitoring.Common.Configurations;
using Microsoft.Azure.Devices.Applications.RemoteMonitoring.Common.Helpers;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Blob;

namespace Microsoft.Azure.Devices.Applications.RemoteMonitoring.DeviceAdmin.Infrastructure.Repository
{
    public class DeviceIconRepository : IDeviceIconRepository
    {
        private readonly string _storageAccountConnectionString;
        private readonly string _deviceIconsBlobStoreContainerName;
        private readonly string _uploadedFolder = "uploaded";
        private readonly string _appliedFolder = "applied";
        private readonly string _policyName;
        private readonly IBlobStorageClient _blobStorageClient;

        public DeviceIconRepository(IConfigurationProvider configurationProvider, IBlobStorageClientFactory blobStorageClientFactory)
        {
            _storageAccountConnectionString = configurationProvider.GetConfigurationSettingValue("device.StorageConnectionString");
            _deviceIconsBlobStoreContainerName = configurationProvider.GetConfigurationSettingValueOrDefault("DeviceIconStoreContainerName", "deviceicons");
            _blobStorageClient = blobStorageClientFactory.CreateClient(_storageAccountConnectionString, _deviceIconsBlobStoreContainerName);
            _policyName = _deviceIconsBlobStoreContainerName + "-write";
        }

        public async Task<DeviceIcon> AddIcon(string deviceId, string fileName, Stream fileStream)
        {
            // replace '.' with '_' so that the name can be used in MVC Url route.
            var name = Path.GetFileName(fileName).Replace(".", "_");
            var blob = await _blobStorageClient.UploadFromStreamAsync($"{_uploadedFolder}/{name}",
                fileStream,
                AccessCondition.GenerateEmptyCondition(),
                new BlobRequestOptions() { StoreBlobContentMD5 = true },
                null);

            return new DeviceIcon(name, blob);
        }

        public Task<DeviceIcon> GetIcon(string deviceId, string name, bool applied)
        {
            string folder = applied ? _appliedFolder : _uploadedFolder;
            MemoryStream stream = new MemoryStream();

            var blob = _blobStorageClient.DownloadToStream($"{folder}/{name}", stream).Result;
            var icon = new DeviceIcon(name, blob)
            {
                ImageStream = stream,
            };

            return Task.FromResult(icon);
        }

        public async Task<IEnumerable<DeviceIcon>> GetIcons(string deviceId)
        {
            string folderPrefix = _appliedFolder + "/";
            var blobs = await _blobStorageClient.ListBlobs(folderPrefix, true);
            return blobs.Select(b =>
            {
                string name = b.Name.Substring(folderPrefix.Length);
                return new DeviceIcon(name, b);
            }).OrderByDescending(i => i.LastModified);
        }

        public async Task<DeviceIcon> SaveIcon(string deviceId, string name)
        {
            var appliedBlob = await _blobStorageClient.MoveBlob($"{_uploadedFolder}/{name}", $"{_appliedFolder}/{name}");
            return new DeviceIcon(name, appliedBlob);
        }
    }
}
