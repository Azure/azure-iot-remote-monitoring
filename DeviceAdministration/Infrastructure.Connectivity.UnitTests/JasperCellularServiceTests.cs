using System;
using System.Linq;
using DeviceManagement.Infrastructure.Connectivity.UnitTests.Properties;
using DeviceManagement.Infrustructure.Connectivity.Models;
using DeviceManagement.Infrustructure.Connectivity.Models.Jasper;
using DeviceManagement.Infrustructure.Connectivity.Models.Security;
using DeviceManagement.Infrustructure.Connectivity.Models.TerminalDevice;
using DeviceManagement.Infrustructure.Connectivity.Services;
using NUnit.Framework;

namespace DeviceManagement.Infrastructure.Connectivity.UnitTests
{
    public class JasperCellularServiceTests
    {
        
        private ICredentials creds;
        private Iccid[] _validIccids;
        private Iccid[] _invalidIccids;
        
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
        //public void TestGetModifiedTerminalsWithExceededTime()
        //{
        //    IExternalCellularService service = new JasperCellularService(creds, new TimeoutInvoker(0,0));
        //    var ex = Assert.Throws<TimeoutException>(
        //        () => service.GetTerminals());
        //}


        //[Test]
        //public void TestGetSingleTerminalsWithAdequateTime()
        //{
        //    IExternalCellularService service = new JasperCellularService(creds, new TimeoutInvoker(50000, 5000000));
        //    var response = service.GetSingleTerminalDetails(_validIccids.FirstOrDefault());
        //    Assert.NotNull(response);

        //}

     

    }
}
