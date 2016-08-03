using DeviceManagement.Infrustructure.Connectivity.Models.Jasper;
using Microsoft.Azure.Devices.Applications.RemoteMonitoring.Common.Models;
using Microsoft.Azure.Devices.Applications.RemoteMonitoring.DeviceAdmin.Infrastructure.Repository;
using Microsoft.Azure.Devices.Applications.RemoteMonitoring.DeviceAdmin.Web.Helpers;
using Moq;
using Ploeh.AutoFixture;
using Xunit;

namespace Microsoft.Azure.Devices.Applications.RemoteMonitoring.DeviceAdmin.Infrastructure.UnitTests.Web.Helpers
{
    public class JasperCredentialProviderTests
    {
        private JasperCredentialsProvider provider;
        private readonly Mock<IApiRegistrationRepository> apiRegMock;
        private readonly IFixture fixture;

        public JasperCredentialProviderTests()
        {
            this.apiRegMock = new Mock<IApiRegistrationRepository>();
            this.provider = new JasperCredentialsProvider(this.apiRegMock.Object);
            this.fixture = new Fixture();
        }

        [Fact]
        public void ProvideTest()
        {
            var apiReg = this.fixture.Create<ApiRegistrationModel>();
            this.apiRegMock.Setup(mock => mock.RecieveDetails()).Returns(apiReg);
            var result = this.provider.Provide();
            var res = result as JasperCredentials;
            Assert.Equal(res.BaseUrl, apiReg.BaseUrl);
            Assert.Equal(res.LicenceKey, apiReg.LicenceKey);
            Assert.Equal(res.Password, apiReg.Password);
            Assert.Equal(res.Username, apiReg.Username);
        }
    }
}
