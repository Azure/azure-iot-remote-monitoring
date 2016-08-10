using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Azure.Devices.Applications.RemoteMonitoring.Common.Exceptions;
using Microsoft.Azure.Devices.Applications.RemoteMonitoring.Common.Helpers;
using Microsoft.Azure.Devices.Applications.RemoteMonitoring.Common.Models;
using Microsoft.Azure.Devices.Applications.RemoteMonitoring.DeviceAdmin.Infrastructure.Exceptions;
using Microsoft.Azure.Devices.Applications.RemoteMonitoring.DeviceAdmin.Infrastructure.Repository;
using Moq;
using Ploeh.AutoFixture;
using Xunit;

namespace Microsoft.Azure.Devices.Applications.RemoteMonitoring.UnitTests.Infrastructure
{
    public class DeviceRegistryRepositoryTests
    {
        private readonly Fixture _fixture;
        private readonly DeviceRegistryRepository _deviceRegistryRepository;
        private readonly Mock<IDocumentDBClient<DeviceModel>> _mockDocumentDBClient;

        public DeviceRegistryRepositoryTests()
        {
            this._fixture = new Fixture();
            this._mockDocumentDBClient = new Mock<IDocumentDBClient<DeviceModel>>();
            this._deviceRegistryRepository = new DeviceRegistryRepository(this._mockDocumentDBClient.Object);
        }

       
        [Theory]
        [InlineData(null)]
        [InlineData("")]
        public async Task GetDeviceAsync_should_throw_argument_exception_when_device_id_is_invalid(string deviceId)
        {
            await Assert.ThrowsAsync<ArgumentException>(() => this._deviceRegistryRepository.GetDeviceAsync(deviceId));
        }

        [Fact]
        public async Task GetDeviceAsync_should_get_device_from_client()
        {
            var devices = this._fixture.Create<List<DeviceModel>>();
            var deviceId = devices.First().DeviceProperties.DeviceID;

            this._mockDocumentDBClient
                .Setup(x => x.QueryAsync())
                .ReturnsAsync(devices.AsQueryable());

            var device = await this._deviceRegistryRepository.GetDeviceAsync(deviceId);

            Assert.NotNull(device);
            Assert.Same(devices[0], device);
        }

        [Fact]
        public async Task GetDeviceAsync_should_return_null_when_device_id_is_not_found()
        {
            var devices = this._fixture.Create<List<DeviceModel>>();

            this._mockDocumentDBClient
                .Setup(x => x.QueryAsync())
                .ReturnsAsync(devices.AsQueryable());

            var device = await this._deviceRegistryRepository.GetDeviceAsync("foobarbaz");

            Assert.Null(device);
        }

        [Fact]
        public async Task AddDeviceAsync_should_throw_argument_null_when_device_is_null()
        {
            await Assert.ThrowsAsync<ArgumentNullException>(() => this._deviceRegistryRepository.AddDeviceAsync(null));
        }

        [Fact]
        public async Task AddDeviceAsync_should_throw_device_exists_exception_when_device_with_same_id_exists()
        {
            var devices = this._fixture.Create<List<DeviceModel>>();
            this._mockDocumentDBClient
                .Setup(x => x.QueryAsync())
                .ReturnsAsync(devices.AsQueryable());

            await Assert.ThrowsAsync<DeviceAlreadyRegisteredException>(() => this._deviceRegistryRepository.AddDeviceAsync(devices.First()));
        }

        [Fact]
        public async Task AddDeviceAsync_should_add_device_to_db()
        {
            var device = this._fixture.Create<DeviceModel>();
            await this._deviceRegistryRepository.AddDeviceAsync(device);
            this._mockDocumentDBClient.Verify(x => x.SaveAsync(device));
        }

        [Theory]
        [InlineData(null)]
        [InlineData("")]
        public async Task RemoveDeviceAsync_should_throw_argument_null_when_device_id_is_not_valid(string deviceId)
        {
            await Assert.ThrowsAsync<ArgumentNullException>(() => this._deviceRegistryRepository.RemoveDeviceAsync(deviceId));
        }

        [Fact]
        public async Task RemoveDeviceAsync_should_throw_device_not_found_when_device_does_not_exist()
        {
            this._mockDocumentDBClient
                .Setup(x => x.QueryAsync())
                .ReturnsAsync(Enumerable.Empty<DeviceModel>().AsQueryable());

            await Assert.ThrowsAsync<DeviceNotRegisteredException>(() => this._deviceRegistryRepository.RemoveDeviceAsync("foobar"));

            this._mockDocumentDBClient.VerifyAll();
        }

        [Fact]
        public async Task RemoveDeviceAsync_should_remove_device_from_store()
        {
            var device = this._fixture.Create<DeviceModel>();

            this._mockDocumentDBClient
                .Setup(x => x.QueryAsync())
                .ReturnsAsync(new [] {device}.AsQueryable());


            await this._deviceRegistryRepository.RemoveDeviceAsync(device.DeviceProperties.DeviceID);

            this._mockDocumentDBClient.Verify(x => x.DeleteAsync(device.id));
        }

        [Fact]
        public async Task UpdateDeviceAsync_should_throw_argument_null_when_device_is_null()
        {
            await Assert.ThrowsAsync<ArgumentNullException>(() => this._deviceRegistryRepository.UpdateDeviceAsync(null));
        }

        [Fact]
        public async Task UpdateDeviceAsync_should_throw_device_property_not_found_when_properties_is_null()
        {
            var device = this._fixture.Create<DeviceModel>();
            device.DeviceProperties = null;
            await Assert.ThrowsAsync<DeviceRequiredPropertyNotFoundException>(() => this._deviceRegistryRepository.UpdateDeviceAsync(device));
        }

        [Theory]
        [InlineData(null)]
        [InlineData("")]
        public async Task UpdateDeviceAsync_should_throw_when_device_id_is_not_valid(string deviceId)
        {
            var device = this._fixture.Create<DeviceModel>();
            device.DeviceProperties.DeviceID = deviceId;
            await Assert.ThrowsAsync<DeviceRequiredPropertyNotFoundException>(() => this._deviceRegistryRepository.UpdateDeviceAsync(device));
        }

        [Fact]
        public async Task UpdateDeviceAsync_should_throw_device_not_found_when_device_does_not_exist()
        {
            var device = this._fixture.Create<DeviceModel>();

            this._mockDocumentDBClient
               .Setup(x => x.QueryAsync())
               .ReturnsAsync(Enumerable.Empty<DeviceModel>().AsQueryable());

            await Assert.ThrowsAsync<DeviceNotRegisteredException>(() => this._deviceRegistryRepository.UpdateDeviceAsync(device));
        }

        [Fact]
        public async Task UpdateDeviceAsync_should_set_db_id_properties_before_updating_to_store()
        {
            var existingDevice = this._fixture.Create<DeviceModel>();
            var updatingDevice = this._fixture.Create<DeviceModel>();

            existingDevice.DeviceProperties.DeviceID = updatingDevice.DeviceProperties.DeviceID;
            updatingDevice.id = null;
            updatingDevice._rid = null;

            this._mockDocumentDBClient
                .Setup(x => x.QueryAsync())
                .ReturnsAsync(new [] {existingDevice}.AsQueryable());

            await this._deviceRegistryRepository.UpdateDeviceAsync(updatingDevice);

            Assert.Equal(existingDevice.id, updatingDevice.id);
            Assert.Equal(existingDevice._rid, updatingDevice._rid);
        }

        [Fact]
        public async Task UpdateDeviceAsync_should_throw_invalid_operation_when_id_cannot_be_set_from_existing()
        {
            var existingDevice = this._fixture.Create<DeviceModel>();
            var updatingDevice = this._fixture.Create<DeviceModel>();

            existingDevice.DeviceProperties.DeviceID = updatingDevice.DeviceProperties.DeviceID;
            existingDevice.id = null;
            updatingDevice.id = null;

            this._mockDocumentDBClient
                .Setup(x => x.QueryAsync())
                .ReturnsAsync(new [] {existingDevice}.AsQueryable());

            await Assert.ThrowsAsync<InvalidOperationException>(() => this._deviceRegistryRepository.UpdateDeviceAsync(updatingDevice));
        }

        [Fact]
        public async Task UpdateDeviceAsync_should_throw_invalid_operation_when_rid_cannot_be_set_from_existing()
        {
            var existingDevice = this._fixture.Create<DeviceModel>();
            var updatingDevice = this._fixture.Create<DeviceModel>();

            existingDevice.DeviceProperties.DeviceID = updatingDevice.DeviceProperties.DeviceID;
            existingDevice._rid= null;
            updatingDevice._rid = null;

            this._mockDocumentDBClient
                .Setup(x => x.QueryAsync())
                .ReturnsAsync(new [] { existingDevice }.AsQueryable());

            await Assert.ThrowsAsync<InvalidOperationException>(() => this._deviceRegistryRepository.UpdateDeviceAsync(updatingDevice));
        }

        [Theory]
        [InlineData(null)]
        [InlineData("")]
        public async Task UpdateDeviceEnabledStateStatus_should_throw_argument_null_when_device_id_is_not_valid(string deviceId)
        {
            await Assert.ThrowsAsync<ArgumentNullException>(() => this._deviceRegistryRepository.UpdateDeviceEnabledStatusAsync(deviceId, true));
        }

        [Fact]
        public async Task UpdateDeviceEnabledStateStatus_should_throw_when_device_is_not_found()
        {
            this._mockDocumentDBClient
                .Setup(x => x.QueryAsync())
                .ReturnsAsync(Enumerable.Empty<DeviceModel>().AsQueryable());

            await Assert.ThrowsAsync<DeviceNotRegisteredException>(() => this._deviceRegistryRepository.UpdateDeviceEnabledStatusAsync("foobar", true));
        }

        [Fact]
        public async Task UpdateDeviceEnabledStateStatus_should_set_hub_enabled_and_update_in_store()
        {
            var device = this._fixture.Create<DeviceModel>();
            device.DeviceProperties.HubEnabledState = false;

            this._mockDocumentDBClient
                .Setup(x => x.QueryAsync())
                .ReturnsAsync(new[] {device}.AsQueryable());

            await this._deviceRegistryRepository.UpdateDeviceEnabledStatusAsync(device.DeviceProperties.DeviceID, true);

            Assert.True(device.DeviceProperties.HubEnabledState);
            this._mockDocumentDBClient.Verify(x => x.SaveAsync(device));
        }
    }
}
