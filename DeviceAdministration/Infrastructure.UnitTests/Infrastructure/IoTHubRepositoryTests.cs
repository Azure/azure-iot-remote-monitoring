using System;
using System.Threading.Tasks;
using Microsoft.Azure.Devices.Applications.RemoteMonitoring.Common.Models;
using Microsoft.Azure.Devices.Applications.RemoteMonitoring.DeviceAdmin.Infrastructure.Repository;
using Moq;
using Ploeh.AutoFixture;
using Xunit;

namespace Microsoft.Azure.Devices.Applications.RemoteMonitoring.DeviceAdmin.Infrastructure.UnitTests.Infrastructure
{
    public class IoTHubRepositoryTests
    {
        private readonly IIotHubRepository iotHubRepository;
        private readonly Mock<IDeviceManager> deviceManagerMock;
        private readonly Fixture fixture;

        public IoTHubRepositoryTests()
        {
            this.deviceManagerMock = new Mock<IDeviceManager>();
            this.deviceManagerMock.Setup(dm => dm.AddDeviceAsync(It.IsAny<Device>())).Returns(Task.FromResult(new Device()));
            this.deviceManagerMock.Setup(dm => dm.RemoveDeviceAsync(It.IsAny<string>())).Returns(Task.FromResult(true));
            this.iotHubRepository = new IotHubRepository(this.deviceManagerMock.Object);
            this.fixture = new Fixture();
        }

        [Fact]
        public async void AddDeviceAsync()
        {
            var device = this.fixture.Create<DeviceModel>();
            var keys = new SecurityKeys("fbsIV6w7gfVUyoRIQFSVgw ==", "1fLjiNCMZF37LmHnjZDyVQ ==");
            var sameDevice = await this.iotHubRepository.AddDeviceAsync(device, keys);
            Assert.Equal(sameDevice, device);
        }

        [Fact]
        public async void TryAddDeviceAsync()
        {
            var device = new Device("deviceId")
                         {
                             Authentication = null,
                             Status = new DeviceStatus()
                         };

            var result = await this.iotHubRepository.TryAddDeviceAsync(device);
            Assert.True(result);

            result = await this.iotHubRepository.TryAddDeviceAsync(null);
            Assert.False(result);
        }

        [Fact]
        public async void GetIotHubDeviceAsync()
        {
            var deviceId = this.fixture.Create<string>();
            this.deviceManagerMock.Setup(dm => dm.GetDeviceAsync(deviceId))
                .Returns(Task.FromResult(new Device(deviceId)));
            var d = await this.iotHubRepository.GetIotHubDeviceAsync(deviceId);
            Assert.Equal(deviceId, d.Id);
        }

        [Fact]
        public async void RemoveDeviceAsync()
        {
            var deviceId = this.fixture.Create<string>();
            await this.iotHubRepository.RemoveDeviceAsync(deviceId);
        }

        [Fact]
        public async void TryRemoveDeviceAsync()
        {
            var deviceId = this.fixture.Create<string>();
            var result = await this.iotHubRepository.TryRemoveDeviceAsync(deviceId);
            Assert.True(result);

            this.deviceManagerMock.Setup(dm => dm.RemoveDeviceAsync(It.IsAny<string>())).Throws(new Exception());
            result = await this.iotHubRepository.TryRemoveDeviceAsync(null);
            Assert.False(result);
        }

        [Fact]
        public async void UpdateDeviceEnabledStatusAsync()
        {
            var deviceId = this.fixture.Create<string>();
            var device = new Device(deviceId);
            device.Status = DeviceStatus.Enabled;

            this.deviceManagerMock.Setup(dm => dm.GetDeviceAsync(deviceId))
                .Returns(Task.FromResult(device));

            this.deviceManagerMock.Setup(dm => dm.UpdateDeviceAsync(It.IsAny<Device>()))
                .Returns(Task.FromResult(device));

            var sameDevice = await this.iotHubRepository.UpdateDeviceEnabledStatusAsync(deviceId, false);
            Assert.Equal(sameDevice.Status, DeviceStatus.Disabled);

            sameDevice = await this.iotHubRepository.UpdateDeviceEnabledStatusAsync(deviceId, true);
            Assert.Equal(sameDevice.Status, DeviceStatus.Enabled);
        }

        [Fact]
        public async void SendCommand()
        {
            var commandHistory = this.fixture.Create<CommandHistory>();
            var deviceId = this.fixture.Create<string>();
            this.deviceManagerMock.Setup(dm => dm.SendAsync(deviceId, It.IsAny<Message>())).Returns(Task.FromResult(true));
            this.deviceManagerMock.Setup(dm => dm.CloseAsyncDevice()).Returns(Task.FromResult(true));

            await this.iotHubRepository.SendCommand(deviceId, commandHistory);
        }

        [Fact]
        public async void GetDeviceKeysAsync()
        {
            var deviceId = this.fixture.Create<string>();
            var device = new Device(deviceId);
            var auth = new AuthenticationMechanism();
            auth.SymmetricKey = new SymmetricKey();
            auth.SymmetricKey.PrimaryKey = "1fLjiNCMZF37LmHnjZDyVQ ==";
            auth.SymmetricKey.SecondaryKey = "fbsIV6w7gfVUyoRIQFSVgw ==";
            device.Authentication = auth;
            this.deviceManagerMock.Setup(dm => dm.GetDeviceAsync(deviceId))
                .Returns(Task.FromResult(device));
            
            SecurityKeys keys = await this.iotHubRepository.GetDeviceKeysAsync(deviceId);
            Assert.NotNull(keys);
            deviceId = this.fixture.Create<string>();
            this.deviceManagerMock.Setup(dm => dm.GetDeviceAsync(deviceId))
                .Returns(Task.FromResult<Device>(null));

            keys = await this.iotHubRepository.GetDeviceKeysAsync(deviceId);
            Assert.Null(keys);
        }
    }
}
