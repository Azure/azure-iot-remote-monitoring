using Microsoft.Azure.Devices.Applications.RemoteMonitoring.Common.Configurations;
using Microsoft.Azure.Devices.Applications.RemoteMonitoring.Common.Models;
using Microsoft.Azure.Devices.Applications.RemoteMonitoring.Common.Repository;
using Microsoft.Azure.Devices.Applications.RemoteMonitoring.DeviceAdmin.Infrastructure.BusinessLogic;
using Microsoft.Azure.Devices.Applications.RemoteMonitoring.DeviceAdmin.Infrastructure.Repository;
using Moq;
using Ploeh.AutoFixture;
using System.Threading.Tasks;
using Microsoft.Azure.Devices.Applications.RemoteMonitoring.DeviceAdmin.Infrastructure.Models;
using Xunit;

namespace Microsoft.Azure.Devices.Applications.RemoteMonitoring.DeviceAdmin.Infrastructure.UnitTests.Infrastructure
{
    public class DeviceLogicTests
    {
        private Mock<IIotHubRepository> _iotHubRepositoryMock;
        private Mock<IDeviceRegistryCrudRepository> _deviceRegistryCrudRepositoryMock;
        private Mock<IDeviceRegistryListRepository> _deviceRegistryListRepositoryMock;
        private Mock<IVirtualDeviceStorage> _virtualDeviceStorageMock;
        private Mock<IConfigurationProvider> _configProviderMock;
        private Mock<ISecurityKeyGenerator> _securityKeyGeneratorMock;
        private Mock<IDeviceRulesLogic> _deviceRulesLogicMock;
        private IDeviceLogic _deviceLogic;
        private Fixture fixture;

        public DeviceLogicTests()
        {
            _iotHubRepositoryMock = new Mock<IIotHubRepository>();
            _deviceRegistryCrudRepositoryMock = new Mock<IDeviceRegistryCrudRepository>();
            _deviceRegistryListRepositoryMock = new Mock<IDeviceRegistryListRepository>();
            _virtualDeviceStorageMock = new Mock<IVirtualDeviceStorage>();
            _configProviderMock = new Mock<IConfigurationProvider>();
            _securityKeyGeneratorMock = new Mock<ISecurityKeyGenerator>();
            _deviceRulesLogicMock = new Mock<IDeviceRulesLogic>();
            _deviceLogic = new DeviceLogic(_iotHubRepositoryMock.Object, _deviceRegistryCrudRepositoryMock.Object, _deviceRegistryListRepositoryMock.Object, _virtualDeviceStorageMock.Object, _securityKeyGeneratorMock.Object, _configProviderMock.Object, _deviceRulesLogicMock.Object);
            fixture = new Fixture();
        }

        [Fact]
        public async void GetDevicesTest()
        {
            DeviceListQuery q = fixture.Create<DeviceListQuery>();
            DeviceListQueryResult r = fixture.Create<DeviceListQueryResult>();
            _deviceRegistryListRepositoryMock.SetupSequence(x => x.GetDeviceList(It.IsAny<DeviceListQuery>()))
                .ReturnsAsync(r)
                .ReturnsAsync(new DeviceListQueryResult());
            

            DeviceListQueryResult res = await _deviceLogic.GetDevices(q);
            Assert.NotNull(res);
            Assert.NotNull(res.Results);
            Assert.NotEqual(0, res.TotalDeviceCount);
            Assert.NotEqual(0, res.TotalFilteredCount);

            res = await _deviceLogic.GetDevices(null);
            Assert.NotNull(res);
            Assert.Null(res.Results);
            Assert.Equal(0, res.TotalDeviceCount);
            Assert.Equal(0, res.TotalFilteredCount);
        }

        [Fact]
        public async void GetDeviceAsyncTest()
        {
            DeviceModel device = fixture.Create<DeviceModel>();
            _deviceRegistryCrudRepositoryMock.Setup(x => x.GetDeviceAsync(device.DeviceProperties.DeviceID)).ReturnsAsync(device);

            DeviceModel retDevice = await _deviceLogic.GetDeviceAsync(device.DeviceProperties.DeviceID);
            Assert.Equal(device, retDevice);

            retDevice = await _deviceLogic.GetDeviceAsync("DeviceNotExist");
            Assert.Null(retDevice);

            retDevice = await _deviceLogic.GetDeviceAsync(null);
            Assert.Null(retDevice);
        }
    }
}