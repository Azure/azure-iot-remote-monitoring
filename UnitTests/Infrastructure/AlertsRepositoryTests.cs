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
    public class AlertsRepositoryTests
    {
        private readonly Mock<IBlobStorageClient> _blobStorageClientMock;
        private readonly AlertsRepository alertsRepository;
        private readonly IFixture fixture;
        private readonly Mock<IConfigurationProvider> _configurationProviderMock;

        public AlertsRepositoryTests()
        {
            fixture = new Fixture();
            fixture.Customize(new AutoConfiguredMoqCustomization());
            _configurationProviderMock = new Mock<IConfigurationProvider>();
            _blobStorageClientMock = new Mock<IBlobStorageClient>();
            var blobStorageFactory = new BlobStorageClientFactory(_blobStorageClientMock.Object);
            _configurationProviderMock.Setup(x => x.GetConfigurationSettingValue(It.IsNotNull<string>()))
                .ReturnsUsingFixture(fixture);
            alertsRepository = new AlertsRepository(_configurationProviderMock.Object, blobStorageFactory);
        }

        [Fact]
        public async void LoadLatestAlertHistoryAsyncTest()
        {
            var year = 2016;
            var month = 7;
            var date = 5;
            var value = "10.0";
            var minTime = new DateTime(year, month, date);

            await Assert.ThrowsAsync<ArgumentOutOfRangeException>(async () => await alertsRepository.LoadLatestAlertHistoryAsync(minTime, 0));

            var blobReader = new Mock<IBlobStorageReader>();
             var blobData = $"deviceid,reading,ruleoutput,time,{Environment.NewLine}Device123,{value},RuleOutput123,{minTime.ToString("o")}";
            var stream = new MemoryStream(Encoding.UTF8.GetBytes(blobData));
            var blobContents = new BlobContents {Data = stream, LastModifiedTime = DateTime.UtcNow};
            var blobContentIterable = new List<BlobContents>();
            blobContentIterable.Add(blobContents);

            blobReader.Setup(x => x.GetEnumerator()).Returns(blobContentIterable.GetEnumerator());

            _blobStorageClientMock
                .Setup(x => x.GetReader(It.IsNotNull<string>(), It.IsAny<DateTime?>()))
                .ReturnsAsync(blobReader.Object);

            var alertsList = await alertsRepository.LoadLatestAlertHistoryAsync(minTime, 5);
            Assert.NotNull(alertsList);
            Assert.NotEmpty(alertsList);
            Assert.Equal(alertsList.First().Value, value);
            Assert.Equal(alertsList.First().Timestamp, minTime);
        }
    }
}