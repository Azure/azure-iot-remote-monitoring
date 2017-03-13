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
using System.Web;
using System.Web.Routing;
using Microsoft.Azure.Devices.Applications.RemoteMonitoring.DeviceAdmin.Web.Helpers;
using System.Threading;
using System;

namespace Microsoft.Azure.Devices.Applications.RemoteMonitoring.UnitTests.Web
{
    public class DashboardControllerTests : IDisposable
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
            var filterMock = fixture.Create<DeviceListFilterResult>();
            var key = fixture.Create<string>();
            var cookiecollection = new HttpCookieCollection();
            var request = new Mock<HttpRequestBase>();
            request.SetupGet(req => req.Cookies).Returns(new HttpCookieCollection());
            var response = new Mock<HttpResponseBase>();
            response.SetupGet(res => res.Cookies).Returns(cookiecollection);
            var context = new Mock<HttpContextBase>();
            context.SetupGet(x => x.Request).Returns(request.Object);
            context.SetupGet(x => x.Response).Returns(response.Object);

            _deviceLogicMock.Setup(mock => mock.GetDevices(It.IsAny<DeviceListFilter>())).ReturnsAsync(filterMock);
            _configurationMock.Setup(mock => mock.GetConfigurationSettingValue("MapApiQueryKey")).Returns(key);
            _dashboardController.ControllerContext = new ControllerContext(context.Object, new RouteData(), _dashboardController);

            var result = await _dashboardController.Index();
            var view = result as ViewResult;
            var model = view.Model as DashboardModel;

            string expectedcookievalue = CultureHelper.GetClosestCulture(Thread.CurrentThread.CurrentCulture.Name).Name;

            Assert.Equal(_dashboardController.Response.Cookies.Count, 1);
            Assert.Equal(_dashboardController.Response.Cookies["_culture"].Value, expectedcookievalue);
            Assert.Equal(model.DeviceIdsForDropdown.Count, filterMock.Results.Count);
            var deviceIDs = model.DeviceIdsForDropdown.First();
            var mockDeviceId = filterMock.Results.First().DeviceProperties.DeviceID;
            Assert.Equal(deviceIDs.Key, mockDeviceId);
            Assert.Equal(deviceIDs.Value, mockDeviceId);
            Assert.Equal(model.MapApiQueryKey, key);

            _deviceLogicMock.Setup(mock => mock.GetDevices(It.IsAny<DeviceListFilter>())).ReturnsAsync(null);
            _configurationMock.Setup(mock => mock.GetConfigurationSettingValue("MapApiQueryKey")).Returns("0");
            result = await _dashboardController.Index();
            view = result as ViewResult;
            model = view.Model as DashboardModel;
            Assert.Equal(model.DeviceIdsForDropdown.Count, 0);
            Assert.Equal(model.MapApiQueryKey, string.Empty);
        }

        #region IDisposable Support
        private bool disposedValue = false; // To detect redundant calls

        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                    _dashboardController.Dispose();
                }

                // TODO: free unmanaged resources (unmanaged objects) and override a finalizer below.
                // TODO: set large fields to null.

                disposedValue = true;
            }
        }

        // TODO: override a finalizer only if Dispose(bool disposing) above has code to free unmanaged resources.
        // ~DashboardControllerTests() {
        //   // Do not change this code. Put cleanup code in Dispose(bool disposing) above.
        //   Dispose(false);
        // }

        // This code added to correctly implement the disposable pattern.
        public void Dispose()
        {
            // Do not change this code. Put cleanup code in Dispose(bool disposing) above.
            Dispose(true);
            // TODO: uncomment the following line if the finalizer is overridden above.
            // GC.SuppressFinalize(this);
        }
        #endregion
    }
}
