using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using DeviceManagement.Infrustructure.Connectivity.Builders;
using DeviceManagement.Infrustructure.Connectivity.DeviceReconnect;
using DeviceManagement.Infrustructure.Connectivity.EricssonApiService;
using DeviceManagement.Infrustructure.Connectivity.EricssonSubscriptionService;
using DeviceManagement.Infrustructure.Connectivity.Models.Other;
using DeviceManagement.Infrustructure.Connectivity.Models.Security;
using DeviceManagement.Infrustructure.Connectivity.Models.TerminalDevice;

namespace DeviceManagement.Infrustructure.Connectivity.Clients
{
    public class EricssonCellularClient
    {
        private readonly ICredentialProvider _credentialProvider;
        public EricssonCellularClient(ICredentialProvider credentialProvider)
        {
            _credentialProvider = credentialProvider;
        }


        public bool ValidateCredentials()
        {

            var isValid = false;

            //simple check - if it throws an exception then the creds are no good
            //todo: catch the correct error code
            try
            {
                var apiStatusClient = EricssonServiceBuilder.GetApiStatusClient(_credentialProvider.Provide());
                apiStatusClient.echo(new echo());
                isValid = true;
            }
            catch (Exception ex)
            {
                isValid = false;
            }
            return isValid;
        }

        public List<Iccid> GetTerminals()
        {
            //todo : Stubbed out with real ICCIDs from test account until ericsson sort an endpoint to do what we need
            return new List<Iccid>()
            {
                new Iccid("9883011539830007560"),
                new Iccid("89883011539830007560"),
                new Iccid("89883011539830007586"),
                new Iccid("89883011539830007594"),
                new Iccid("89883011539830007602"),
            };
        }

        public Terminal GetSingleTerminalDetails(Iccid iccid)
        {
            var terminal = new EricssonTerminal();
            try
            {
                var subManClient = EricssonServiceBuilder.GetSubscriptionManagementClient(_credentialProvider.Provide());
                var response = subManClient.QuerySimResource_v2(new QuerySimResource_v2() { resource = new resource() { id = iccid.Id, type = "icc" } });

                //check it even exists
                if (response.simResource.Length <= 0) return terminal;
                var sim = response.simResource.First();

                terminal.Status = sim.simSubscriptionStatus.ToString();
                terminal.DateOfActivation = sim.firstActivationDate; //todo: check this is correct
                terminal.Iccid = new Iccid(sim.icc);
                terminal.Imei = new Imei(sim.imei);
                terminal.Imsi = new Imsi(sim.imsi);
                terminal.Msisdn = new Msisdn(sim.msisdn); //todo: needs a view 
                terminal.RatePlan = sim.productOfferName;
                terminal.PriceProfileName = sim.priceProfileName;
                terminal.PdpContextProfileName = sim.pdpContextProfileName;
                terminal.AccountId = Convert.ToInt32(sim.customerNo); //todo : this will be customer number on the view

            }
            catch (Exception exception)
            {
                return terminal;
            }
            return terminal;
        }

        public List<SessionInfo> GetSingleSessionInfo(Iccid iccid)
        {
            return new List<SessionInfo>();
        }

        public List<SimState> GetAllAvailableSimStates()
        {
            return GetSimStatesFromEricssonSimStateEnum();
        }

        public List<SimState> GetValidTargetSimStates(SimState currentState)
        {
            return getValidTargetSimStates(currentState);
        }

        /// <summary>
        /// Gets the available subscription packages from the appropriate api provider
        /// </summary>
        /// <returns>SubscriptionPackage Model</returns>
        public subscriptionPackage[] GetAvailableSubscriptionPackages()
        {
            var subscriptionManagementClient = EricssonServiceBuilder.GetSubscriptionManagementClient(_credentialProvider.Provide());
            var availableSubscriptions = subscriptionManagementClient.QuerySubscriptionPackages(new QuerySubscriptionPackages());
            return availableSubscriptions;
        }

        public RequestSubscriptionStatusChangeResponse UpdateSimState(string iccid, subscriptionStatusRequest updatedState)
        {
            var subscriptionManagementClient = EricssonServiceBuilder.GetSubscriptionManagementClient(_credentialProvider.Provide());
            return subscriptionManagementClient.RequestSubscriptionStatusChange(new RequestSubscriptionStatusChange()
            {
                resource = new resource()
                {
                    id = iccid,
                    type = "icc"
                },
                subscriptionStatus = updatedState
            });
        }

        public QuerySubscriptionStatusChangeResponse UpdateSimState(string serviceRequestId)
        {
            var subscriptionManagementClient = EricssonServiceBuilder.GetSubscriptionManagementClient(_credentialProvider.Provide());
            return subscriptionManagementClient.QuerySubscriptionStatusChange(new QuerySubscriptionStatusChange()
            {
                serviceRequestId = serviceRequestId
            });
        }

      
        public List<SimState> GetSimStatesFromEricssonSimStateEnum()
        {
            return Enum.GetNames(typeof(subscriptionStatus)).Select(simStateName => new SimState()
            {
                Name = simStateName,
                IsActive = false
            }).ToList();
        }

        public bool UpdateSubscriptionPackage(string iccid, string updatedPackage)
        {
            var subscriptionManagementClient = EricssonServiceBuilder.GetSubscriptionManagementClient(_credentialProvider.Provide());
            var result =
                subscriptionManagementClient.RequestSubscriptionPackageChange(new RequestSubscriptionPackageChange()
                {
                    subscriptionPackage = updatedPackage,
                    resource = new resource()
                    {
                        id = iccid,
                        type = "icc"
                    }
                });
            //TODO SR should we wait for this process to be complete? It is long running I think.
            return true;
        }

        public ReconnectResponse ReconnectTerminal(string iccid)
        {
            var deviceReconnectClient = EricssonServiceBuilder.GetDeviceReconnectClient(_credentialProvider.Provide());
            return deviceReconnectClient.reconnect(new Reconnect()
            {
                resource = new Resource()
                {
                    id = iccid,
                    type = ResourceType.icc
                }
            });
        }

        public bool SendSms(string iccid, string smsText)
        {
            return true;
        }

        private List<subscriptionStatus> allValidTargetStates()
        {
            return new List<subscriptionStatus>()
            {
                subscriptionStatus.Active,
                subscriptionStatus.Deactivated,
                subscriptionStatus.Terminated,
                subscriptionStatus.Pause
            };
        }

        private List<SimState> ensureCurrentStateIsInList(List<SimState> simStateList, SimState currentState)
        {
            if (simStateList.All(s => s.Name != currentState.Name))
            {
                simStateList.Add(currentState);
            }
            return simStateList;
        }

        private List<SimState> getValidTargetSimStates(SimState currentState)
        {
            List<SimState> result;
            var allValidTargets = allValidTargetStates().Select(simState => new SimState()
            {
                Name = simState.ToString(),
                IsActive = false
            }).ToList();
            var selected = allValidTargets.FirstOrDefault(s => s.Name == currentState.Name);
            if (selected == null)
            {
                allValidTargets.Add(currentState);
            }
            else
            {
                selected.IsActive = true;
            }

            switch (currentState.Name)
            {
                case "Active":
                    result = allValidTargets.Where(ss => ss.Name == "Deactivated" || ss.Name == "Pause" || ss.Name == "Terminated").ToList();
                    break;
                case "Deactivated":
                    result = allValidTargets.Where(ss => ss.Name == "Active" || ss.Name == "Pause" || ss.Name == "Terminated").ToList();
                    break;
                case "Pause":
                    result = allValidTargets.Where(ss => ss.Name == "Active" || ss.Name == "Terminated").ToList();
                    break;
                case "Terminated":
                    result = allValidTargets.Where(ss => ss.Name == "Active").ToList();
                    break;
                default:
                    {
                        result = new List<SimState>()
                        {
                            currentState
                        };
                        break;
                    }
            }
            return ensureCurrentStateIsInList(result, currentState);
        }


    }
}
