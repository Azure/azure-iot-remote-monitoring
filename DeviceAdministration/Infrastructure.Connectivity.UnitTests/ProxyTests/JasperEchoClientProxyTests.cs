using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DeviceManagement.Infrastructure.Connectivity.UnitTests.Properties;
using DeviceManagement.Infrustructure.Connectivity.Models.Jasper;
using DeviceManagement.Infrustructure.Connectivity.Models.Security;
using DeviceManagement.Infrustructure.Connectivity.Proxies;
using NUnit.Framework;

namespace DeviceManagement.Infrastructure.Connectivity.UnitTests.ProxyTests
{
    [TestFixture]
    public class JasperEchoClientProxyTests
    {
        private ICredentials creds;
        private IAuthenticationValidationProxy proxy;

        [SetUp]
        public void SetUp()
        {
            creds = new JasperCredentials()
            {
                Username = Resources.TestUsername,
                Password = Resources.TestPassword,
                LicenceKey = Resources.TestLicenceKey,
                BaseUrl = Resources.TestBaseUrl
            };

            proxy = new AuthenticationValidationProxy(creds);
        }


        [Test]
        public void TestCredentials()
        {
            var x = proxy.ValidateCredentials();
        }

    }

}


