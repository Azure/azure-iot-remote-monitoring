using System;
using System.Threading.Tasks;
using Microsoft.Azure.Devices.Applications.RemoteMonitoring.Common.Models;
using Microsoft.Azure.Devices.Applications.RemoteMonitoring.DeviceAdmin.Infrastructure.Repository;
using Moq;
using Ploeh.AutoFixture;
using Xunit;

namespace Microsoft.Azure.Devices.Applications.RemoteMonitoring.UnitTests.Infrastructure
{
    public class IoTHubRepositoryTests
    {
        private readonly Mock<IIoTHubDeviceManager> deviceManagerMock;
        private readonly Fixture fixture;
        private readonly IIotHubRepository iotHubRepository;

        public IoTHubRepositoryTests()
        {
            deviceManagerMock = new Mock<IIoTHubDeviceManager>();
            deviceManagerMock.Setup(dm => dm.AddDeviceAsync(It.IsAny<Device>())).ReturnsAsync(new Device());
            deviceManagerMock.Setup(dm => dm.RemoveDeviceAsync(It.IsAny<string>())).Returns(Task.FromResult(true));
            iotHubRepository = new IotHubRepository(deviceManagerMock.Object);
            fixture = new Fixture();
        }

        [Fact]
        public async void AddDeviceAsync()
        {
            var device = fixture.Create<DeviceModel>();
            var keys = new SecurityKeys("fbsIV6w7gfVUyoRIQFSVgw ==", "1fLjiNCMZF37LmHnjZDyVQ ==");
            var sameDevice = await iotHubRepository.AddDeviceAsync(device, keys);
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

            var result = await iotHubRepository.TryAddDeviceAsync(device);
            Assert.True(result);

            result = await iotHubRepository.TryAddDeviceAsync(null);
            Assert.False(result);
        }

        [Fact]
        public async void GetIotHubDeviceAsync()
        {
            var deviceId = fixture.Create<string>();
            deviceManagerMock.Setup(dm => dm.GetDeviceAsync(deviceId))
                .ReturnsAsync(new Device(deviceId));
            var d = await iotHubRepository.GetIotHubDeviceAsync(deviceId);
            Assert.Equal(deviceId, d.Id);
        }

        [Fact]
        public async void RemoveDeviceAsync()
        {
            var deviceId = fixture.Create<string>();
            await iotHubRepository.RemoveDeviceAsync(deviceId);
            deviceManagerMock.Verify(mock => mock.RemoveDeviceAsync(deviceId), Times.Once());
        }

        [Fact]
        public async void TryRemoveDeviceAsync()
        {
            var deviceId = fixture.Create<string>();
            var result = await iotHubRepository.TryRemoveDeviceAsync(deviceId);
            Assert.True(result);

            deviceManagerMock.Setup(dm => dm.RemoveDeviceAsync(It.IsAny<string>())).Throws(new Exception());
            result = await iotHubRepository.TryRemoveDeviceAsync(null);
            Assert.False(result);
        }

        [Fact]
        public async void UpdateDeviceEnabledStatusAsync()
        {
            var deviceId = fixture.Create<string>();
            var device = new Device(deviceId);
            device.Status = DeviceStatus.Enabled;

            deviceManagerMock.Setup(dm => dm.GetDeviceAsync(deviceId))
                .ReturnsAsync(device);

            deviceManagerMock.Setup(dm => dm.UpdateDeviceAsync(It.IsAny<Device>()))
                .ReturnsAsync(device);

            var sameDevice = await iotHubRepository.UpdateDeviceEnabledStatusAsync(deviceId, false);
            Assert.Equal(sameDevice.Status, DeviceStatus.Disabled);

            sameDevice = await iotHubRepository.UpdateDeviceEnabledStatusAsync(deviceId, true);
            Assert.Equal(sameDevice.Status, DeviceStatus.Enabled);
        }

        [Fact]
        public async void SendCommand()
        {
            var commandHistory = fixture.Create<CommandHistory>();
            var deviceId = fixture.Create<string>();
            deviceManagerMock.Setup(dm => dm.SendAsync(deviceId, It.IsAny<Message>())).Returns(Task.FromResult(true));
            deviceManagerMock.Setup(dm => dm.CloseAsyncDevice()).Returns(Task.FromResult(true));

            await iotHubRepository.SendCommand(deviceId, commandHistory);
            deviceManagerMock.Verify(mock => mock.SendAsync(deviceId, It.IsAny<Message>()), Times.Once());
            deviceManagerMock.Verify(mock => mock.CloseAsyncDevice(), Times.Once());
        }

        [Fact]
        public async void GetDeviceKeysAsync()
        {
            var deviceId = fixture.Create<string>();
            var device = new Device(deviceId);
            var auth = new AuthenticationMechanism();
            auth.SymmetricKey = new SymmetricKey();
            auth.SymmetricKey.PrimaryKey = "1fLjiNCMZF37LmHnjZDyVQ ==";
            auth.SymmetricKey.SecondaryKey = "fbsIV6w7gfVUyoRIQFSVgw ==";
            device.Authentication = auth;
            deviceManagerMock.Setup(dm => dm.GetDeviceAsync(deviceId))
                .ReturnsAsync(device);

            var keys = await iotHubRepository.GetDeviceKeysAsync(deviceId);
            Assert.NotNull(keys);
            deviceId = fixture.Create<string>();
            deviceManagerMock.Setup(dm => dm.GetDeviceAsync(deviceId))
                .ReturnsAsync(null);

            keys = await iotHubRepository.GetDeviceKeysAsync(deviceId);
            Assert.Null(keys);
        }
    }
}