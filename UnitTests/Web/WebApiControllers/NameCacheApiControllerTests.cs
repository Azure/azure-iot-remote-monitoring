using Microsoft.Azure.Devices.Applications.RemoteMonitoring.DeviceAdmin.Infrastructure.BusinessLogic;
using Microsoft.Azure.Devices.Applications.RemoteMonitoring.DeviceAdmin.Infrastructure.Models;
using Microsoft.Azure.Devices.Applications.RemoteMonitoring.DeviceAdmin.Web.WebApiControllers;
using Moq;
using Xunit;

namespace Microsoft.Azure.Devices.Applications.RemoteMonitoring.UnitTests.Web.
    WebApiControllers
{
    public class NameCacheApiControllerTests
    {
        private readonly NameCacheApiController controller;

        public NameCacheApiControllerTests()
        {
            var logic = new Mock<INameCacheLogic>();
            controller = new NameCacheApiController(logic.Object);
            controller.InitializeRequest();
        }

        [Fact]
        public async void GetDeviceTwinAndMethodTest()
        {
            var result = await controller.GetNameList(NameCacheEntityType.All);
            result.AssertOnError();
        }
    }
}