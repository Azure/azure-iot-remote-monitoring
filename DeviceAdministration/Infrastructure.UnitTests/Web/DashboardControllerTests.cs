using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;
using Microsoft.Azure.Devices.Applications.RemoteMonitoring.Common.Configurations;
using Microsoft.Azure.Devices.Applications.RemoteMonitoring.Common.Models;
using Microsoft.Azure.Devices.Applications.RemoteMonitoring.DeviceAdmin.Infrastructure.BusinessLogic;
using Microsoft.Azure.Devices.Applications.RemoteMonitoring.DeviceAdmin.Infrastructure.Models;
using Microsoft.Azure.Devices.Applications.RemoteMonitoring.DeviceAdmin.Web.Controllers;
using Microsoft.Azure.Devices.Applications.RemoteMonitoring.DeviceAdmin.Web.Models;
using Moq;
using Ploeh.AutoFixture;
using Xunit;
using Enumerable = System.Linq.Enumerable;

namespace Microsoft.Azure.Devices.Applications.RemoteMonitoring.DeviceAdmin.Infrastructure.UnitTests.Web
{
    public class DashboardControllerTests
    {
        private readonly DashboardController dashboardController;
        private readonly Mock<IDeviceLogic> deviceLogicMock;
        private readonly Mock<IDeviceTelemetryLogic> deviceTelemetryMock;
        private readonly Mock<IConfigurationProvider> configMock;
        private readonly Fixture fixture;

        public DashboardControllerTests()
        {
            this.deviceLogicMock = new Mock<IDeviceLogic>();
            this.deviceTelemetryMock = new Mock<IDeviceTelemetryLogic>();
            this.configMock = new Mock<IConfigurationProvider>();
            this.dashboardController = new DashboardController(this.deviceLogicMock.Object, this.deviceTelemetryMock.Object, this.configMock.Object);
            this.fixture = new Fixture();
        }

        [Fact]
        public async void IndexTest()
        {
            var querRes = this.fixture.Create<DeviceListQueryResult>();
            var key = this.fixture.Create<string>();
            this.deviceLogicMock.Setup(mock => mock.GetDevices(It.IsAny<DeviceListQuery>())).ReturnsAsync(querRes);
            this.configMock.Setup(mock => mock.GetConfigurationSettingValue("MapApiQueryKey")).Returns(key);

            var result = await this.dashboardController.Index();
            var view = result as ViewResult;
            var model = view.Model as DashboardModel;
            Assert.Equal(model.DeviceIdsForDropdown.Count, querRes.Results.Count);
            KeyValuePair<string, string> deviceIDs = model.DeviceIdsForDropdown.First();
            string mockDeviceId = querRes.Results.First().DeviceProperties.DeviceID;
            Assert.Equal(deviceIDs.Key, mockDeviceId);
            Assert.Equal(deviceIDs.Value, mockDeviceId);
            Assert.Equal(model.MapApiQueryKey, key);

            this.deviceLogicMock.Setup(mock => mock.GetDevices(It.IsAny<DeviceListQuery>())).ReturnsAsync(null);
            this.configMock.Setup(mock => mock.GetConfigurationSettingValue("MapApiQueryKey")).Returns("0");
            result = await this.dashboardController.Index();
            view = result as ViewResult;
            model = view.Model as DashboardModel;
            Assert.Equal(model.DeviceIdsForDropdown.Count, 0);
            Assert.Equal(model.MapApiQueryKey, string.Empty);
        }
    }
}
