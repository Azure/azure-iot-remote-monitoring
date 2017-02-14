using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Azure.Devices.Applications.RemoteMonitoring.Common.Helpers;
using Microsoft.Azure.Devices.Applications.RemoteMonitoring.Common.Models;
using Microsoft.Azure.Devices.Applications.RemoteMonitoring.DeviceAdmin.Infrastructure.Models;
using Microsoft.Azure.Devices.Applications.RemoteMonitoring.DeviceAdmin.Infrastructure.Repository;
using Microsoft.Azure.Devices.Shared;
using Moq;
using Ploeh.AutoFixture;
using Xunit;

namespace Microsoft.Azure.Devices.Applications.RemoteMonitoring.UnitTests.Infrastructure
{
    public class DeviceRegistryRepositoryWithIoTHubDMTests
    {
        private readonly Fixture _fixture;
        private readonly DeviceRegistryRepository _deviceRegistryRepository;
        private readonly Mock<IDocumentDBClient<DeviceModel>> _mockDocumentDBClient;
        private readonly Mock<IIoTHubDeviceManager> _mockIoTHubDeviceManager;

        public DeviceRegistryRepositoryWithIoTHubDMTests()
        {
            this._fixture = new Fixture();
            this._mockDocumentDBClient = new Mock<IDocumentDBClient<DeviceModel>>();
            this._mockIoTHubDeviceManager = new Mock<IIoTHubDeviceManager>();
            this._deviceRegistryRepository = new DeviceRegistryRepositoryWithIoTHubDM(this._mockDocumentDBClient.Object, this._mockIoTHubDeviceManager.Object);
        }

        [Fact]
        public async Task GetDeviceAsyncTest()
        {
            var devices = this._fixture.Create<List<DeviceModel>>();
            var deviceId = devices.First().DeviceProperties.DeviceID;

            this._mockDocumentDBClient
                .Setup(x => x.QueryAsync())
                .ReturnsAsync(devices.AsQueryable());

            var twin = this._fixture.Create<Twin>();

            this._mockIoTHubDeviceManager
                .Setup(x => x.GetTwinAsync(deviceId))
                .ReturnsAsync(twin);

            var device = await this._deviceRegistryRepository.GetDeviceAsync(deviceId);

            Assert.NotNull(device);
            Assert.Same(devices[0], device);

            Assert.Same(device.Twin, twin);
        }

        [Fact]
        public async Task GetDeviceAsyncReturnNullTest()
        {
            var devices = this._fixture.Create<List<DeviceModel>>();

            this._mockDocumentDBClient
                .Setup(x => x.QueryAsync())
                .ReturnsAsync(devices.AsQueryable());

            var device = await this._deviceRegistryRepository.GetDeviceAsync("foobarbaz");

            Assert.Null(device);
        }

        [Fact]
        public async Task AddDeviceTest()
        {
            var device = this._fixture.Create<DeviceModel>();
            var deviceId = device.DeviceProperties.DeviceID;

            await this._deviceRegistryRepository.AddDeviceAsync(device);

            this._mockDocumentDBClient.Verify(x => x.SaveAsync(device));
            this._mockIoTHubDeviceManager.Verify(x => x.UpdateTwinAsync(
                deviceId,
                It.Is<Twin>(twin => IsHubEnabledStateUpdatingTwin(twin, deviceId, "Running"))));
        }

        [Fact]
        public async Task UpdateDeviceTest()
        {
            var existingDevice = this._fixture.Create<DeviceModel>();
            var updatingDevice = this._fixture.Create<DeviceModel>();
            var deviceId = updatingDevice.DeviceProperties.DeviceID;

            existingDevice.DeviceProperties.DeviceID = deviceId;
            existingDevice.Twin.DeviceId = deviceId;
            updatingDevice.Twin.DeviceId = deviceId;

            existingDevice.Twin.Tags["city"] = "Beijing";
            updatingDevice.Twin.Tags["city"] = "Shanghai";

            this._mockDocumentDBClient
                .Setup(x => x.QueryAsync())
                .ReturnsAsync(new[] { existingDevice }.AsQueryable());

            this._mockIoTHubDeviceManager
                .Setup(x => x.GetTwinAsync(deviceId))
                .ReturnsAsync(existingDevice.Twin);

            await this._deviceRegistryRepository.UpdateDeviceAsync(updatingDevice);

            this._mockIoTHubDeviceManager.Verify(x => x.UpdateTwinAsync(deviceId, updatingDevice.Twin));
        }

        [Fact]
        public async Task UpdateDeviceSkipTwinUpdateTest()
        {
            var existingDevice = this._fixture.Create<DeviceModel>();
            var updatingDevice = this._fixture.Create<DeviceModel>();
            var deviceId = updatingDevice.DeviceProperties.DeviceID;

            existingDevice.DeviceProperties.DeviceID = deviceId;
            existingDevice.Twin.DeviceId = deviceId;
            updatingDevice.Twin.DeviceId = deviceId;

            existingDevice.Twin.Tags["city"] = "Beijing";
            updatingDevice.Twin.Tags["city"] = "Beijing";

            this._mockDocumentDBClient
                .Setup(x => x.QueryAsync())
                .ReturnsAsync(new[] { existingDevice }.AsQueryable());

            this._mockIoTHubDeviceManager
                .Setup(x => x.GetTwinAsync(deviceId))
                .ReturnsAsync(existingDevice.Twin);

            await this._deviceRegistryRepository.UpdateDeviceAsync(updatingDevice);

            this._mockIoTHubDeviceManager.Verify(x => x.UpdateTwinAsync(It.IsAny<string>(), It.IsAny<Twin>()), Times.Never);
        }

        [Fact]
        public async Task UpdateDeviceNullTwinTest()
        {
            var existingDevice = this._fixture.Create<DeviceModel>();
            var updatingDevice = this._fixture.Create<DeviceModel>();
            var deviceId = updatingDevice.DeviceProperties.DeviceID;

            existingDevice.DeviceProperties.DeviceID = deviceId;
            existingDevice.Twin.DeviceId = deviceId;
            updatingDevice.Twin = null;

            this._mockDocumentDBClient
                .Setup(x => x.QueryAsync())
                .ReturnsAsync(new[] { existingDevice }.AsQueryable());

            await this._deviceRegistryRepository.UpdateDeviceAsync(updatingDevice);

            this._mockIoTHubDeviceManager.Verify(x => x.UpdateTwinAsync(It.IsAny<string>(), It.IsAny<Twin>()), Times.Never);
        }

        [Fact]
        public async Task UpdateDeviceEnabledStatusTest()
        {
            var device = this._fixture.Create<DeviceModel>();
            var deviceId = device.DeviceProperties.DeviceID;
            device.DeviceProperties.HubEnabledState = false;

            this._mockDocumentDBClient
                .Setup(x => x.QueryAsync())
                .ReturnsAsync(new[] { device }.AsQueryable());

            await this._deviceRegistryRepository.UpdateDeviceEnabledStatusAsync(deviceId, true);

            Assert.True(device.DeviceProperties.HubEnabledState);
            this._mockDocumentDBClient.Verify(x => x.SaveAsync(device));
            this._mockIoTHubDeviceManager.Verify(x => x.UpdateTwinAsync(
                deviceId,
                It.Is<Twin>(twin => IsHubEnabledStateUpdatingTwin(twin, deviceId, "Runing"))));
        }

        [Fact]
        public async Task GetDeviceListTest()
        {
            var rand = new Random();

            var filtedDevices = this._fixture.CreateMany<DeviceModel>(16).ToList();
            filtedDevices.ForEach(device =>
            {
                device.Twin.DeviceId = device.DeviceProperties.DeviceID;
                device.Twin.Tags["seq"] = rand.Next();
            });

            var filter = new DeviceListFilter
            {
                Skip = 0,
                Take = int.MaxValue,
                SortOrder = QuerySortOrder.Ascending
            };

            this._mockDocumentDBClient
                .Setup(x => x.QueryAsync())
                .ReturnsAsync(filtedDevices.AsQueryable());

            this._mockIoTHubDeviceManager
                .Setup(x => x.GetDeviceCountAsync())
                .ReturnsAsync(1024);

            this._mockIoTHubDeviceManager
                .Setup(x => x.QueryDevicesAsync(It.IsAny<DeviceListFilter>(), It.IsAny<int>())).
                ReturnsAsync(filtedDevices.Select(d => d.Twin));

            var result = await this._deviceRegistryRepository.GetDeviceList(filter);

            Assert.Equal(result.TotalDeviceCount, 1024);
            Assert.Equal(result.TotalFilteredCount, filtedDevices.Count());
            Assert.True(result.Results.SequenceEqual(filtedDevices));

            filter.SortColumn = "twin.tags.seq";
            result = await this._deviceRegistryRepository.GetDeviceList(filter);
            Assert.True(result.Results.SequenceEqual(filtedDevices.OrderBy(d => (int)d.Twin.Tags["seq"])));

            filter.SortOrder = QuerySortOrder.Descending;
            result = await this._deviceRegistryRepository.GetDeviceList(filter);
            Assert.True(result.Results.SequenceEqual(filtedDevices.OrderByDescending(d => (int)d.Twin.Tags["seq"])));
        }

        private bool IsHubEnabledStateUpdatingTwin(Twin twin, string deviceId, string status)
        {
            return twin.DeviceId == deviceId &&
                twin.ETag == "*" &&
                twin.Tags["HubEnabledState"].ToString() == "Running" &&
                twin.Properties.Desired.Count == 0 &&
                twin.Properties.Reported.Count == 0;
        }
    }
}
