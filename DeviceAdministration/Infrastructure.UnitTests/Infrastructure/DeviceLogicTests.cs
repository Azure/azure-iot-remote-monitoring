using Microsoft.Azure.Devices.Applications.RemoteMonitoring.Common.Configurations;
using Microsoft.Azure.Devices.Applications.RemoteMonitoring.Common.Models;
using Microsoft.Azure.Devices.Applications.RemoteMonitoring.Common.Repository;
using Microsoft.Azure.Devices.Applications.RemoteMonitoring.DeviceAdmin.Infrastructure.BusinessLogic;
using Microsoft.Azure.Devices.Applications.RemoteMonitoring.DeviceAdmin.Infrastructure.Repository;
using Moq;
using Ploeh.AutoFixture;
using System.Threading.Tasks;
using Xunit;

namespace Microsoft.Azure.Devices.Applications.RemoteMonitoring.DeviceAdmin.Infrastructure.UnitTests.Infrastructure
{
    public class DeviceLogicTests
    {
        private IIotHubRepository _iotHubRepositoryMock;
        private IDeviceRegistryCrudRepository _deviceRegistryCrudRepositoryMock;
        private IDeviceRegistryListRepository _deviceRegistryListRepositoryMock;
        private IVirtualDeviceStorage _virtualDeviceStorageMock;
        private IConfigurationProvider _configProviderMock;
        private ISecurityKeyGenerator _securityKeyGeneratorMock;
        private IDeviceRulesLogic _deviceRulesLogicMock;
        private IDeviceLogic _deviceLogicMock;

        public DeviceLogicTests()
        {
            _iotHubRepositoryMock = (new Mock<IIotHubRepository>()).Object;
            _deviceRegistryCrudRepositoryMock = (new Mock<IDeviceRegistryCrudRepository>()).Object;
            _deviceRegistryListRepositoryMock = (new Mock<IDeviceRegistryListRepository>()).Object;
            _virtualDeviceStorageMock = (new Mock<IVirtualDeviceStorage>()).Object;
            _configProviderMock = (new Mock<IConfigurationProvider>()).Object;
            _securityKeyGeneratorMock = (new Mock<ISecurityKeyGenerator>()).Object;
            _deviceRulesLogicMock = (new Mock<IDeviceRulesLogic>()).Object;
        }

        ~DeviceLogicTests() { }

        [Fact]
        public async void GetDeviceAsyncTest()
        {
            var fixture = new Fixture();
            var deviceRegistryMock = new Mock<IDeviceRegistryCrudRepository>();
            DeviceModel device = fixture.Create<DeviceModel>();
            deviceRegistryMock.Setup(x => x.GetDeviceAsync(device.DeviceProperties.DeviceID)).Returns(Task.FromResult(device));
            _deviceLogicMock = new DeviceLogic(_iotHubRepositoryMock, deviceRegistryMock.Object, _deviceRegistryListRepositoryMock, _virtualDeviceStorageMock, _securityKeyGeneratorMock, _configProviderMock, _deviceRulesLogicMock);
            DeviceModel retDevice = await _deviceLogicMock.GetDeviceAsync(device.DeviceProperties.DeviceID);
            Assert.Equal(device, retDevice);
        }
    }
}