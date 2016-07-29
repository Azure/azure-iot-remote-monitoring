using Microsoft.Azure.Devices.Applications.RemoteMonitoring.DeviceAdmin.Infrastructure.Repository;
using Moq;
using Xunit;

namespace Microsoft.Azure.Devices.Applications.RemoteMonitoring.DeviceAdmin.Infrastructure.UnitTests.Infrastructure
{
    public class IoTHubRepositoryTests
    {
        private IIotHubRepository iotHubRepository;
        private readonly Mock<IDeviceManager> deviceManagerMock;

        public IoTHubRepositoryTests()
        {
            this.deviceManagerMock = new Mock<IDeviceManager>();
            this.iotHubRepository = new IotHubRepository(this.deviceManagerMock.Object);
        }

        [Fact]
        public void AddDeviceAsync()
        {
         
        }

        [Fact]
        public void TryAddDeviceAsync()
        {
        }

        [Fact]
        public void GetIotHubDeviceAsync()
        {
        }

        [Fact]
        public void RemoveDeviceAsync()
        {
        }

        [Fact]
        public void TryRemoveDeviceAsync()
        {
        }

        [Fact]
        public void UpdateDeviceEnabledStatusAsync()
        {
        }

        [Fact]
        public void SendCommand()
        {
        }

        [Fact]
        public void GetDeviceKeysAsync()
        {
        }
    }
}
