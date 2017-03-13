using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Azure.Devices.Applications.RemoteMonitoring.DeviceAdmin.Infrastructure.BusinessLogic;
using Microsoft.Azure.Devices.Applications.RemoteMonitoring.DeviceAdmin.Infrastructure.Models;
using Microsoft.Azure.Devices.Applications.RemoteMonitoring.DeviceAdmin.Web.WebApiControllers;
using Moq;
using Ploeh.AutoFixture;
using Xunit;

namespace Microsoft.Azure.Devices.Applications.RemoteMonitoring.UnitTests.Web.
    WebApiControllers
{
    public class DeviceListColumnsApiControllerTests : IDisposable
    {
        private readonly Mock<IUserSettingsLogic> userSettingsLogic;
        private readonly DeviceListColumnsApiController deviceListColumnsApiController;
        private readonly IFixture fixture;

        public DeviceListColumnsApiControllerTests()
        {
            userSettingsLogic = new Mock<IUserSettingsLogic>();
            deviceListColumnsApiController = new DeviceListColumnsApiController(userSettingsLogic.Object);
            deviceListColumnsApiController.InitializeRequest();
            fixture = new Fixture();
        }

        [Fact]
        public async void GetDeviceListColumnsTest()
        {
            var columns = fixture.Create<IEnumerable<DeviceListColumns>>();
            userSettingsLogic.Setup(mock => mock.GetDeviceListColumnsAsync(It.IsAny<string>())).ReturnsAsync(columns);
            var result = await deviceListColumnsApiController.GetDeviceListColumns();
            result.AssertOnError();
            var data = result.ExtractContentDataAs<IEnumerable<DeviceListColumns>>();
            Assert.NotNull(data);
            Assert.Equal(data.Count(), columns.Count());
            Assert.Equal(data.First().Name, columns.First().Name);

            userSettingsLogic.Setup(mock => mock.GetDeviceListColumnsAsync(It.IsAny<string>())).ReturnsAsync(null);
            result = await deviceListColumnsApiController.GetDeviceListColumns();
            result.AssertOnError();
            data = result.ExtractContentDataAs<IEnumerable<DeviceListColumns>>();
            Assert.NotNull(data);
            Assert.True(data.Any(c => c.Name.StartsWith("reported")));
        }

        [Fact]
        public async void GetGlobalDeviceListColumnsTest()
        {
            var columns = fixture.Create<IEnumerable<DeviceListColumns>>();
            userSettingsLogic.Setup(mock => mock.GetGlobalDeviceListColumnsAsync()).ReturnsAsync(columns);
            var result = await deviceListColumnsApiController.GetGlobalDeviceListColumns();
            result.AssertOnError();
            var data = result.ExtractContentDataAs<IEnumerable<DeviceListColumns>>();
            Assert.NotNull(data);
            Assert.Equal(data.Count(), columns.Count());
            Assert.Equal(data.First().Name, columns.First().Name);
        }

        [Fact]
        public async void UpdateDeviceListColumnsTest()
        {
            var columns = fixture.Create<IEnumerable<DeviceListColumns>>();
            userSettingsLogic.Setup(mock => mock.SetDeviceListColumnsAsync(It.IsAny<string>(), columns, true)).ReturnsAsync(true);
            var res = await deviceListColumnsApiController.UpdateDeviceListColumns(columns, true);
            res.AssertOnError();
            var data = res.ExtractContentDataAs<bool>();
            Assert.Equal(data, true);
        }

        #region IDisposable Support
        private bool disposedValue = false; // To detect redundant calls

        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                    deviceListColumnsApiController.Dispose();
                }

                // TODO: free unmanaged resources (unmanaged objects) and override a finalizer below.
                // TODO: set large fields to null.

                disposedValue = true;
            }
        }

        // TODO: override a finalizer only if Dispose(bool disposing) above has code to free unmanaged resources.
        // ~DeviceListColumnsApiControllerTests() {
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