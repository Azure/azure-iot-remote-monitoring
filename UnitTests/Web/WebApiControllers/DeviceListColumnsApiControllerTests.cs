using System.Collections.Generic;
using System.Linq;
using Microsoft.Azure.Devices.Applications.RemoteMonitoring.DeviceAdmin.Infrastructure.Models;
using Microsoft.Azure.Devices.Applications.RemoteMonitoring.DeviceAdmin.Infrastructure.Repository;
using Microsoft.Azure.Devices.Applications.RemoteMonitoring.DeviceAdmin.Web.WebApiControllers;
using Moq;
using Ploeh.AutoFixture;
using Xunit;

namespace Microsoft.Azure.Devices.Applications.RemoteMonitoring.UnitTests.Web.
    WebApiControllers
{
    public class DeviceListColumnsApiControllerTests
    {
        private readonly Mock<IDeviceListColumnsRepository> deviceListColumnsRepository;
        private readonly DeviceListColumnsApiController deviceListColumnsApiController;
        private readonly IFixture fixture;

        public DeviceListColumnsApiControllerTests()
        {
            deviceListColumnsRepository = new Mock<IDeviceListColumnsRepository>();
            deviceListColumnsApiController = new DeviceListColumnsApiController(deviceListColumnsRepository.Object);
            deviceListColumnsApiController.InitializeRequest();
            fixture = new Fixture();
        }

        [Fact]
        public async void GetDeviceListColumnsTest()
        {
            var columns = fixture.Create<IEnumerable<DeviceListColumns>>();
            deviceListColumnsRepository.Setup(mock => mock.GetAsync(It.IsAny<string>())).ReturnsAsync(columns);
            var result = await deviceListColumnsApiController.GetDeviceListColumns();
            result.AssertOnError();
            var data = result.ExtractContentDataAs<IEnumerable<DeviceListColumns>>();
            Assert.NotNull(data);
            Assert.Equal(data.Count(), columns.Count());
            Assert.Equal(data.First().Name, columns.First().Name);

            deviceListColumnsRepository.Setup(mock => mock.GetAsync(It.IsAny<string>())).ReturnsAsync(null);
            result = await deviceListColumnsApiController.GetDeviceListColumns();
            result.AssertOnError();
            data = result.ExtractContentDataAs<IEnumerable<DeviceListColumns>>();
            Assert.NotNull(data);
            Assert.True(data.Any(c => c.Name.StartsWith("reported")));
        }

        [Fact]
        public async void UpdateDeviceListColumnsTest()
        {
            var columns = fixture.Create<IEnumerable<DeviceListColumns>>();
            deviceListColumnsRepository.Setup(mock => mock.SaveAsync(It.IsAny<string>(), columns)).ReturnsAsync(true);
            var res = await deviceListColumnsApiController.UpdateDeviceListColumns(columns);
            res.AssertOnError();
            var data = res.ExtractContentDataAs<bool>();
            Assert.Equal(data, true);
        }
    }
}