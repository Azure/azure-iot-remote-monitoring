using System;
using DeviceManagement.Infrustructure.Connectivity.Builders;
using DeviceManagement.Infrustructure.Connectivity.com.jasperwireless.spark.echo;
using DeviceManagement.Infrustructure.Connectivity.Constants;
using DeviceManagement.Infrustructure.Connectivity.Models.Security;

namespace DeviceManagement.Infrustructure.Connectivity.Proxies
{
    internal class AuthenticationValidationProxy : IAuthenticationValidationProxy
    {
        private readonly ICredentials _jasperCredentials;
        private readonly EchoService _service;

        public AuthenticationValidationProxy(ICredentials jasperCredentials)
        {
            _jasperCredentials = jasperCredentials;
            _service = JasperServiceBuilder.GetEchoService(_jasperCredentials);
        }

        public EchoResponse ValidateCredentials()
        {
            var request = new EchoRequest
            {
                licenseKey = _jasperCredentials.LicenceKey,
                messageId = Guid.NewGuid() + "-" + JasperApiConstants.MESSAGE_ID,
                version = JasperApiConstants.PROGRAM_VERSION,
                value = ""
            };

            return _service.Echo(request);
        }
    }
}