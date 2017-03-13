using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Azure.Devices.Applications.RemoteMonitoring.Common.Configurations;
using Microsoft.Azure.Devices.Applications.RemoteMonitoring.Common.Exceptions;
using Microsoft.Azure.Devices.Applications.RemoteMonitoring.Common.Extensions;
using Microsoft.Azure.Devices.Applications.RemoteMonitoring.Common.Models;
using Microsoft.Azure.Devices.Applications.RemoteMonitoring.Common.Models.Commands;
using Microsoft.Azure.Devices.Applications.RemoteMonitoring.Common.Repository;
using Microsoft.Azure.Devices.Applications.RemoteMonitoring.DeviceAdmin.Infrastructure.BusinessLogic;
using Microsoft.Azure.Devices.Applications.RemoteMonitoring.DeviceAdmin.Infrastructure.Exceptions;
using Microsoft.Azure.Devices.Applications.RemoteMonitoring.DeviceAdmin.Infrastructure.Models;
using Microsoft.Azure.Devices.Applications.RemoteMonitoring.DeviceAdmin.Infrastructure.Repository;
using Moq;
using Newtonsoft.Json;
using Ploeh.AutoFixture;
using Xunit;

namespace Microsoft.Azure.Devices.Applications.RemoteMonitoring.UnitTests.Infrastructure
{
    public class DeviceLogicTests
    {
        private readonly Mock<IConfigurationProvider> _configProviderMock;
        private readonly IDeviceLogic _deviceLogic;
        private readonly Mock<IDeviceRegistryCrudRepository> _deviceRegistryCrudRepositoryMock;
        private readonly Mock<IDeviceRegistryListRepository> _deviceRegistryListRepositoryMock;
        private readonly Mock<IDeviceRulesLogic> _deviceRulesLogicMock;
        private readonly Mock<INameCacheLogic> _nameCacheLogicMock;
        private readonly Mock<IIotHubRepository> _iotHubRepositoryMock;
        private readonly Mock<ISecurityKeyGenerator> _securityKeyGeneratorMock;
        private readonly Mock<IVirtualDeviceStorage> _virtualDeviceStorageMock;
        private readonly Mock<IDeviceListFilterRepository> _deviceListFilterMock;
        private readonly Fixture fixture;

        public DeviceLogicTests()
        {
            this._iotHubRepositoryMock = new Mock<IIotHubRepository>();
            this._deviceRegistryCrudRepositoryMock = new Mock<IDeviceRegistryCrudRepository>();
            this._deviceRegistryListRepositoryMock = new Mock<IDeviceRegistryListRepository>();
            this._virtualDeviceStorageMock = new Mock<IVirtualDeviceStorage>();
            this._configProviderMock = new Mock<IConfigurationProvider>();
            this._securityKeyGeneratorMock = new Mock<ISecurityKeyGenerator>();
            this._deviceRulesLogicMock = new Mock<IDeviceRulesLogic>();
            this._nameCacheLogicMock = new Mock<INameCacheLogic>();
            this._deviceListFilterMock = new Mock<IDeviceListFilterRepository>();
            this._deviceLogic = new DeviceLogic(this._iotHubRepositoryMock.Object,
                                                this._deviceRegistryCrudRepositoryMock.Object,
                                                this._deviceRegistryListRepositoryMock.Object,
                                                this._virtualDeviceStorageMock.Object,
                                                this._securityKeyGeneratorMock.Object,
                                                this._configProviderMock.Object,
                                                this._deviceRulesLogicMock.Object,
                                                this._nameCacheLogicMock.Object,
                                                this._deviceListFilterMock.Object);
            this.fixture = new Fixture();
        }

        [Fact]
        public async void GetDevicesTest()
        {
            var filter = this.fixture.Create<DeviceListFilter>();
            var result = this.fixture.Create<DeviceListFilterResult>();
            this._deviceRegistryListRepositoryMock.SetupSequence(x => x.GetDeviceList(It.IsNotNull<DeviceListFilter>()))
                .ReturnsAsync(result)
                .ReturnsAsync(new DeviceListFilterResult());

            var res = await this._deviceLogic.GetDevices(filter);
            Assert.NotNull(res);
            Assert.NotNull(res.Results);
            Assert.NotEqual(0, res.TotalDeviceCount);
            Assert.NotEqual(0, res.TotalFilteredCount);
        }

        [Fact]
        public async void GetDeviceAsyncTest()
        {
            var device = this.fixture.Create<DeviceModel>();
            this._deviceRegistryCrudRepositoryMock.Setup(x => x.GetDeviceAsync(device.DeviceProperties.DeviceID))
                .ReturnsAsync(device);

            var retDevice = await this._deviceLogic.GetDeviceAsync(device.DeviceProperties.DeviceID);
            Assert.Equal(device, retDevice);

            retDevice = await this._deviceLogic.GetDeviceAsync("DeviceNotExist");
            Assert.Null(retDevice);

            retDevice = await this._deviceLogic.GetDeviceAsync(null);
            Assert.Null(retDevice);
        }

        [Fact]
        public async void AddDeviceAsyncTest()
        {
            var d1 = this.fixture.Create<DeviceModel>();
            this._iotHubRepositoryMock.Setup(x => x.AddDeviceAsync(It.IsAny<DeviceModel>(), It.IsAny<SecurityKeys>()))
                .ReturnsAsync(d1);

            //Add device without DeviceProperties
            d1.DeviceProperties = null;
            await Assert.ThrowsAsync<ValidationException>(async () => await this._deviceLogic.AddDeviceAsync(d1));

            //Add device with Null or empty DeviceId
            d1.DeviceProperties = this.fixture.Create<DeviceProperties>();
            d1.DeviceProperties.DeviceID = null;
            await Assert.ThrowsAsync<ValidationException>(async () => await this._deviceLogic.AddDeviceAsync(d1));
            d1.DeviceProperties.DeviceID = "";
            await Assert.ThrowsAsync<ValidationException>(async () => await this._deviceLogic.AddDeviceAsync(d1));

            //Add existing device
            var d2 = this.fixture.Create<DeviceModel>();
            this._deviceRegistryCrudRepositoryMock.Setup(x => x.GetDeviceAsync(d2.DeviceProperties.DeviceID))
                .ReturnsAsync(d2);
            await Assert.ThrowsAsync<ValidationException>(async () => await this._deviceLogic.AddDeviceAsync(d2));

            d1.DeviceProperties.DeviceID = this.fixture.Create<string>();
            var keys = new SecurityKeys("fbsIV6w7gfVUyoRIQFSVgw ==", "1fLjiNCMZF37LmHnjZDyVQ ==");
            this._securityKeyGeneratorMock.Setup(x => x.CreateRandomKeys()).Returns(keys);
            var hostname = this.fixture.Create<string>();
            this._configProviderMock.Setup(x => x.GetConfigurationSettingValue(It.IsAny<string>())).Returns(hostname);

            //Device registry throws exception
            this._deviceRegistryCrudRepositoryMock.Setup(x => x.AddDeviceAsync(It.IsAny<DeviceModel>()))
                .ThrowsAsync(new Exception());
            this._iotHubRepositoryMock.Setup(x => x.TryRemoveDeviceAsync(It.IsAny<string>())).ReturnsAsync(true).Verifiable();
            await Assert.ThrowsAsync<Exception>(async () => await this._deviceLogic.AddDeviceAsync(d1));
            this._virtualDeviceStorageMock.Verify(x => x.AddOrUpdateDeviceAsync(It.IsAny<InitialDeviceConfig>()),
                                                  Times.Never());
            this._iotHubRepositoryMock.Verify(x => x.TryRemoveDeviceAsync(d1.DeviceProperties.DeviceID), Times.Once());

            //Custom device
            d1.IsSimulatedDevice = false;
            this._deviceRegistryCrudRepositoryMock.Setup(x => x.AddDeviceAsync(It.IsAny<DeviceModel>())).ReturnsAsync(d1);
            var ret = await this._deviceLogic.AddDeviceAsync(d1);
            this._virtualDeviceStorageMock.Verify(x => x.AddOrUpdateDeviceAsync(It.IsAny<InitialDeviceConfig>()),
                                                  Times.Never());
            Assert.NotNull(ret);
            Assert.Equal(d1, ret.Device);
            Assert.Equal(keys, ret.SecurityKeys);

            //Simulated device
            this._deviceRegistryCrudRepositoryMock.Setup(x => x.AddDeviceAsync(It.IsAny<DeviceModel>())).ReturnsAsync(d1);
            this._virtualDeviceStorageMock.Setup(x => x.AddOrUpdateDeviceAsync(It.IsAny<InitialDeviceConfig>())).Verifiable();
            d1.IsSimulatedDevice = true;
            ret = await this._deviceLogic.AddDeviceAsync(d1);
            this._virtualDeviceStorageMock.Verify(x => x.AddOrUpdateDeviceAsync(It.IsAny<InitialDeviceConfig>()),
                                                  Times.Once());
            Assert.NotNull(ret);
            Assert.Equal(d1, ret.Device);
            Assert.Equal(keys, ret.SecurityKeys);
        }

        [Fact]
        public async void RemoveDeviceAsyncTest()
        {
            var deviceId = this.fixture.Create<string>();
            var device = new Device(deviceId);
            this._iotHubRepositoryMock.Setup(x => x.GetIotHubDeviceAsync(It.IsNotNull<string>())).ReturnsAsync(device);
            this._iotHubRepositoryMock.Setup(x => x.RemoveDeviceAsync(It.IsNotNull<string>())).Returns(Task.FromResult(true));

            //Device not registered with IoTHub
            await
                Assert.ThrowsAsync<DeviceNotRegisteredException>(async () => await this._deviceLogic.RemoveDeviceAsync(null));

            //Should pass without any exceptions
            this._virtualDeviceStorageMock.Setup(x => x.RemoveDeviceAsync(It.IsNotNull<string>())).ReturnsAsync(true);
            this._deviceRulesLogicMock.Setup(x => x.RemoveAllRulesForDeviceAsync(It.IsNotNull<string>())).ReturnsAsync(true);
            await this._deviceLogic.RemoveDeviceAsync(deviceId);
            this._virtualDeviceStorageMock.Verify(x => x.RemoveDeviceAsync(deviceId), Times.Once());
            this._deviceRulesLogicMock.Verify(x => x.RemoveAllRulesForDeviceAsync(deviceId), Times.Once());

            //Device registry throws exception
            this._deviceRegistryCrudRepositoryMock.Setup(x => x.RemoveDeviceAsync(It.IsAny<string>()))
                .Throws(new Exception());
            this._iotHubRepositoryMock.Setup(x => x.TryAddDeviceAsync(It.IsAny<Device>())).ReturnsAsync(true).Verifiable();
            await Assert.ThrowsAsync<Exception>(async () => await this._deviceLogic.RemoveDeviceAsync(deviceId));
            this._iotHubRepositoryMock.Verify(x => x.TryAddDeviceAsync(device), Times.Once());
        }

        [Fact]
        public async void UpdateDeviceAsyncTest()
        {
            var d = this.fixture.Create<DeviceModel>();
            this._deviceRegistryCrudRepositoryMock.Setup(x => x.UpdateDeviceAsync(It.IsNotNull<DeviceModel>()))
                .ReturnsAsync(d);

            var r = await this._deviceLogic.UpdateDeviceAsync(d);
            Assert.Equal(d, r);
        }

        [Fact]
        public async void UpdateDeviceFromDeviceInfoPacketAsyncTest()
        {
            //Device is null
            await
                Assert.ThrowsAsync<ArgumentNullException>(
                                                          async () => await this._deviceLogic.UpdateDeviceFromDeviceInfoPacketAsync(null));

            var d = this.fixture.Create<DeviceModel>();
            this._deviceRegistryCrudRepositoryMock.Setup(x => x.GetDeviceAsync(d.IoTHub.ConnectionDeviceId))
                .ReturnsAsync(d);
            this._deviceRegistryCrudRepositoryMock.Setup(x => x.UpdateDeviceAsync(It.IsAny<DeviceModel>()))
                .ReturnsAsync(d);
            var r = await this._deviceLogic.UpdateDeviceFromDeviceInfoPacketAsync(d);
            Assert.Equal(d, r);

            d.SystemProperties = null;
            d.Telemetry = null;
            d.Commands = null;
            r = await this._deviceLogic.UpdateDeviceFromDeviceInfoPacketAsync(d);
            Assert.Equal(d, r);
        }

        [Fact]
        public async void SendCommandAsyncTest()
        {
            var d = this.fixture.Create<DeviceModel>();
            this._deviceRegistryCrudRepositoryMock.Setup(x => x.GetDeviceAsync(d.DeviceProperties.DeviceID))
                .ReturnsAsync(d);

            //Invalid device
            await
                Assert.ThrowsAsync<DeviceNotRegisteredException>(
                                                                 async () => await this._deviceLogic.SendCommandAsync(null, null, DeliveryType.Message, null));

            //Invalid command
            await
                Assert.ThrowsAsync<UnsupportedCommandException>(
                                                                async () =>
                                                                await
                                                                this._deviceLogic.SendCommandAsync(d.DeviceProperties.DeviceID,
                                                                                                   "Invalid command",
                                                                                                   DeliveryType.Message,
                                                                                                   null));

            //Valid command
            this._iotHubRepositoryMock.Setup(x => x.SendCommand(It.IsNotNull<string>(), It.IsNotNull<CommandHistory>()))
                .Returns(Task.FromResult(true));
            this._deviceRegistryCrudRepositoryMock.Setup(x => x.UpdateDeviceAsync(It.IsNotNull<DeviceModel>()))
                .ReturnsAsync(new DeviceModel());
            await this._deviceLogic.SendCommandAsync(d.DeviceProperties.DeviceID, d.Commands[0].Name, d.Commands[0].DeliveryType, null);
        }

        [Fact]
        public void ExtractLocationsDataTest()
        {
            var listOfDevices = this.fixture.Create<List<DeviceModel>>();
            foreach (var d in listOfDevices)
            {
                d.Twin.Properties.Reported.Set("Device.Location.Latitude", fixture.Create<double>());
                d.Twin.Properties.Reported.Set("Device.Location.Longitude", fixture.Create<double>());
            }

            var latitudes = new List<double>();
            var longitudes = new List<double>();
            var locations = new List<DeviceLocationModel>();
            foreach (var d in listOfDevices)
            {
                try
                {
                    latitudes.Add((double)d.Twin.Properties.Reported.Get("Device.Location.Latitude"));
                    longitudes.Add((double)d.Twin.Properties.Reported.Get("Device.Location.Longitude"));
                    locations.Add(new DeviceLocationModel
                    {
                        DeviceId = d.DeviceProperties.DeviceID,
                        Latitude = (double)d.Twin.Properties.Reported.Get("Device.Location.Latitude"),
                        Longitude = (double)d.Twin.Properties.Reported.Get("Device.Location.Longitude")
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

            var res = this._deviceLogic.ExtractLocationsData(listOfDevices);
            Assert.NotNull(res);
            Assert.Equal(JsonConvert.SerializeObject(locations), JsonConvert.SerializeObject(res.DeviceLocationList));
            Assert.Equal(minLat, res.MinimumLatitude);
            Assert.Equal(maxLat, res.MaximumLatitude);
            Assert.Equal(minLong, res.MinimumLongitude);
            Assert.Equal(maxLong, res.MaximumLongitude);

            res = this._deviceLogic.ExtractLocationsData(null);
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
            var d = this.fixture.Create<DeviceModel>();
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

            Assert.Null(this._deviceLogic.ExtractTelemetry(null));

            var res = this._deviceLogic.ExtractTelemetry(d);
            Assert.Equal(JsonConvert.SerializeObject(exp), JsonConvert.SerializeObject(res));
        }

        [Fact]
        public async void UpdateDeviceEnabledStatusAsyncTest_customDevice()
        {
            var deviceId = this.fixture.Create<string>();
            var isEnabled = this.fixture.Create<bool>();
            var device = this.fixture.Create<DeviceModel>();
            device.IsSimulatedDevice = false;
            this._iotHubRepositoryMock.Setup(mock => mock.UpdateDeviceEnabledStatusAsync(deviceId, isEnabled)).ReturnsAsync(new Device());
            this._deviceRegistryCrudRepositoryMock.SetupSequence(mock => mock.UpdateDeviceEnabledStatusAsync(deviceId, isEnabled))
                .ReturnsAsync(device)
                .Throws<Exception>();
            var res = await this._deviceLogic.UpdateDeviceEnabledStatusAsync(deviceId, isEnabled);
            Assert.Equal(res, device);

            this._iotHubRepositoryMock.Setup(mock => mock.UpdateDeviceEnabledStatusAsync(deviceId, !isEnabled)).ReturnsAsync(new Device());
            await Assert.ThrowsAsync<Exception>(async () => await this._deviceLogic.UpdateDeviceEnabledStatusAsync(deviceId, isEnabled));
        }

        [Fact]
        public async void UpdateDeviceEnabledStatusAsyncTest_simulatedDevice()
        {
            var device = this.fixture.Create<DeviceModel>();
            var keys = fixture.Create<SecurityKeys>();
            var hostname = "hostname";
            device.IsSimulatedDevice = true;
            var deviceId = device.DeviceProperties.DeviceID;
            InitialDeviceConfig savedConfig = null;
            this._configProviderMock.Setup(x => x.GetConfigurationSettingValue(It.IsAny<string>())).Returns(hostname);
            this._iotHubRepositoryMock.Setup(mock => mock.UpdateDeviceEnabledStatusAsync(deviceId, It.IsAny<bool>())).ReturnsAsync(new Device());
            this._iotHubRepositoryMock.Setup(mock => mock.GetDeviceKeysAsync(deviceId)).ReturnsAsync(keys);
            this._deviceRegistryCrudRepositoryMock.Setup(
                mock => mock.UpdateDeviceEnabledStatusAsync(deviceId, It.IsAny<bool>()))
                .ReturnsAsync(device);
            this._virtualDeviceStorageMock.Setup(
                mock => mock.AddOrUpdateDeviceAsync(It.IsNotNull<InitialDeviceConfig>()))
                .Callback<InitialDeviceConfig>(conf => savedConfig = conf)
                .Returns(Task.FromResult(true))
                .Verifiable();

            // Enable simulated device
            var res = await this._deviceLogic.UpdateDeviceEnabledStatusAsync(deviceId, true);
            Assert.Equal(res, device);
            _virtualDeviceStorageMock.Verify();
            Assert.Equal(deviceId, savedConfig.DeviceId);
            Assert.Equal(hostname, savedConfig.HostName);
            Assert.Equal(keys.PrimaryKey, savedConfig.Key);

            this._virtualDeviceStorageMock.Setup(mock => mock.RemoveDeviceAsync(deviceId))
                .Returns(Task.FromResult(true))
                .Verifiable();
            // Disable simulated device
            res = await this._deviceLogic.UpdateDeviceEnabledStatusAsync(deviceId, false);
            Assert.Equal(res, device);
            _virtualDeviceStorageMock.Verify();
        }

        [Fact]
        public void ApplyDevicePropertyValueModelsTest()
        {
            var device = this.fixture.Create<DeviceModel>();
            var devicePropertyValueModels = this.fixture.Create<IEnumerable<DevicePropertyValueModel>>();
            this._deviceLogic.ApplyDevicePropertyValueModels(device, devicePropertyValueModels);

            Assert.Throws<ArgumentNullException>(() => this._deviceLogic.ApplyDevicePropertyValueModels(null, devicePropertyValueModels));
            Assert.Throws<ArgumentNullException>(() => this._deviceLogic.ApplyDevicePropertyValueModels(device, null));
            device.DeviceProperties = null;
            Assert.Throws<DeviceRequiredPropertyNotFoundException>(
                                                                   () =>
                                                                   this._deviceLogic.ApplyDevicePropertyValueModels(device, devicePropertyValueModels));
        }

        [Fact]
        public void ExtractDevicePropertyValuesModelsTest()
        {
            var device = this.fixture.Create<DeviceModel>();
            this._configProviderMock.Setup(mock => mock.GetConfigurationSettingValue("iotHub.HostName")).Returns("hostName");
            var res = this._deviceLogic.ExtractDevicePropertyValuesModels(device);
            Assert.Equal(res.Count(), 19);
            Assert.Equal(res.Last().Name, "HostName");
            Assert.Equal(res.Last().Value, "hostName");

            Assert.Throws<ArgumentNullException>(() => this._deviceLogic.ExtractDevicePropertyValuesModels(null));
            device.DeviceProperties = null;
            Assert.Throws<DeviceRequiredPropertyNotFoundException>(
                                                                   () =>
                                                                   this._deviceLogic.ExtractDevicePropertyValuesModels(device));
        }

        [Fact]
        public async Task GenerateNDevicesTest()
        {
            var device = this.fixture.Create<DeviceModel>();
            this._iotHubRepositoryMock.Setup(mock => mock.AddDeviceAsync(It.IsAny<DeviceModel>(), It.IsAny<SecurityKeys>()))
                .ReturnsAsync(new DeviceModel());

            this._deviceRegistryCrudRepositoryMock.Setup(mock => mock.AddDeviceAsync(It.IsAny<DeviceModel>())).ReturnsAsync(device);
            this._configProviderMock.Setup(mock => mock.GetConfigurationSettingValue("iotHub.HostName")).Returns("hostName");
            this._virtualDeviceStorageMock.Setup(mock => mock.AddOrUpdateDeviceAsync(It.IsAny<InitialDeviceConfig>())).Returns(Task.FromResult(true));

            await this._deviceLogic.GenerateNDevices(10);
            this._deviceRegistryCrudRepositoryMock.Verify(mock => mock.AddDeviceAsync(It.IsAny<DeviceModel>()), Times.Exactly(10));
        }
    }
}