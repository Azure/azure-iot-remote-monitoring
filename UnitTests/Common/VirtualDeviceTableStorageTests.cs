using System.Collections.Generic;
using System.Linq;
using Microsoft.Azure.Devices.Applications.RemoteMonitoring.Common.Configurations;
using Microsoft.Azure.Devices.Applications.RemoteMonitoring.Common.Helpers;
using Microsoft.Azure.Devices.Applications.RemoteMonitoring.Common.Models;
using Microsoft.Azure.Devices.Applications.RemoteMonitoring.Common.Repository;
using Microsoft.WindowsAzure.Storage.Table;
using Moq;
using Ploeh.AutoFixture;
using Ploeh.AutoFixture.AutoMoq;
using Xunit;

namespace Microsoft.Azure.Devices.Applications.RemoteMonitoring.UnitTests.Common
{
    public class VirtualDeviceTableStorageTests
    {
        private Mock<IAzureTableStorageClient> _tableStorageClientMock;
        private IVirtualDeviceStorage _virtualDeviceStorage;
        private IFixture _fixture;

        public VirtualDeviceTableStorageTests()
        {
            _fixture = new Fixture();
            var configProviderMock = new Mock<IConfigurationProvider>();
            configProviderMock.Setup(x => x.GetConfigurationSettingValue(It.IsNotNull<string>()))
                .ReturnsUsingFixture(_fixture);
            _tableStorageClientMock = new Mock<IAzureTableStorageClient>();
            var tableStorageClientFactory = new AzureTableStorageClientFactory(_tableStorageClientMock.Object);
            _virtualDeviceStorage = new VirtualDeviceTableStorage(configProviderMock.Object, tableStorageClientFactory);
        }

        [Fact]
        public async void GetDeviceListAsync()
        {
            var deviceEntities = _fixture.Create<List<DeviceListEntity>>();
            _tableStorageClientMock.Setup(x => x.ExecuteQueryAsync(It.IsNotNull<TableQuery<DeviceListEntity>>()))
                .ReturnsAsync(deviceEntities);
            var ret = await _virtualDeviceStorage.GetDeviceListAsync();
            Assert.NotNull(ret);
            Assert.Equal(deviceEntities.Count, ret.Count);
            Assert.Equal(deviceEntities[0].DeviceId, ret[0].DeviceId);
            Assert.Equal(deviceEntities[0].HostName, ret[0].HostName);
            Assert.Equal(deviceEntities[0].Key, ret[0].Key);
        }

        [Fact]
        public async void GetDeviceAsyncTest()
        {
            _fixture.Customize<DeviceListEntity>(ob => ob.With(x => x.DeviceId, "DeviceXXXId"));
            var entities = _fixture.CreateMany<DeviceListEntity>().ToList();
            _tableStorageClientMock.Setup(x => x.ExecuteQueryAsync(It.IsNotNull<TableQuery<DeviceListEntity>>()))
                .ReturnsAsync(entities);
            var ret = await _virtualDeviceStorage.GetDeviceAsync("DeviceXXXId");
            Assert.NotNull(ret);
            Assert.Equal(entities[0].DeviceId, ret.DeviceId);
            Assert.Equal(entities[0].HostName, ret.HostName);
            Assert.Equal(entities[0].Key, ret.Key);
        }

        [Fact]
        public async void RemoveDeviceAsyncTest()
        {
            var entities = _fixture.Create<List<DeviceListEntity>>();
            _tableStorageClientMock.Setup(x => x.ExecuteQueryAsync(It.IsNotNull<TableQuery<DeviceListEntity>>()))
                .ReturnsAsync(entities);
            _tableStorageClientMock.Setup(x => x.ExecuteAsync(It.IsNotNull<TableOperation>()))
                .ReturnsAsync(new TableResult() {Result = entities[0]});
            Assert.True(await _virtualDeviceStorage.RemoveDeviceAsync(entities[0].DeviceId));
        }

        [Fact]
        public async void AddOrUpdateDeviceAsyncTest()
        {
            var deviceConfig = _fixture.Create<InitialDeviceConfig>();
            _tableStorageClientMock.Setup(x => x.ExecuteAsync(It.IsNotNull<TableOperation>()))
                .ReturnsAsync(new TableResult());
            await _virtualDeviceStorage.AddOrUpdateDeviceAsync(deviceConfig);
        }
    }
}