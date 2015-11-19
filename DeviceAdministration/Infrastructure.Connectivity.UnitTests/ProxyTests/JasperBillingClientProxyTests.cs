using DeviceManagement.Infrastructure.Connectivity.UnitTests.Properties;
using DeviceManagement.Infrustructure.Connectivity.Models;
using DeviceManagement.Infrustructure.Connectivity.Models.Jasper;
using DeviceManagement.Infrustructure.Connectivity.Models.Security;
using DeviceManagement.Infrustructure.Connectivity.Proxies;
using NUnit.Framework;

namespace DeviceManagement.Infrastructure.Connectivity.UnitTests.ProxyTests
{
    [TestFixture]
    public class JasperBillingClientProxyTests
    {
        private ICredentials creds;
        private IJasperBillingClientProxy proxy;

        private string[] validIccids;
        private string[] invalidIccids;

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

            proxy = new JasperBillingClientProxy(creds);

            validIccids = new[]
            {
                "8964050087210623943", "8964050087210623612",
                "8964050087210623935"
            };

            invalidIccids = new[]
            {
                "89640500090000000", "890000000000000000",
                "89640500"
            };
           
        }

        //[Test]
        //public void TestGetTerminalUsage()
        //{
        //    var x = proxy.GetTerminalUsage(validIccids[0], DateTime.Now.AddDays(-1));
        //    Assert.IsNotNull(x);
        //}

        //[Test]
        //public void TestGetTerminalUsageDataDetails()
        //{
        //    var x = proxy.GetTerminalUsageDataDetails(validIccids[0], DateTime.Now.AddDays(-1));
        //    Assert.IsNotNull(x);
        //}

        //[Test]
        //public void TestGetTerminalUsageVoiceDetails()
        //{
        //    var x = proxy.GetTerminalUsageVoiceDetails(validIccids[0], DateTime.Now.AddDays(-1));
        //    Assert.IsNotNull(x);
        //}

        //[Test]
        //public void TestGetTerminalUsageSmsDetails()
        //{
        //    var x = proxy.GetTerminalUsageSmsDetails(validIccids[0], DateTime.Now.AddDays(-1));
        //    Assert.IsNotNull(x);
        //}

    }
}
