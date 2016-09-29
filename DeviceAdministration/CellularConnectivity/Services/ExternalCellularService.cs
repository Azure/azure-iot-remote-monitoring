using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using DeviceManagement.Infrustructure.Connectivity.Clients;
using DeviceManagement.Infrustructure.Connectivity.Models.Constants;
using DeviceManagement.Infrustructure.Connectivity.Models.Other;
using DeviceManagement.Infrustructure.Connectivity.Models.Security;
using DeviceManagement.Infrustructure.Connectivity.Models.TerminalDevice;

namespace DeviceManagement.Infrustructure.Connectivity.Services
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
                case ApiRegistrationProviderTypes.Jasper:
                    terminals = new JasperCellularClient(_credentialProvider).GetTerminals();
                    break;
                case ApiRegistrationProviderTypes.Ericsson:
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
                case ApiRegistrationProviderTypes.Jasper:
                    terminal = new JasperCellularClient(_credentialProvider).GetSingleTerminalDetails(iccid);
                    break;
                case ApiRegistrationProviderTypes.Ericsson:
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
                case ApiRegistrationProviderTypes.Jasper:
                    sessionInfo = new JasperCellularClient(_credentialProvider).GetSingleSessionInfo(iccid);
                    break;
                case ApiRegistrationProviderTypes.Ericsson:
                    sessionInfo = new EricssonCellularClient(_credentialProvider).GetSingleSessionInfo(iccid);
                    break;
                default:
                    throw new IndexOutOfRangeException($"Could not find a service for '{registrationProvider.ToString()}' provider");
            }

            return sessionInfo;
        }

        public SimState GetCurrentSimState(string iccid)
        {
            return GetAvailableSimStates(iccid).FirstOrDefault(s => s.Name == "Active");
        }

        public List<SimState> GetAvailableSimStates(string iccid)
        {
            List<SimState> availableStates;
            var registrationProvider = _credentialProvider.Provide().ApiRegistrationProvider;

            switch (registrationProvider)
            {
                case ApiRegistrationProviderTypes.Jasper:
                    availableStates = getAvailableSimStates();
                    break;
                case ApiRegistrationProviderTypes.Ericsson:
                    availableStates = getAvailableSimStates();
                    break;
                default:
                    throw new IndexOutOfRangeException($"Could not find a service for '{registrationProvider}' provider");
            }
            return availableStates;
        }

        public List<SubscriptionPackage> GetAvailableSubscriptionPackages(string iccid)
        {
            List<SubscriptionPackage> availableSubscriptionPackages;
            var registrationProvider = _credentialProvider.Provide().ApiRegistrationProvider;

            switch (registrationProvider)
            {
                case ApiRegistrationProviderTypes.Jasper:
                    availableSubscriptionPackages = getAvailableSubscriptionPackages();
                    break;
                case ApiRegistrationProviderTypes.Ericsson:
                    availableSubscriptionPackages = getAvailableSubscriptionPackages();
                    break;
                default:
                    throw new IndexOutOfRangeException($"Could not find a service for '{registrationProvider}' provider");
            }
            return availableSubscriptionPackages;
        }

        public async Task<bool> UpdateSimState(string iccid, string updatedState)
        {
            return true;
        }

        public async Task<bool> UpdateSubscriptionPackage(string iccid, string updatedPackage)
        {
            return true;
        }

        public async Task<bool> ReconnectTerminal(string iccid)
        {
            return true;
        }

        public async Task<bool> SendSms(string iccid, string smsText)
        {
            return true;
        }


        public SubscriptionPackage GetCurrentSubscriptionPackage(string iccid)
        {
            return GetAvailableSubscriptionPackages(iccid).FirstOrDefault(s => s.Name == "Basic");
        }

        /// <summary>
        /// The API does not have a way to validate credentials so this method calls
        /// GetTerminals() and checks the response for validation errors.
        /// </summary>
        /// <returns>True if valid. False if not valid</returns>
        public bool ValidateCredentials()
        {
            bool isValid;
            var registrationProvider = _credentialProvider.Provide().ApiRegistrationProvider;

            switch (registrationProvider)
            {
                case ApiRegistrationProviderTypes.Jasper:
                    isValid = new JasperCellularClient(_credentialProvider).ValidateCredentials();
                    break;
                case ApiRegistrationProviderTypes.Ericsson:
                    isValid = new EricssonCellularClient(_credentialProvider).ValidateCredentials();
                    break;
                default:
                    throw new IndexOutOfRangeException($"Could not find a service for '{registrationProvider.ToString()}' provider");
            }

            return isValid;
        }

        private List<SimState> getAvailableSimStates()
        {
            return new List<SimState>()
            {
                new SimState()
                {
                    Id = "1",
                    Name = "Active"
                },
                new SimState()
                {
                    Id = "2",
                    Name = "Disconnected"
                }
            };
        }

        /// <summary>
        /// Gets the available subscription packages from the appropriate api provider
        /// </summary>
        /// <returns>SubscriptionPackage Model</returns>
        private List<SubscriptionPackage> getAvailableSubscriptionPackages()
        {
            return new List<SubscriptionPackage>()
            {
                new SubscriptionPackage()
                {
                    Id = "1",
                    Name = "Basic"
                },
                new SubscriptionPackage()
                {
                    Id = "2",
                    Name = "Expensive"
                }
            };
        }

    }
}
