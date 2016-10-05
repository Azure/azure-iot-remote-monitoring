using System;
using DeviceManagement.Infrustructure.Connectivity.Builders;
using DeviceManagement.Infrustructure.Connectivity.com.jasper.api.sms;
using DeviceManagement.Infrustructure.Connectivity.com.jasperwireless.spark.terminal;
using DeviceManagement.Infrustructure.Connectivity.Constants;
using DeviceManagement.Infrustructure.Connectivity.Models.Security;
using DeviceManagement.Infrustructure.Connectivity.Models.TerminalDevice;

namespace DeviceManagement.Infrustructure.Connectivity.Proxies
{
    internal class JasperTerminalClientProxy : IJasperTerminalClientProxy
    {
        private readonly ICredentials _jasperCredentials;
        private readonly TerminalService _service;

        public JasperTerminalClientProxy(ICredentials jasperCredentials)
        {
            _jasperCredentials = jasperCredentials;
            _service = JasperServiceBuilder.GetTerminalService(_jasperCredentials);
        }

        public GetModifiedTerminalsResponse GetModifiedTerminals()
        {
            var request = new GetModifiedTerminalsRequest
            {
                licenseKey = _jasperCredentials.LicenceKey,
                version = JasperApiConstants.PROGRAM_VERSION,
                simStateSpecified = false,
                messageId = Guid.NewGuid() + "-" + JasperApiConstants.MESSAGE_ID,
                since = new DateTime(2013, 1, 1),
                sinceSpecified = true
            };
          
            return _service.GetModifiedTerminals(request);
        }

        public GetTerminalDetailsResponse GetSingleTerminalDetails(Iccid iccid)
        {
            Argument.CheckIfNull(iccid, "iccid");

            var request = new GetTerminalDetailsRequest
            {
                licenseKey = _jasperCredentials.LicenceKey,
                messageId = Guid.NewGuid() + "-" + JasperApiConstants.MESSAGE_ID,
                version = JasperApiConstants.PROGRAM_VERSION,
                iccids = new[] {iccid.Id}
            };

            return _service.GetTerminalDetails(request);
        }

        public GetSessionInfoResponse GetSingleSessionInfo(Iccid iccid)
        {
            Argument.CheckIfNull(iccid, "iccid");

            var request = new GetSessionInfoRequest
            {
                licenseKey = _jasperCredentials.LicenceKey,
                messageId = Guid.NewGuid() + "-" + JasperApiConstants.MESSAGE_ID,
                version = JasperApiConstants.PROGRAM_VERSION,
                iccid = new[] {iccid.Id}
            };

            return _service.GetSessionInfo(request);
        }
    }
}