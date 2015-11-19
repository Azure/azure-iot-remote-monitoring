using System.Linq;
using DeviceManagement.Infrastructure.Connectivity.UnitTests.Properties;
using DeviceManagement.Infrustructure.Connectivity.Exceptions;
using DeviceManagement.Infrustructure.Connectivity.Models;
using DeviceManagement.Infrustructure.Connectivity.Models.Jasper;
using DeviceManagement.Infrustructure.Connectivity.Models.Security;
using DeviceManagement.Infrustructure.Connectivity.Models.TerminalDevice;
using DeviceManagement.Infrustructure.Connectivity.Proxies;
using NUnit.Framework;

namespace DeviceManagement.Infrastructure.Connectivity.UnitTests.ProxyTests
{
    [TestFixture]
    public class JasperTerminalClientProxyTests
    {

        private ICredentials _creds;
        private IJasperTerminalClientProxy _proxy;
        private Iccid[] _validIccids;
        private Iccid[] _invalidIccids;

        private const int NUMBER_OF_TERMINALS = 30;


        [SetUp]
        public void SetUp()
        {
            _creds = new JasperCredentials()
            {
                Username = Resources.TestUsername,
                Password = Resources.TestPassword,
                LicenceKey = Resources.TestLicenceKey,
                BaseUrl = Resources.TestBaseUrl
            };

            _proxy = new JasperTerminalClientProxy(_creds);

            _validIccids = new[]
            {
                new Iccid(){Id = "8964050087210623943"}, 
                new Iccid(){Id = "8964050087210623612"},
                new Iccid(){Id = "8964050087210623935"} 
            };

            _invalidIccids = new[]
            {
                new Iccid(){Id = "89640500090000000"}, 
                new Iccid(){Id = "890000000000000000"},
                new Iccid(){Id = "89640500"}
            };

        }

        //[Test]
        //public void TestGetModifiedTerminalsFromTerminalProxy()
        //{
        //   var response =  _proxy.GetModifiedTerminals();
        //   Assert.IsNotNull(response);
        //   Assert.True(response.iccids.Length>0);
        //}

        //[Test]
        //public void TestGetSingleValidTerminalDetails()
        //{
        //    var response = _proxy.GetSingleTerminalDetails(_validIccids.First());       
        //    Assert.IsNotNull(response);
        //    Assert.IsTrue(response.terminals.Length == 1);
        //}

        //[Test]
        //public void TestGetSingleInvalidTerminalDetails()
        //{
        //   const string terminalNotFoundErrorCode = "100100";
        //   var ex = Assert.Throws<JasperConnectivityException>(
        //        () => _proxy.GetSingleTerminalDetails(_invalidIccids[0])); 
                
        // Assert.That(ex.Error.Code, Is.EqualTo(terminalNotFoundErrorCode));                                                  
        //}

        //[Test]
        //public void TestGetValidSingleSessionInfo()
        //{
        //    Assert.DoesNotThrow(() => _proxy.GetSingleSessionInfo(_validIccids[0]));
        //}

        //[Test]
        //public void TestGetInvalidSingleSessionInfo()
        //{
        //   const string terminalNotFoundErrorCode = "100100";
        //   var ex =  Assert.Throws<JasperConnectivityException>(() => _proxy.GetSingleSessionInfo(_invalidIccids[0]));
          
        //    Assert.That(ex.Error.Code, Is.EqualTo(terminalNotFoundErrorCode));
        //}
      
    }
}
