using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using DeviceManagement.Infrustructure.Connectivity.Builders;
using DeviceManagement.Infrustructure.Connectivity.DeviceReconnect;
using DeviceManagement.Infrustructure.Connectivity.EricssonApiService;
using DeviceManagement.Infrustructure.Connectivity.EricssonSubscriptionService;
using DeviceManagement.Infrustructure.Connectivity.EricssonTrafficManagment;
using DeviceManagement.Infrustructure.Connectivity.Models.Other;
using DeviceManagement.Infrustructure.Connectivity.Models.Security;
using DeviceManagement.Infrustructure.Connectivity.Models.TerminalDevice;
using resource = DeviceManagement.Infrustructure.Connectivity.EricssonSubscriptionService.resource;
using System.Net.Http;
using System.Text;
using DeviceManagement.Infrustructure.Connectivity.Models.Jasper;
using System.Web.Script.Serialization;
using System.Net.Http.Headers;
using DeviceManagement.Infrustructure.Connectivity.Exceptions;

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
            };
        }

        public Terminal GetSingleTerminalDetails(Iccid iccid)
        {
            var terminal = new EricssonTerminal();
            try
            {
                var subManClient = EricssonServiceBuilder.GetSubscriptionManagementClient(_credentialProvider.Provide());
                var querySimResourceResponse = subManClient.QuerySimResource_v2(new QuerySimResource_v2() { resource = new resource() { id = iccid.Id, type = "icc" } });

                //check it even exists
                if (querySimResourceResponse.simResource.Length <= 0) return terminal;
                var sim = querySimResourceResponse.simResource.First();

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
                terminal.IsInActiveState = isAnActiveState(sim.simSubscriptionStatus);

                var querySubscriptionsResult = QuerySubscriptions(sim.imsi);
                if (querySubscriptionsResult.subscriptions.Any())
                {
                    var subscription = querySubscriptionsResult.subscriptions.First();
                    terminal.LastData = DateTime.Compare(subscription.lastData, DateTime.MinValue) != 0 ? subscription.lastData : (DateTime?)null;
                    terminal.LastPDPContext = DateTime.Compare(subscription.lastPDPContext, DateTime.MinValue) != 0 ? subscription.lastPDPContext : (DateTime?)null;
                }

                var querySubscriptionTraffic = QuerySubscriptionTraffic(sim.imsi);
                if (querySubscriptionTraffic.traffic.Any())
                {
                    var subscription = querySubscriptionTraffic.traffic.First();
                    terminal.CountryCode = subscription.lastLu.countryCode;
                    terminal.OperatorCode = subscription.lastLu.operatorCode;
                }

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

        public List<SimState> GetValidTargetSimStates(string currentState)
        {
            return getValidTargetSimStates(currentState);
        }

        /// <summary>
        /// Gets the available subscription packages from the appropriate api provider
        /// </summary>
        /// <returns>SubscriptionPackage Model</returns>
        public List<SubscriptionPackage> GetAvailableSubscriptionPackages(string iccid, string currentSubscription)
        {
            var subscriptionManagementClient = EricssonServiceBuilder.GetSubscriptionManagementClient(_credentialProvider.Provide());

            var availableSubscriptionPackages = subscriptionManagementClient.QuerySubscriptionPackages(new QuerySubscriptionPackages())
            .Select(
                subscription => new SubscriptionPackage()
                {
                    Name = subscription.subscriptionPackageName,
                    IsActive = subscription.subscriptionPackageName == currentSubscription
                }
            ).ToList();

            return availableSubscriptionPackages;
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

        public QuerySubscriptionsResponse QuerySubscriptions(string imsi)
        {
            var subscriptionManagementClient = EricssonServiceBuilder.GetSubscriptionManagementClient(_credentialProvider.Provide());
            return subscriptionManagementClient.QuerySubscriptions(new QuerySubscriptionsRequest()
            {
                maxResults = 10,
                resource = new resource()
                {
                    id = imsi,
                    type = "imsi"
                }
            });
        }

        public queryResponse QuerySubscriptionTraffic(string imsi)
        {
            var subscriptionTrafficClient = EricssonServiceBuilder.GetSubscriptionTrafficClient(_credentialProvider.Provide());
            return subscriptionTrafficClient.query(new query()
            {
                resource = new EricssonTrafficManagment.resource()
                {
                    id = imsi,
                    type = resourceType.imsi
                }
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

        public RequestSubscriptionPackageChangeResponse UpdateSubscriptionPackage(string iccid, string updatedPackage)
        {
            var subscriptionManagementClient = EricssonServiceBuilder.GetSubscriptionManagementClient(_credentialProvider.Provide());
            //TODO SR should we wait for this process to be complete? It is long running I think
            return subscriptionManagementClient.RequestSubscriptionPackageChange(new RequestSubscriptionPackageChange()
            {
                subscriptionPackage = updatedPackage,
                resource = new resource()
                {
                    id = iccid,
                    type = "icc"
                }
            });
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

        public async Task<bool> SendSMS(string msisdn, string messageContent)
        {
            var creds = (EricssonCredentials)_credentialProvider.Provide();
            var senderAddress = creds.EnterpriseSenderNumber;
            var smsEndpointBaseUrl = creds.SmsEndpointBaseUrl;
            var basicAuthPassword = Base64Encode($"{creds.Username}:{creds.Password}");
            if (string.IsNullOrWhiteSpace(senderAddress) || string.IsNullOrWhiteSpace(smsEndpointBaseUrl))
            {
                throw new ApplicationException("You have not provided an EnterpriseSenderAddress and/or a SmsEndpointBaseUrl");
            }
            var requestBodyModel = new SendSmsRequest()
            {
                outboundSMSMessageRequest = new Outboundsmsmessagerequest()
                {
                    address = new string[] { $"tel:{msisdn}" },
                    senderAddress = $"tel:{senderAddress}",
                    outboundSMSTextMessage = new Outboundsmstextmessage()
                    {
                        message = messageContent
                    }
                }
            };
            var requestBody = new JavaScriptSerializer().Serialize(requestBodyModel);
            var content = new StringContent(requestBody, Encoding.UTF8, "application/json");
            var endpointUrl = $"https://{smsEndpointBaseUrl}/dcpapi/smsmessaging/v1/outbound/tel:{senderAddress}/requests";

            using (var client = new HttpClient())
            {
                client.DefaultRequestHeaders.Accept.Clear();
                client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
                client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Basic", basicAuthPassword);
                client.DefaultRequestHeaders.Add("Connection", "keep-alive");
                client.DefaultRequestHeaders.Host = $"{smsEndpointBaseUrl}:80";
                var httpRequestMessage = new HttpRequestMessage(HttpMethod.Post, endpointUrl)
                {
                    Version = HttpVersion.Version10,
                    Content = content
                };
                try
                {
                    var response = await client.SendAsync(httpRequestMessage);
                    if (response.StatusCode != HttpStatusCode.Created)
                    {
                        throw new ApplicationException("Failed to Send SMS");
                    }
                }
                catch (Exception exception)
                {
                    throw new CellularConnectivityException(exception);
                }
            }

            return true;
        }

        private bool isAnActiveState(subscriptionStatus status)
        {
            switch (status)
            {
                case subscriptionStatus.Active:
                    {
                        return true;
                    }
                default:
                    {
                        return false;
                    }
            }
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

        private List<SimState> ensureCurrentStateIsInList(List<SimState> simStateList, string simState)
        {
            if (simStateList.All(s => s.Name != simState))
            {
                simStateList.Add(new SimState() { IsActive = true, Name = simState });
            }
            return simStateList;
        }

        private List<SimState> getValidTargetSimStates(string currentState)
        {
            List<SimState> result;
            var allValidTargets = allValidTargetStates().Select(simState => new SimState()
            {
                Name = simState.ToString(),
                IsActive = currentState == simState.ToString()
            }).ToList();

            switch (currentState)
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
                            new SimState() { IsActive = true, Name = currentState }
                        };
                        break;
                    }
            }
            return ensureCurrentStateIsInList(result, currentState);
        }

        private class SendSmsRequest
        {
            public Outboundsmsmessagerequest outboundSMSMessageRequest { get; set; }
        }

        private class Outboundsmsmessagerequest
        {
            public string[] address { get; set; }
            public string senderAddress { get; set; }
            public Outboundsmstextmessage outboundSMSTextMessage { get; set; }
            public string senderName { get; set; }
        }

        private class Outboundsmstextmessage
        {
            public string message { get; set; }
        }

        private static string Base64Encode(string plainText)
        {
            var plainTextBytes = System.Text.Encoding.UTF8.GetBytes(plainText);
            return System.Convert.ToBase64String(plainTextBytes);
        }

    }
}
