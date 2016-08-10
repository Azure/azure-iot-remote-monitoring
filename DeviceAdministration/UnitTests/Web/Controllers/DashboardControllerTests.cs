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

namespace Microsoft.Azure.Devices.Applications.RemoteMonitoring.UnitTests.Web
{
    public class DashboardControllerTests
    {
        private readonly Mock<IConfigurationProvider> _configurationMock;
        private readonly DashboardController _dashboardController;
        private readonly Mock<IDeviceLogic> _deviceLogicMock;
        private readonly Fixture fixture;

        public DashboardControllerTests()
        {
            _deviceLogicMock = new Mock<IDeviceLogic>();
            var deviceTelemetryMock = new Mock<IDeviceTelemetryLogic>();
            _configurationMock = new Mock<IConfigurationProvider>();
            _dashboardController = new DashboardController(
                _deviceLogicMock.Object, 
                deviceTelemetryMock.Object, 
                _configurationMock.Object);

            fixture = new Fixture();
        }

        [Fact]
        public async void IndexTest()
        {
            var querRes = fixture.Create<DeviceListQueryResult>();
            var key = fixture.Create<string>();
            _deviceLogicMock.Setup(mock => mock.GetDevices(It.IsAny<DeviceListQuery>())).ReturnsAsync(querRes);
            _configurationMock.Setup(mock => mock.GetConfigurationSettingValue("MapApiQueryKey")).Returns(key);

            var result = await _dashboardController.Index();
            var view = result as ViewResult;
            var model = view.Model as DashboardModel;

            Assert.Equal(model.DeviceIdsForDropdown.Count, querRes.Results.Count);
            var deviceIDs = model.DeviceIdsForDropdown.First();
            var mockDeviceId = querRes.Results.First().DeviceProperties.DeviceID;
            Assert.Equal(deviceIDs.Key, mockDeviceId);
            Assert.Equal(deviceIDs.Value, mockDeviceId);
            Assert.Equal(model.MapApiQueryKey, key);

            _deviceLogicMock.Setup(mock => mock.GetDevices(It.IsAny<DeviceListQuery>())).ReturnsAsync(null);
            _configurationMock.Setup(mock => mock.GetConfigurationSettingValue("MapApiQueryKey")).Returns("0");
            result = await _dashboardController.Index();
            view = result as ViewResult;
            model = view.Model as DashboardModel;
            Assert.Equal(model.DeviceIdsForDropdown.Count, 0);
            Assert.Equal(model.MapApiQueryKey, string.Empty);
        }
    }
}
