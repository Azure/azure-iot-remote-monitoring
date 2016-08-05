using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Azure.Devices.Applications.RemoteMonitoring.Common.Configurations;
using Microsoft.Azure.Devices.Applications.RemoteMonitoring.Common.Models;
using Microsoft.Azure.Devices.Applications.RemoteMonitoring.Common.Repository;
using Microsoft.Azure.Devices.Applications.RemoteMonitoring.DeviceAdmin.Infrastructure.BusinessLogic;
using Microsoft.Azure.Devices.Applications.RemoteMonitoring.DeviceAdmin.Infrastructure.Exceptions;
using Microsoft.Azure.Devices.Applications.RemoteMonitoring.DeviceAdmin.Infrastructure.Models;
using Microsoft.Azure.Devices.Applications.RemoteMonitoring.DeviceAdmin.Infrastructure.Repository;
using Moq;
using Newtonsoft.Json;
using Ploeh.AutoFixture;
using Xunit;

namespace Microsoft.Azure.Devices.Applications.RemoteMonitoring.DeviceAdmin.Infrastructure.UnitTests.Infrastructure
{
    public class DeviceLogicTests
    {
        private readonly Mock<IConfigurationProvider> _configProviderMock;
        private readonly IDeviceLogic _deviceLogic;
        private readonly Mock<IDeviceRegistryCrudRepository> _deviceRegistryCrudRepositoryMock;
        private readonly Mock<IDeviceRegistryListRepository> _deviceRegistryListRepositoryMock;
        private readonly Mock<IDeviceRulesLogic> _deviceRulesLogicMock;
        private readonly Mock<IIotHubRepository> _iotHubRepositoryMock;
        private readonly Mock<ISecurityKeyGenerator> _securityKeyGeneratorMock;
        private readonly Mock<IVirtualDeviceStorage> _virtualDeviceStorageMock;
        private readonly Fixture fixture;

        public DeviceLogicTests()
        {
            _iotHubRepositoryMock = new Mock<IIotHubRepository>();
            _deviceRegistryCrudRepositoryMock = new Mock<IDeviceRegistryCrudRepository>();
            _deviceRegistryListRepositoryMock = new Mock<IDeviceRegistryListRepository>();
            _virtualDeviceStorageMock = new Mock<IVirtualDeviceStorage>();
            _configProviderMock = new Mock<IConfigurationProvider>();
            _securityKeyGeneratorMock = new Mock<ISecurityKeyGenerator>();
            _deviceRulesLogicMock = new Mock<IDeviceRulesLogic>();
            _deviceLogic = new DeviceLogic(_iotHubRepositoryMock.Object, _deviceRegistryCrudRepositoryMock.Object,
                _deviceRegistryListRepositoryMock.Object, _virtualDeviceStorageMock.Object,
                _securityKeyGeneratorMock.Object, _configProviderMock.Object, _deviceRulesLogicMock.Object);
            fixture = new Fixture();
        }

        [Fact]
        public async void GetDevicesTest()
        {
            var q = fixture.Create<DeviceListQuery>();
            var r = fixture.Create<DeviceListQueryResult>();
            _deviceRegistryListRepositoryMock.SetupSequence(x => x.GetDeviceList(It.IsAny<DeviceListQuery>()))
                .ReturnsAsync(r)
                .ReturnsAsync(new DeviceListQueryResult());


            var res = await _deviceLogic.GetDevices(q);
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
            var device = fixture.Create<DeviceModel>();
            _deviceRegistryCrudRepositoryMock.Setup(x => x.GetDeviceAsync(device.DeviceProperties.DeviceID))
                .ReturnsAsync(device);

            var retDevice = await _deviceLogic.GetDeviceAsync(device.DeviceProperties.DeviceID);
            Assert.Equal(device, retDevice);

            retDevice = await _deviceLogic.GetDeviceAsync("DeviceNotExist");
            Assert.Null(retDevice);

            retDevice = await _deviceLogic.GetDeviceAsync(null);
            Assert.Null(retDevice);
        }

        [Fact]
        public async void AddDeviceAsyncTest()
        {
            var d1 = fixture.Create<DeviceModel>();
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
            var d2 = fixture.Create<DeviceModel>();
            _deviceRegistryCrudRepositoryMock.Setup(x => x.GetDeviceAsync(d2.DeviceProperties.DeviceID))
                .ReturnsAsync(d2);
            await Assert.ThrowsAsync<ValidationException>(async () => await _deviceLogic.AddDeviceAsync(d2));

            d1.DeviceProperties.DeviceID = fixture.Create<string>();
            var keys = new SecurityKeys("fbsIV6w7gfVUyoRIQFSVgw ==", "1fLjiNCMZF37LmHnjZDyVQ ==");
            _securityKeyGeneratorMock.Setup(x => x.CreateRandomKeys()).Returns(keys);
            var hostname = fixture.Create<string>();
            _configProviderMock.Setup(x => x.GetConfigurationSettingValue(It.IsAny<string>())).Returns(hostname);

            //Device registry throws exception
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
            var ret = await _deviceLogic.AddDeviceAsync(d1);
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
            _virtualDeviceStorageMock.Verify(x => x.AddOrUpdateDeviceAsync(It.IsAny<InitialDeviceConfig>()),
                Times.Once());
            Assert.NotNull(ret);
            Assert.Equal(d1, ret.Device);
            Assert.Equal(keys, ret.SecurityKeys);
        }

        [Fact]
        public async void RemoveDeviceAsyncTest()
        {
            var deviceId = fixture.Create<string>();
            var device = new Device(deviceId);
            _iotHubRepositoryMock.Setup(x => x.GetIotHubDeviceAsync(It.IsNotNull<string>())).ReturnsAsync(device);
            _iotHubRepositoryMock.Setup(x => x.RemoveDeviceAsync(It.IsNotNull<string>())).Returns(Task.FromResult(true));

            //Device not registered with IoTHub
            await
                Assert.ThrowsAsync<DeviceNotRegisteredException>(async () => await _deviceLogic.RemoveDeviceAsync(null));

            //Should pass without any exceptions
            _virtualDeviceStorageMock.Setup(x => x.RemoveDeviceAsync(It.IsNotNull<string>())).ReturnsAsync(true);
            _deviceRulesLogicMock.Setup(x => x.RemoveAllRulesForDeviceAsync(It.IsNotNull<string>())).ReturnsAsync(true);
            await _deviceLogic.RemoveDeviceAsync(deviceId);
            _virtualDeviceStorageMock.Verify(x => x.RemoveDeviceAsync(deviceId), Times.Once());
            _deviceRulesLogicMock.Verify(x => x.RemoveAllRulesForDeviceAsync(deviceId), Times.Once());

            //Device registry throws exception
            _deviceRegistryCrudRepositoryMock.Setup(x => x.RemoveDeviceAsync(It.IsAny<string>()))
                .Throws(new Exception());
            _iotHubRepositoryMock.Setup(x => x.TryAddDeviceAsync(It.IsAny<Device>())).ReturnsAsync(true).Verifiable();
            await Assert.ThrowsAsync<Exception>(async () => await _deviceLogic.RemoveDeviceAsync(deviceId));
            _iotHubRepositoryMock.Verify(x => x.TryAddDeviceAsync(device), Times.Once());
        }

        [Fact]
        public async void UpdateDeviceAsyncTest()
        {
            var d = fixture.Create<DeviceModel>();
            _deviceRegistryCrudRepositoryMock.Setup(x => x.UpdateDeviceAsync(It.IsNotNull<DeviceModel>()))
                .ReturnsAsync(d);

            var r = await _deviceLogic.UpdateDeviceAsync(d);
            Assert.Equal(d, r);
        }

        [Fact]
        public async void UpdateDeviceFromDeviceInfoPacketAsyncTest()
        {
            //Device is null
            await
                Assert.ThrowsAsync<ArgumentNullException>(
                    async () => await _deviceLogic.UpdateDeviceFromDeviceInfoPacketAsync(null));

            var d = fixture.Create<DeviceModel>();
            _deviceRegistryCrudRepositoryMock.Setup(x => x.GetDeviceAsync(d.DeviceProperties.DeviceID))
                .ReturnsAsync(d);
            _deviceRegistryCrudRepositoryMock.Setup(x => x.UpdateDeviceAsync(It.IsAny<DeviceModel>()))
                .ReturnsAsync(d);
            var r = await _deviceLogic.UpdateDeviceFromDeviceInfoPacketAsync(d);
            Assert.Equal(d, r);

            d.SystemProperties = null;
            d.Telemetry = null;
            d.Commands = null;
            r = await _deviceLogic.UpdateDeviceFromDeviceInfoPacketAsync(d);
            Assert.Equal(d, r);
        }

        [Fact]
        public async void SendCommandAsyncTest()
        {
            var d = fixture.Create<DeviceModel>();
            _deviceRegistryCrudRepositoryMock.Setup(x => x.GetDeviceAsync(d.DeviceProperties.DeviceID))
                .ReturnsAsync(d);

            //Invalid device
            await
                Assert.ThrowsAsync<DeviceNotRegisteredException>(
                    async () => await _deviceLogic.SendCommandAsync(null, null, null));

            //Invalid command
            await
                Assert.ThrowsAsync<UnsupportedCommandException>(
                    async () =>
                        await _deviceLogic.SendCommandAsync(d.DeviceProperties.DeviceID, "Invalid command", null));

            //Valid command
            _iotHubRepositoryMock.Setup(x => x.SendCommand(It.IsNotNull<string>(), It.IsNotNull<CommandHistory>()))
                .Returns(Task.FromResult(true));
            _deviceRegistryCrudRepositoryMock.Setup(x => x.UpdateDeviceAsync(It.IsNotNull<DeviceModel>()))
                .ReturnsAsync(new DeviceModel());
            await _deviceLogic.SendCommandAsync(d.DeviceProperties.DeviceID, d.Commands[0].Name, null);
        }

        [Fact]
        public void ExtractLocationsDataTest()
        {
            var listOfDevices = fixture.Create<List<DeviceModel>>();
            var latitudes = new List<double>();
            var longitudes = new List<double>();
            var locations = new List<DeviceLocationModel>();
            foreach (var d in listOfDevices)
            {
                try
                {
                    latitudes.Add(d.DeviceProperties.Latitude.Value);
                    longitudes.Add(d.DeviceProperties.Longitude.Value);
                    locations.Add(new DeviceLocationModel
                    {
                        DeviceId = d.DeviceProperties.DeviceID,
                        Latitude = d.DeviceProperties.Latitude.Value,
                        Longitude = d.DeviceProperties.Longitude.Value
                    });
                }
                catch (Exception)
                {
                }
            }
            var offset = 0.05;
            var minLat = latitudes.Min() - offset;
            var maxLat = latitudes.Max() + offset;
            var minLong = longitudes.Min() - offset;
            var maxLong = longitudes.Max() + offset;

            var res = _deviceLogic.ExtractLocationsData(listOfDevices);
            Assert.NotNull(res);
            Assert.Equal(JsonConvert.SerializeObject(locations), JsonConvert.SerializeObject(res.DeviceLocationList));
            Assert.Equal(minLat, res.MinimumLatitude);
            Assert.Equal(maxLat, res.MaximumLatitude);
            Assert.Equal(minLong, res.MinimumLongitude);
            Assert.Equal(maxLong, res.MaximumLongitude);

            res = _deviceLogic.ExtractLocationsData(null);
            Assert.NotNull(res);
            Assert.Equal(JsonConvert.SerializeObject(new List<DeviceLocationModel>()),
                JsonConvert.SerializeObject(res.DeviceLocationList));
            Assert.Equal(47.6 - offset, res.MinimumLatitude);
            Assert.Equal(47.6 + offset, res.MaximumLatitude);
            Assert.Equal(-122.3 - offset, res.MinimumLongitude);
            Assert.Equal(-122.3 + offset, res.MaximumLongitude);
        }

        [Fact]
        public void ExtractTelemetryTest()
        {
            var d = fixture.Create<DeviceModel>();
            var exp = new List<DeviceTelemetryFieldModel>();
            foreach (var t in d.Telemetry)
            {
                exp.Add(new DeviceTelemetryFieldModel
                {
                    DisplayName = t.DisplayName,
                    Name = t.Name,
                    Type = t.Type
                });
            }

            Assert.Null(_deviceLogic.ExtractTelemetry(null));

            var res = _deviceLogic.ExtractTelemetry(d);
            Assert.Equal(JsonConvert.SerializeObject(exp), JsonConvert.SerializeObject(res));
        }
    }
}