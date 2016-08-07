using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DeviceManagement.Infrustructure.Connectivity.Models.TerminalDevice;
using DeviceManagement.Infrustructure.Connectivity.Models.Security;
using DeviceManagement.Infrustructure.Connectivity.com.jasperwireless.spark.terminal;
using DeviceManagement.Infrustructure.Connectivity.Exceptions;
using DeviceManagement.Infrustructure.Connectivity.Proxies;
using DeviceManagement.Infrustructure.Connectivity.Models.Enums;

namespace DeviceManagement.Infrustructure.Connectivity.Services
{
    public class ExternalCellularService : IExternalCellularService
    {
        private readonly ICredentialProvider _credentialProvider;
        private readonly IJasperCellularService _jasperCellularService;

        public ExternalCellularService(
            IJasperCellularService jasperCellularService,
            ICredentialProvider credentialProvider)
        {
            _credentialProvider = credentialProvider;
        }

        public List<Iccid> GetTerminals(ApiRegistrationProviderType registrationProvider)
        {
            List<Iccid> terminals = new List<Iccid>();

            switch (registrationProvider)
            {
                case ApiRegistrationProviderType.Jasper:
                    terminals = _jasperCellularService.GetTerminals();
                    break;
                case Models.Enums.ApiRegistrationProviderType.Ericsson:
                    //TODO call ericsson service

                    break;
                default:
                    throw new IndexOutOfRangeException($"Could not find a service for '{registrationProvider.ToString()}' provider");
            }

            return terminals;
        }

        public Terminal GetSingleTerminalDetails(Iccid iccid, ApiRegistrationProviderType registrationProvider)
        {
            Terminal terminal = null;

            switch (registrationProvider)
            {
                case ApiRegistrationProviderType.Jasper:
                    terminal = _jasperCellularService.GetSingleTerminalDetails(iccid);
                    break;
                case ApiRegistrationProviderType.Ericsson:
                    //TODO call ericsson service
                    break;
                default:
                    throw new IndexOutOfRangeException($"Could not find a service for '{registrationProvider.ToString()}' provider");
            }

            return terminal;
        }

        public List<SessionInfo> GetSingleSessionInfo(Iccid iccid, ApiRegistrationProviderType registrationProvider)
        {
            List<SessionInfo> sessionInfo = null;

            switch (registrationProvider)
            {
                case ApiRegistrationProviderType.Jasper:
                    sessionInfo = _jasperCellularService.GetSingleSessionInfo(iccid);
                    break;
                case ApiRegistrationProviderType.Ericsson:
                    //TODO call ericsson service
                    break;
                default:
                    throw new IndexOutOfRangeException($"Could not find a service for '{registrationProvider.ToString()}' provider");
            }

            return sessionInfo;
        }

        /// <summary>
        /// The API does not have a way to validate credentials so this method calls
        /// GetTerminals() and checks the response for validation errors.
        /// </summary>
        /// <returns>True if valid. False if not valid</returns>
        public bool ValidateCredentials(ApiRegistrationProviderType registrationProvider)
        {
            bool isValid = false;

            switch (registrationProvider)
            {
                case ApiRegistrationProviderType.Jasper:
                    isValid = _jasperCellularService.ValidateCredentials();
                    break;
                case ApiRegistrationProviderType.Ericsson:
                    //TODO call ericsson service
                    break;
                default:
                    throw new IndexOutOfRangeException($"Could not find a service for '{registrationProvider.ToString()}' provider");
            }

            return isValid;
        }
    }
}
