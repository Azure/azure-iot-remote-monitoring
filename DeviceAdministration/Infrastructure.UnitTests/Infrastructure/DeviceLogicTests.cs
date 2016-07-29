using System;
using Microsoft.Azure.Devices.Applications.RemoteMonitoring.Common.Configurations;
using Microsoft.Azure.Devices.Applications.RemoteMonitoring.Common.Models;
using Microsoft.Azure.Devices.Applications.RemoteMonitoring.Common.Repository;
using Microsoft.Azure.Devices.Applications.RemoteMonitoring.DeviceAdmin.Infrastructure.BusinessLogic;
using Microsoft.Azure.Devices.Applications.RemoteMonitoring.DeviceAdmin.Infrastructure.Repository;
using Moq;
using Ploeh.AutoFixture;
using System.Threading.Tasks;
using Microsoft.Azure.Devices.Applications.RemoteMonitoring.DeviceAdmin.Infrastructure.Exceptions;
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

        [Fact]
        public async void AddDeviceAsyncTest()
        {
            DeviceModel d1 = fixture.Create<DeviceModel>();
            _iotHubRepositoryMock.Setup(x => x.AddDeviceAsync(It.IsAny<DeviceModel>(), It.IsAny<SecurityKeys>()))
                .ReturnsAsync(d1);

            //Add device without DeviceProperties
            d1.DeviceProperties = null;
            await Assert.ThrowsAsync<ValidationException>(async () => await _deviceLogic.AddDeviceAsync(d1));

            //Add device with Null or empty DeviceId
            d1.DeviceProperties = fixture.Create<DeviceProperties>();
            d1.DeviceProperties.DeviceID = null;
            await Assert.ThrowsAsync<ValidationException>(async () => await _deviceLogic.AddDeviceAsync(d1));
            d1.DeviceProperties.DeviceID = "";
            await Assert.ThrowsAsync<ValidationException>(async () => await _deviceLogic.AddDeviceAsync(d1));

            //Add existing device
            DeviceModel d2 = fixture.Create<DeviceModel>();
            _deviceRegistryCrudRepositoryMock.Setup(x => x.GetDeviceAsync(d2.DeviceProperties.DeviceID)).ReturnsAsync(d2);
            await Assert.ThrowsAsync<ValidationException>(async () => await _deviceLogic.AddDeviceAsync(d2));

            d1.DeviceProperties.DeviceID = fixture.Create<string>();
            var keys = new SecurityKeys("fbsIV6w7gfVUyoRIQFSVgw ==", "1fLjiNCMZF37LmHnjZDyVQ ==");
            _securityKeyGeneratorMock.Setup(x => x.CreateRandomKeys()).Returns(keys);
            var hostname = fixture.Create<string>();
            _configProviderMock.Setup(x => x.GetConfigurationSettingValue(It.IsAny<string>())).Returns(hostname);

            //DocDb throws exception
            _deviceRegistryCrudRepositoryMock.Setup(x => x.AddDeviceAsync(It.IsAny<DeviceModel>()))
                .ThrowsAsync(new Exception());
            _iotHubRepositoryMock.Setup(x => x.TryRemoveDeviceAsync(It.IsAny<string>())).ReturnsAsync(true).Verifiable();
            await Assert.ThrowsAsync<Exception>(async () => await _deviceLogic.AddDeviceAsync(d1));
            _virtualDeviceStorageMock.Verify(x => x.AddOrUpdateDeviceAsync(It.IsAny<InitialDeviceConfig>()),
                Times.Never());
            _iotHubRepositoryMock.Verify(x => x.TryRemoveDeviceAsync(d1.DeviceProperties.DeviceID), Times.Once());

            //Custom device
            d1.IsSimulatedDevice = false;
            _deviceRegistryCrudRepositoryMock.Setup(x => x.AddDeviceAsync(It.IsAny<DeviceModel>())).ReturnsAsync(d1);
            DeviceWithKeys ret = await _deviceLogic.AddDeviceAsync(d1);
            _virtualDeviceStorageMock.Verify(x => x.AddOrUpdateDeviceAsync(It.IsAny<InitialDeviceConfig>()),
                Times.Never());
            Assert.NotNull(ret);
            Assert.Equal(d1, ret.Device);
            Assert.Equal(keys, ret.SecurityKeys);

            //Simulated device
            _deviceRegistryCrudRepositoryMock.Setup(x => x.AddDeviceAsync(It.IsAny<DeviceModel>())).ReturnsAsync(d1);
            _virtualDeviceStorageMock.Setup(x => x.AddOrUpdateDeviceAsync(It.IsAny<InitialDeviceConfig>())).Verifiable();
            d1.IsSimulatedDevice = true;
            ret = await _deviceLogic.AddDeviceAsync(d1);
            _virtualDeviceStorageMock.Verify(x=>x.AddOrUpdateDeviceAsync(It.IsAny<InitialDeviceConfig>()), Times.Once());
            Assert.NotNull(ret);
            Assert.Equal(d1, ret.Device);
            Assert.Equal(keys, ret.SecurityKeys);
        }
    }
}