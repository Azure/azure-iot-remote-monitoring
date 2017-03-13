using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using Microsoft.Azure.Devices.Applications.RemoteMonitoring.Common.Configurations;
using Microsoft.Azure.Devices.Applications.RemoteMonitoring.Common.Models;
using Microsoft.Azure.Devices.Applications.RemoteMonitoring.Common.Repository;
using Microsoft.Azure.Devices.Applications.RemoteMonitoring.DeviceAdmin.Infrastructure.BusinessLogic;
using Microsoft.Azure.Devices.Applications.RemoteMonitoring.DeviceAdmin.Infrastructure.Models;
using Microsoft.Azure.Devices.Applications.RemoteMonitoring.DeviceAdmin.Infrastructure.Repository;
using Moq;
using Ploeh.AutoFixture;
using Xunit;

namespace Microsoft.Azure.Devices.Applications.RemoteMonitoring.UnitTests.Infrastructure
{
    public class DeviceLogicWithIoTHubDMTests
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

        public DeviceLogicWithIoTHubDMTests()
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
            this._deviceLogic = new DeviceLogicWithIoTHubDM(this._iotHubRepositoryMock.Object,
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
        public void ApplyDevicePropertyValueModelsTest()
        {
            var device = this.fixture.Create<DeviceModel>();
            var devicePropertyValueModels = this.fixture.Create<IEnumerable<DevicePropertyValueModel>>();
            devicePropertyValueModels = devicePropertyValueModels.Concat(new DevicePropertyValueModel[]
            {
                new DevicePropertyValueModel
                {
                    Name = "tags.x",
                    Value = "one"
                },
                new DevicePropertyValueModel
                {
                    Name = "properties.desired.y",
                    Value = "two"
                },
                new DevicePropertyValueModel
                {
                    Name = "properties.reported.z",
                    Value = "three"
                },
            });

            this._deviceLogic.ApplyDevicePropertyValueModels(device, devicePropertyValueModels);

            Assert.Equal(device.Twin.Tags["x"].ToString(), "one");
            Assert.Equal(device.Twin.Properties.Desired["y"].ToString(), "two");
            Assert.False(device.Twin.Properties.Reported.Contains("z"));

            Assert.Throws<ArgumentNullException>(() => this._deviceLogic.ApplyDevicePropertyValueModels(null, devicePropertyValueModels));
            Assert.Throws<ArgumentNullException>(() => this._deviceLogic.ApplyDevicePropertyValueModels(device, null));
        }

        [Fact]
        public void ExtractDevicePropertyValuesModelsTest()
        {
            var now = DateTime.Now;

            var device = this.fixture.Create<DeviceModel>();
            device.Twin.Tags["x"] = "one";
            device.Twin.Properties.Desired["y"] = 2;
            device.Twin.Properties.Reported["z"] = now;

            this._configProviderMock.Setup(mock => mock.GetConfigurationSettingValue("iotHub.HostName")).Returns("hostName");
            var res = this._deviceLogic.ExtractDevicePropertyValuesModels(device);

            Assert.Equal(res.Count(), 5);
            Assert.Equal(res.Last().Name, "HostName");
            Assert.Equal(res.Last().Value, "hostName");

            var tagX = res.Single(m => m.Name == "tags.x");
            Assert.Equal(tagX.Value, "one");
            Assert.Equal(tagX.IsEditable, true);
            Assert.Equal(tagX.DisplayOrder, 1);

            var desiredY = res.Single(m => m.Name == "properties.desired.y");
            Assert.Equal(desiredY.Value, "2");
            Assert.Equal(desiredY.IsEditable, true);
            Assert.Equal(desiredY.DisplayOrder, 2);

            var reportedZ = res.Single(m => m.Name == "properties.reported.z");
            Assert.Equal(reportedZ.Value, now.ToString(CultureInfo.InvariantCulture));
            Assert.Equal(reportedZ.IsEditable, false);
            Assert.Equal(reportedZ.DisplayOrder, 3);

            Assert.Throws<ArgumentNullException>(() => this._deviceLogic.ExtractDevicePropertyValuesModels(null));
        }
    }
}
