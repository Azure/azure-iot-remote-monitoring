using Microsoft.Azure.Devices.Applications.RemoteMonitoring.Common.Models;
using Microsoft.Azure.Devices.Applications.RemoteMonitoring.DeviceAdmin.Infrastructure.BusinessLogic;
using Microsoft.Azure.Devices.Applications.RemoteMonitoring.DeviceAdmin.Web.WebApiControllers;
using Xunit;

namespace Microsoft.Azure.Devices.Applications.RemoteMonitoring.DeviceAdmin.Infrastructure.UnitTests.Web.WebApiControllers
{
    public class KeyApiControllerTests
    {
        private readonly KeyApiController keyApiController;
        private readonly IKeyLogic keyLogic;
        private readonly ISecurityKeyGenerator securityKeyGenerator;

        public KeyApiControllerTests()
        {
            this.securityKeyGenerator = new SecurityKeyGenerator();
            this.keyLogic = new KeyLogic(this.securityKeyGenerator);
            this.keyApiController = new KeyApiController(this.keyLogic);
            this.keyApiController.InitializeRequest();
        }

        [Fact]
        public async void GetKeysAsyncTest()
        {
            var res = await this.keyApiController.GetKeysAsync();
            res.AssertOnError();
            var data = res.ExtractContentDataAs<SecurityKeys>();
            Assert.NotNull(data);
            Assert.NotNull(data.PrimaryKey);
            Assert.NotNull(data.SecondaryKey);
        }
    }
}
