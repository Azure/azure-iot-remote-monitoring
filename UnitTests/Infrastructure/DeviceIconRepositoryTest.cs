using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using Microsoft.Azure.Devices.Applications.RemoteMonitoring.Common.Configurations;
using Microsoft.Azure.Devices.Applications.RemoteMonitoring.Common.Helpers;
using Microsoft.Azure.Devices.Applications.RemoteMonitoring.DeviceAdmin.Infrastructure.Repository;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Blob;
using Moq;
using Ploeh.AutoFixture;
using Ploeh.AutoFixture.AutoMoq;
using Xunit;
using Ploeh.AutoFixture.Kernel;

namespace Microsoft.Azure.Devices.Applications.RemoteMonitoring.UnitTests.Infrastructure
{
    public class DeviceIconRepositoryTest
    {
        private readonly Mock<IBlobStorageClient> _blobStorageClientMock;
        private readonly DeviceIconRepository deviceIconRepository;
        private readonly IFixture fixture;
        private readonly Mock<IConfigurationProvider> _configurationProviderMock;

        public DeviceIconRepositoryTest()
        {
            fixture = new Fixture();
            fixture.Customize(new AutoConfiguredMoqCustomization());
            _configurationProviderMock = new Mock<IConfigurationProvider>();
            _blobStorageClientMock = new Mock<IBlobStorageClient>();
            var blobStorageFactory = new BlobStorageClientFactory(_blobStorageClientMock.Object);
            _configurationProviderMock.Setup(x => x.GetConfigurationSettingValue(It.IsNotNull<string>()))
                .ReturnsUsingFixture(fixture);
            deviceIconRepository = new DeviceIconRepository(_configurationProviderMock.Object,
                blobStorageFactory);
        }

        [Fact]
        public async void AddIconTest()
        {
            var uri = new Uri("https://account1.blob.core.windows.net/container1/device1_jpg");
            var mockBlob = new CloudBlockBlob(uri);
            string blobData = "This is image blob data";
            var stream = new MemoryStream(Encoding.UTF8.GetBytes(blobData));
            _blobStorageClientMock.Setup(x => x.UploadFromStreamAsync(It.IsAny<string>(),
                It.IsAny<string>(),
                It.IsNotNull<Stream>(),
                It.IsNotNull<AccessCondition>(),
                It.IsAny<BlobRequestOptions>(),
                It.IsAny<OperationContext>())).ReturnsAsync(mockBlob);
            _blobStorageClientMock.Setup(x => x.MoveBlob(It.IsAny<string>(), It.IsNotNull<string>())).ReturnsAsync(mockBlob);
            var icon = await deviceIconRepository.AddIcon("device1.jpg", stream);
            Assert.Equal(icon.Name, "device1_jpg");
        }

        [Fact]
        public async void GetIconsTest()
        {
            var uri1 = new Uri("https://account1.blob.core.windows.net/deviceicons/applied/device1_jpg");
            var uri2 = new Uri("https://account1.blob.core.windows.net/deviceicons/applied/device2_jpg");
            var mockBlobs = new List<ICloudBlob>() {
                new CloudBlockBlob(uri1), new CloudBlockBlob(uri2),
            };
            string blobData = "This is image blob data";
            var stream = new MemoryStream(Encoding.UTF8.GetBytes(blobData));
            _blobStorageClientMock.Setup(x => x.ListBlobs(It.IsAny<string>(),
                It.IsAny<bool>())).ReturnsAsync(mockBlobs);
            var icons = await deviceIconRepository.GetIcons(0, 10);
            Assert.Equal(new string[] { "device1_jpg", "device2_jpg" }, icons.Results.Select(i => i.Name).ToArray());
            Assert.Equal(2, icons.TotalCount);
        }

        [Fact]
        public async void GetIconTest()
        {
            var uri = new Uri("https://account1.blob.core.windows.net/container1/device1_jpg");
            var mockBlob = new CloudBlockBlob(uri);
            string blobData = "This is image blob data";
            var stream = new MemoryStream(Encoding.UTF8.GetBytes(blobData));
            _blobStorageClientMock.Setup(x => x.GetBlob(It.IsAny<string>())).ReturnsAsync(mockBlob);
            var icon = await deviceIconRepository.GetIcon("device1_jpg");
            Assert.Equal("device1_jpg", icon.Name);
            Assert.Equal(uri.ToString(), icon.BlobUrl);
        }

        [Fact]
        public async void SaveIconsTest()
        {
            string targetName = "device1_jpg";
            var uri = new Uri("https://account1.blob.core.windows.net/deviceicons/applied/device1_jpg");
            var mockBlob = new CloudBlockBlob(uri);
            _blobStorageClientMock.Setup(x => x.GetBlob(It.IsNotNull<string>())).ReturnsAsync(mockBlob);
            var icons = await deviceIconRepository.SaveIcon(targetName);
            Assert.Equal(targetName, icons.Name);
        }

        [Fact]
        public async void DeleteIconsTest()
        {
            string targetName = "device1_jpg";
            _blobStorageClientMock.Setup(x => x.DeleteBlob(It.IsAny<string>())).ReturnsAsync(true);
            var result = await deviceIconRepository.DeleteIcon(targetName);
            Assert.True(result);
        }

        [Fact]
        public async void GetIconStorageUrlPrefixTest()
        {
            string containerUri = "https://account1.blob.core.windows.net/deviceicons";
            _blobStorageClientMock.Setup(x => x.GetContainerUri()).ReturnsAsync(containerUri);
            string url = await deviceIconRepository.GetIconStorageUriPrefix();
            Assert.True(url.StartsWith(containerUri));
        }
    }
}
