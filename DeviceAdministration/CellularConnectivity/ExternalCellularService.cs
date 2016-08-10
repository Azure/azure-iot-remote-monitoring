using System;
using System.Collections.Generic;
using DeviceManagement.Infrustructure.Connectivity.Clients;
using DeviceManagement.Infrustructure.Connectivity.Models.Enums;
using DeviceManagement.Infrustructure.Connectivity.Models.Security;
using DeviceManagement.Infrustructure.Connectivity.Models.TerminalDevice;

namespace DeviceManagement.Infrustructure.Connectivity
{
    public class ExternalCellularService : IExternalCellularService
    {
        private readonly ICredentialProvider _credentialProvider;

        public ExternalCellularService(ICredentialProvider credentialProvider)
        {
            _credentialProvider = credentialProvider;
        }

        public List<Iccid> GetTerminals()
        {
            List<Iccid> terminals;
            var registrationProvider = _credentialProvider.Provide().ApiRegistrationProvider;

            switch (registrationProvider)
            {
                case ApiRegistrationProviderType.Jasper:
                    terminals = new JasperCellularClient(_credentialProvider).GetTerminals();
                    break;
                case ApiRegistrationProviderType.Ericsson:
                    terminals = new EricssonCellularClient(_credentialProvider).GetTerminals();
                    break;
                default:
                    throw new IndexOutOfRangeException($"Could not find a service for '{registrationProvider.ToString()}' provider");
            }

            return terminals;
        }

        public Terminal GetSingleTerminalDetails(Iccid iccid)
        {
            Terminal terminal;
            var registrationProvider = _credentialProvider.Provide().ApiRegistrationProvider;

            switch (registrationProvider)
            {
                case ApiRegistrationProviderType.Jasper:
                    terminal = new JasperCellularClient(_credentialProvider).GetSingleTerminalDetails(iccid);
                    break;
                case ApiRegistrationProviderType.Ericsson:
                    terminal = new EricssonCellularClient(_credentialProvider).GetSingleTerminalDetails(iccid);
                    break;
                default:
                    throw new IndexOutOfRangeException($"Could not find a service for '{registrationProvider.ToString()}' provider");
            }

            return terminal;
        }

        public List<SessionInfo> GetSingleSessionInfo(Iccid iccid)
        {
            List<SessionInfo> sessionInfo = null;
            var registrationProvider = _credentialProvider.Provide().ApiRegistrationProvider;

            switch (registrationProvider)
            {
                case ApiRegistrationProviderType.Jasper:
                    sessionInfo = new JasperCellularClient(_credentialProvider).GetSingleSessionInfo(iccid);
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
        public bool ValidateCredentials()
        {
            bool isValid = false;
            var registrationProvider = _credentialProvider.Provide().ApiRegistrationProvider;

            switch (registrationProvider)
            {
                case ApiRegistrationProviderType.Jasper:
                    isValid = new JasperCellularClient(_credentialProvider).ValidateCredentials();
                    break;
                case ApiRegistrationProviderType.Ericsson:
                    isValid = new EricssonCellularClient(_credentialProvider).ValidateCredentials();
                    break;
                default:
                    throw new IndexOutOfRangeException($"Could not find a service for '{registrationProvider.ToString()}' provider");
            }

            return isValid;
        }
    }
}
