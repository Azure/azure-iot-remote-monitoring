using DeviceManagement.Infrustructure.Connectivity.Models.Constants;
using DeviceManagement.Infrustructure.Connectivity.Models.Jasper;
using Microsoft.Azure.Devices.Applications.RemoteMonitoring.Common.Models;
using Microsoft.Azure.Devices.Applications.RemoteMonitoring.DeviceAdmin.Infrastructure.Repository;
using Microsoft.Azure.Devices.Applications.RemoteMonitoring.DeviceAdmin.Web.Helpers;
using Moq;
using Ploeh.AutoFixture;
using Xunit;

namespace Microsoft.Azure.Devices.Applications.RemoteMonitoring.UnitTests.Web.Helpers
{
    public class JasperCredentialProviderTests
    {
        private readonly Mock<IApiRegistrationRepository> apiRegMock;
        private readonly IFixture fixture;
        private readonly JasperCredentialsProvider provider;

        public JasperCredentialProviderTests()
        {
            apiRegMock = new Mock<IApiRegistrationRepository>();
            provider = new JasperCredentialsProvider(apiRegMock.Object);
            fixture = new Fixture();
        }

        [Fact]
        public void ProvideTest()
        {
            var apiReg = fixture.Create<ApiRegistrationModel>();
            apiReg.ApiRegistrationProvider = ApiRegistrationProviderTypes.Jasper;
            apiRegMock.Setup(mock => mock.RecieveDetails()).Returns(apiReg);
            var result = provider.Provide();
            var res = result as JasperCredentials;
            Assert.Equal(res.BaseUrl, apiReg.BaseUrl);
            Assert.Equal(res.LicenceKey, apiReg.LicenceKey);
            Assert.Equal(res.Password, apiReg.Password);
            Assert.Equal(res.Username, apiReg.Username);
        }
    }
}