using System;
using DeviceManagement.Infrustructure.Connectivity.Builders;
using DeviceManagement.Infrustructure.Connectivity.com.jasperwireless.spark.billing;
using DeviceManagement.Infrustructure.Connectivity.Constants;
using DeviceManagement.Infrustructure.Connectivity.Models.Security;
using DeviceManagement.Infrustructure.Connectivity.Models.TerminalDevice;

namespace DeviceManagement.Infrustructure.Connectivity.Proxies
{
    public class JasperBillingClientProxy : IJasperBillingClientProxy
    {
        private readonly ICredentials _jasperCredentials;
        private readonly BillingService _service;

        public JasperBillingClientProxy(ICredentials jasperCredentials)
        {
            _jasperCredentials = jasperCredentials;
            _service = JasperServiceBuilder.GetBillingService(_jasperCredentials);
        }

        public GetTerminalUsageDataDetailsResponse GetTerminalUsageDataDetails(Iccid iccid, DateTime cycleStartDate)
        {
            var request = new GetTerminalUsageDataDetailsRequest
            {
                licenseKey = _jasperCredentials.LicenceKey,
                messageId = Guid.NewGuid() + "-" + JasperApiConstants.MESSAGE_ID,
                version = JasperApiConstants.PROGRAM_VERSION,
                iccid = iccid.Id,
                cycleStartDate = cycleStartDate
            };


            
            return _service.GetTerminalUsageDataDetails(request);
        }

        public GetTerminalUsageSmsDetailsResponse GetTerminalUsageSmsDetails(Iccid iccid, DateTime cycleStartDate)
        {
            var request = new GetTerminalUsageSmsDetailsRequest
            {
                licenseKey = _jasperCredentials.LicenceKey,
                messageId = Guid.NewGuid() + "-" + JasperApiConstants.MESSAGE_ID,
                version = JasperApiConstants.PROGRAM_VERSION,
                iccid = iccid.Id,
                cycleStartDate = cycleStartDate
            };

            return _service.GetTerminalUsageSmsDetails(request);
        }

        public GetTerminalUsageVoiceDetailsResponse GetTerminalUsageVoiceDetails(Iccid iccid, DateTime cycleStartDate)
        {
            var request = new GetTerminalUsageVoiceDetailsRequest
            {
                licenseKey = _jasperCredentials.LicenceKey,
                messageId = Guid.NewGuid() + "-" + JasperApiConstants.MESSAGE_ID,
                version = JasperApiConstants.PROGRAM_VERSION,
                iccid = iccid.Id,
                cycleStartDate = cycleStartDate
            };

            return _service.GetTerminalUsageVoiceDetails(request);
        }

        public GetTerminalUsageResponse GetTerminalUsage(Iccid iccid, DateTime cycleStartDate)
        {
            var request = new GetTerminalUsageRequest
            {
                licenseKey = _jasperCredentials.LicenceKey,
                messageId = Guid.NewGuid() + "-" + JasperApiConstants.MESSAGE_ID,
                version = JasperApiConstants.PROGRAM_VERSION,
                iccid = iccid.Id,
                cycleStartDate = cycleStartDate
            };

            return _service.GetTerminalUsage(request);
        }
    }
}