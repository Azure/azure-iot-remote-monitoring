using System.Linq;
using System.Web.Mvc;
using Microsoft.Azure.Devices.Applications.RemoteMonitoring.Common.Configurations;
using Microsoft.Azure.Devices.Applications.RemoteMonitoring.DeviceAdmin.Infrastructure.BusinessLogic;
using Microsoft.Azure.Devices.Applications.RemoteMonitoring.DeviceAdmin.Infrastructure.Models;
using Microsoft.Azure.Devices.Applications.RemoteMonitoring.DeviceAdmin.Web.Controllers;
using Microsoft.Azure.Devices.Applications.RemoteMonitoring.DeviceAdmin.Web.Models;
using Moq;
using Ploeh.AutoFixture;
using Xunit;

namespace Microsoft.Azure.Devices.Applications.RemoteMonitoring.DeviceAdmin.Infrastructure.UnitTests.Web
{
    public class DashboardControllerTests
    {
        private readonly Mock<IConfigurationProvider> configMock;
        private readonly DashboardController dashboardController;
        private readonly Mock<IDeviceLogic> deviceLogicMock;
        private readonly Mock<IDeviceTelemetryLogic> deviceTelemetryMock;
        private readonly Fixture fixture;

        public DashboardControllerTests()
        {
            deviceLogicMock = new Mock<IDeviceLogic>();
            deviceTelemetryMock = new Mock<IDeviceTelemetryLogic>();
            configMock = new Mock<IConfigurationProvider>();
            dashboardController = new DashboardController(deviceLogicMock.Object, deviceTelemetryMock.Object,
                configMock.Object);
            fixture = new Fixture();
        }

        [Fact]
        public async void IndexTest()
        {
            var querRes = fixture.Create<DeviceListQueryResult>();
            var key = fixture.Create<string>();
            deviceLogicMock.Setup(mock => mock.GetDevices(It.IsAny<DeviceListQuery>())).ReturnsAsync(querRes);
            configMock.Setup(mock => mock.GetConfigurationSettingValue("MapApiQueryKey")).Returns(key);

            var result = await dashboardController.Index();
            var view = result as ViewResult;
            var model = view.Model as DashboardModel;
            Assert.Equal(model.DeviceIdsForDropdown.Count, querRes.Results.Count);
            var deviceIDs = model.DeviceIdsForDropdown.First();
            var mockDeviceId = querRes.Results.First().DeviceProperties.DeviceID;
            Assert.Equal(deviceIDs.Key, mockDeviceId);
            Assert.Equal(deviceIDs.Value, mockDeviceId);
            Assert.Equal(model.MapApiQueryKey, key);

            deviceLogicMock.Setup(mock => mock.GetDevices(It.IsAny<DeviceListQuery>())).ReturnsAsync(null);
            configMock.Setup(mock => mock.GetConfigurationSettingValue("MapApiQueryKey")).Returns("0");
            result = await dashboardController.Index();
            view = result as ViewResult;
            model = view.Model as DashboardModel;
            Assert.Equal(model.DeviceIdsForDropdown.Count, 0);
            Assert.Equal(model.MapApiQueryKey, string.Empty);
        }
    }
}