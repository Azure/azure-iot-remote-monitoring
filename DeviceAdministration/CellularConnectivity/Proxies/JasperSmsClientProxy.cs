using System;
using DeviceManagement.Infrustructure.Connectivity.Builders;
using DeviceManagement.Infrustructure.Connectivity.com.jasper.api.sms;
using DeviceManagement.Infrustructure.Connectivity.com.jasperwireless.spark.terminal;
using DeviceManagement.Infrustructure.Connectivity.Constants;
using DeviceManagement.Infrustructure.Connectivity.Models.Security;
using DeviceManagement.Infrustructure.Connectivity.Models.TerminalDevice;

namespace DeviceManagement.Infrustructure.Connectivity.Proxies
{
    internal class JasperSmsClientProxy : IJasperSmsClientProxy
    {
        private readonly ICredentials _jasperCredentials;
        private readonly SmsService _service;

        public JasperSmsClientProxy(ICredentials jasperCredentials)
        {
            _jasperCredentials = jasperCredentials;
            _service = JasperServiceBuilder.GetSmsService(_jasperCredentials);
        }

        public SendSMSResponse SendSms(string iccid, string messageText)
        {
            Argument.CheckIfNull(iccid, "iccid");
            Argument.CheckIfNull(messageText, "messageText");

            var request = new SendSMSRequest()
            {
                sentToIccid = iccid,
                messageId = Guid.NewGuid() + "-" + "0",
                version = JasperApiConstants.PROGRAM_VERSION,
                messageText = "hello",
                licenseKey = _jasperCredentials.LicenceKey,
            };
            return _service.SendSMS(request);
        }

    }
}