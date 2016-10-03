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

        public List<SimState> GetAllAvailableSimStates(string iccid)
        {
            List<SimState> availableStates;
            var registrationProvider = _credentialProvider.Provide().ApiRegistrationProvider;

            switch (registrationProvider)
            {
                case ApiRegistrationProviderTypes.Jasper:
                    var jasperClient = new JasperCellularClient(_credentialProvider);
                    availableStates = jasperClient.GetAvailableSimStates(iccid);
                    break;
                case ApiRegistrationProviderTypes.Ericsson:
                    var ericssonClient = new EricssonCellularClient(_credentialProvider);
                    availableStates = ericssonClient.GetAllAvailableSimStates();
                    break;
                default:
                    throw new IndexOutOfRangeException($"Could not find a service for '{registrationProvider}' provider");
            }
            return availableStates;
        }

        public List<SimState> GetValidTargetSimStates(SimState currentState)
        {
            List<SimState> availableStates;
            var registrationProvider = _credentialProvider.Provide().ApiRegistrationProvider;

            switch (registrationProvider)
            {
                case ApiRegistrationProviderTypes.Jasper:
                    var jasperClient = new JasperCellularClient(_credentialProvider);
                    availableStates = jasperClient.GetValidTargetSimStates(currentState);
                    break;
                case ApiRegistrationProviderTypes.Ericsson:
                    var ericssonClient = new EricssonCellularClient(_credentialProvider);
                    availableStates = ericssonClient.GetValidTargetSimStates(currentState);
                    break;
                default:
                    throw new IndexOutOfRangeException($"Could not find a service for '{registrationProvider}' provider");
            }
            return availableStates;
        }

        public List<SubscriptionPackage> GetAvailableSubscriptionPackages()
        {
            List<SubscriptionPackage> availableSubscriptionPackages = new List<SubscriptionPackage>();
            var registrationProvider = _credentialProvider.Provide().ApiRegistrationProvider;

            switch (registrationProvider)
            {
                case ApiRegistrationProviderTypes.Jasper:
                    var jasperClient = new JasperCellularClient(_credentialProvider);
                    availableSubscriptionPackages = jasperClient.GetAvailableSubscriptionPackages();
                    break;
                case ApiRegistrationProviderTypes.Ericsson:
                    var ericssonClient = new EricssonCellularClient(_credentialProvider);
                    var subscriptionQueryResponse = ericssonClient.GetAvailableSubscriptionPackages();
                    availableSubscriptionPackages.AddRange(subscriptionQueryResponse.Select(
                        subscription => new SubscriptionPackage()
                    {
                        Name = subscription.subscriptionPackageName,
                        IsActive = false
                    }));
                    break;
                default:
                    throw new IndexOutOfRangeException($"Could not find a service for '{registrationProvider}' provider");
            }
            return availableSubscriptionPackages;
        }

        public async Task<bool> UpdateSimState(string iccid, string updatedState)
        {
            bool result;
            var registrationProvider = _credentialProvider.Provide().ApiRegistrationProvider;

            switch (registrationProvider)
            {
                case ApiRegistrationProviderTypes.Jasper:
                    var jasperClient = new JasperCellularClient(_credentialProvider);
                    result = jasperClient.UpdateSimState(iccid, updatedState);
                    break;
                case ApiRegistrationProviderTypes.Ericsson:
                    var ericssonClient = new EricssonCellularClient(_credentialProvider);
                    var status = SubscriptionStatusFactory.CreateEricssonSubscriptionStatusRequestEnum(updatedState);
                    var requestResult = ericssonClient.UpdateSimState(iccid, status);
                    // TODO SR property handle this response
                    result = true;
                    break;
                default:
                    throw new IndexOutOfRangeException($"Could not find a service for '{registrationProvider}' provider");
            }
            return result;
        }

        public async Task<bool> UpdateSubscriptionPackage(string iccid, string updatedPackage)
        {
            bool result;
            var registrationProvider = _credentialProvider.Provide().ApiRegistrationProvider;

            switch (registrationProvider)
            {
                case ApiRegistrationProviderTypes.Jasper:
                    var jasperClient = new JasperCellularClient(_credentialProvider);
                    result = jasperClient.UpdateSubscriptionPackage(iccid, updatedPackage);
                    break;
                case ApiRegistrationProviderTypes.Ericsson:
                    var ericssonClient = new EricssonCellularClient(_credentialProvider);
                    result = ericssonClient.UpdateSubscriptionPackage(iccid, updatedPackage);
                    break;
                default:
                    throw new IndexOutOfRangeException($"Could not find a service for '{registrationProvider}' provider");
            }
            return result;
        }

        public async Task<bool> ReconnectTerminal(string iccid)
        {
            bool result;
            var registrationProvider = _credentialProvider.Provide().ApiRegistrationProvider;

            switch (registrationProvider)
            {
                case ApiRegistrationProviderTypes.Jasper:
                    var jasperClient = new JasperCellularClient(_credentialProvider);
                    result = jasperClient.ReconnectTerminal(iccid);
                    break;
                case ApiRegistrationProviderTypes.Ericsson:
                    var ericssonClient = new EricssonCellularClient(_credentialProvider);
                    result = ericssonClient.ReconnectTerminal(iccid);
                    break;
                default:
                    throw new IndexOutOfRangeException($"Could not find a service for '{registrationProvider}' provider");
            }
            return result;
        }

        public async Task<bool> SendSms(string iccid, string smsText)
        {
            bool result;
            var registrationProvider = _credentialProvider.Provide().ApiRegistrationProvider;

            switch (registrationProvider)
            {
                case ApiRegistrationProviderTypes.Jasper:
                    var jasperClient = new JasperCellularClient(_credentialProvider);
                    result = jasperClient.SendSms(iccid, smsText);
                    break;
                case ApiRegistrationProviderTypes.Ericsson:
                    var ericssonClient = new EricssonCellularClient(_credentialProvider);
                    result = ericssonClient.SendSms(iccid, smsText);
                    break;
                default:
                    throw new IndexOutOfRangeException($"Could not find a service for '{registrationProvider}' provider");
            }
            return result;
        }


        public SubscriptionPackage GetCurrentSubscriptionPackage(string currentSubscriptionName)
        {
            return GetAvailableSubscriptionPackages().FirstOrDefault(s => s.Name == currentSubscriptionName);
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
                    Name = "Active"
                },
                new SimState()
                {
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
                    Name = "Basic"
                },
                new SubscriptionPackage()
                {
                    Name = "Expensive"
                }
            };
        }

    }
}
