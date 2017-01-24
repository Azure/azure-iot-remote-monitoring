using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using Microsoft.Azure.Devices.Applications.RemoteMonitoring.Common.Configurations;
using Microsoft.Azure.Devices.Applications.RemoteMonitoring.Common.Helpers;
using Microsoft.Azure.Devices.Applications.RemoteMonitoring.Common.Models;
using Microsoft.Azure.Devices.Applications.RemoteMonitoring.DeviceAdmin.Infrastructure.Repository;
using Moq;
using Ploeh.AutoFixture;
using Ploeh.AutoFixture.AutoMoq;
using Xunit;

namespace Microsoft.Azure.Devices.Applications.RemoteMonitoring.UnitTests.Infrastructure
{
    public class DeviceTelemetryRepositoryTest
    {
        private readonly Mock<IBlobStorageClient> _blobStorageClientMock;
        private readonly DeviceTelemetryRepository deviceTelemetryRepository;
        private readonly IFixture fixture;
        private readonly Mock<IConfigurationProvider> _configurationProviderMock;

        public DeviceTelemetryRepositoryTest()
        {
            fixture = new Fixture();
            fixture.Customize(new AutoConfiguredMoqCustomization());
            _configurationProviderMock = new Mock<IConfigurationProvider>();
            _blobStorageClientMock = new Mock<IBlobStorageClient>();
            var blobStorageFactory = new BlobStorageClientFactory(_blobStorageClientMock.Object);
            _configurationProviderMock.Setup(x => x.GetConfigurationSettingValue(It.IsNotNull<string>()))
                .ReturnsUsingFixture(fixture);
            deviceTelemetryRepository = new DeviceTelemetryRepository(_configurationProviderMock.Object,
                blobStorageFactory);
        }

        [Fact]
        public async void LoadLatestDeviceTelemetryAsyncTest()
        {
            var year = 2016;
            var month = 7;
            var date = 5;
            var minTime = new DateTime(year, month, date);

            var blobReader = new Mock<IBlobStorageReader>();
            var blobData = "deviceid,temperature,humidity,externaltemperature,eventprocessedutctime,partitionid,eventenqueuedutctime,IoTHub" +
                Environment.NewLine +
                "test1,34.200411299709423,32.2233033525866,,2016-08-04T23:07:14.2549606Z,3," + minTime.ToString("o") + ",Record";
            var stream = new MemoryStream(Encoding.UTF8.GetBytes(blobData));
            var blobContents = new BlobContents {Data = stream, LastModifiedTime = DateTime.UtcNow};
            var blobContentIterable = new List<BlobContents>();
            blobContentIterable.Add(blobContents);

            blobReader.Setup(x => x.GetEnumerator()).Returns(blobContentIterable.GetEnumerator());
            _blobStorageClientMock.Setup(x => x.GetReader(It.IsNotNull<string>(), It.IsAny<DateTime?>()))
                .ReturnsAsync(blobReader.Object);
            var telemetryList = await deviceTelemetryRepository.LoadLatestDeviceTelemetryAsync("test1", null, minTime);
            Assert.NotNull(telemetryList);
            Assert.NotEmpty(telemetryList);
            Assert.Equal(telemetryList.First().DeviceId, "test1");
            Assert.Equal(telemetryList.First().Timestamp, minTime);
        }

        [Fact]
        public async void LoadLatestDeviceTelemetrySummaryAsyncTest()
        {
            var year = 2016;
            var month = 7;
            var date = 5;
            var minTime = new DateTime(year, month, date);

            var blobReader = new Mock<IBlobStorageReader>();
            var blobData = "deviceid,averagehumidity,minimumhumidity,maxhumidity,timeframeminutes" + Environment.NewLine +
                           "test2,37.806204872115607,37.806204872115607,37.806204872115607,5";
            var stream = new MemoryStream(Encoding.UTF8.GetBytes(blobData));
            var blobContents = new BlobContents {Data = stream, LastModifiedTime = DateTime.UtcNow};
            var blobContentIterable = new List<BlobContents>();
            blobContentIterable.Add(blobContents);

            blobReader.Setup(x => x.GetEnumerator()).Returns(blobContentIterable.GetEnumerator());
            _blobStorageClientMock.Setup(x => x.GetReader(It.IsNotNull<string>(), It.IsAny<DateTime?>()))
                .ReturnsAsync(blobReader.Object);
            var telemetrySummaryList =
                await deviceTelemetryRepository.LoadLatestDeviceTelemetrySummaryAsync("test2", minTime);
            Assert.NotNull(telemetrySummaryList);
            Assert.Equal(telemetrySummaryList.DeviceId, "test2");
            Assert.Equal(telemetrySummaryList.AverageHumidity, 37.806204872115607);
            Assert.Equal(telemetrySummaryList.MinimumHumidity, 37.806204872115607);
            Assert.Equal(telemetrySummaryList.MaximumHumidity, 37.806204872115607);
            Assert.Equal(telemetrySummaryList.TimeFrameMinutes, 5);
        }
    }
}