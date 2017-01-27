using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using DeviceManagement.Infrustructure.Connectivity.com.jasper.api.sms;
using DeviceManagement.Infrustructure.Connectivity.com.jasperwireless.spark.terminal;
using DeviceManagement.Infrustructure.Connectivity.Constants;
using DeviceManagement.Infrustructure.Connectivity.Exceptions;
using DeviceManagement.Infrustructure.Connectivity.Models.Other;
using DeviceManagement.Infrustructure.Connectivity.Models.Security;
using DeviceManagement.Infrustructure.Connectivity.Models.TerminalDevice;
using DeviceManagement.Infrustructure.Connectivity.Proxies;

namespace DeviceManagement.Infrustructure.Connectivity.Clients
{
    public class JasperCellularClient
    {
        private readonly ICredentialProvider _credentialProvider;
        private const string CellularInvalidCreds = "400200";
        private const string CellularInvalidLicense = "400100";

        public JasperCellularClient(ICredentialProvider credentialProvider)
        {
            _credentialProvider = credentialProvider;
        }

        public List<Iccid> GetTerminals()
        {
            var proxy = BuildJasperTerminalClientProxy();

            GetModifiedTerminalsResponse response;

            try
            {
                response = proxy.GetModifiedTerminals();
            }
            catch (Exception e)
            {
                throw new CellularConnectivityException(e);
            }

            return response.iccids.Select(iccid => new Iccid { Id = iccid }).ToList();
        }

        public Terminal GetSingleTerminalDetails(Iccid iccid)
        {
            var proxy = BuildJasperTerminalClientProxy();

            GetTerminalDetailsResponse response;

            try
            {
                response = proxy.GetSingleTerminalDetails(iccid);
            }
            catch (Exception exception)
            {
                throw new CellularConnectivityException(exception);
            }

            var terminal = response.terminals.FirstOrDefault();

            if (terminal == null)
            {
                return new Terminal();
            }

            return new Terminal
            {
                OverageLimitReached = terminal.overageLimitReached,
                Status = terminal.status,
                MonthToDateDataUsage = terminal.monthToDateDataUsage,
                RatePlan = terminal.ratePlan,
                AccountId = terminal.accountId,
                IsInActiveState = isAnActiveState(terminal.status),
                Iccid = new Iccid
                {
                    Id = terminal.iccid
                },
                Imei = new Imei
                {
                    Id = terminal.imei
                },
                Imsi = new Imsi
                {
                    Id = terminal.imsi
                }
            };
        }

        public List<SessionInfo> GetSingleSessionInfo(Iccid iccid)
        {
            var proxy = BuildJasperTerminalClientProxy();

            GetSessionInfoResponse response;
            try
            {
                response = proxy.GetSingleSessionInfo(iccid);
            }
            catch (Exception exception)
            {
                throw new CellularConnectivityException(exception);
            }

            var sessionInfo = response.sessionInfo;
            if (sessionInfo == null)
            {
                return new List<SessionInfo>();
            }

            return sessionInfo.Select(info => new SessionInfo
            {
                DateSessionEnded = info.dateSessionEnded,
                DateSessionStarted = info.dateSessionStarted,
                IpAddress = info.ipAddress,
                Iccid = new Iccid
                {
                    Id = info.iccid
                }
            }).ToList();
        }

        public SendSMSResponse SendSms(string iccid, string messageText)
        {
            var proxy = BuildJasperSmsClientProxy();
            SendSMSResponse response;
            try
            {
                response = proxy.SendSms(iccid, messageText);
            }
            catch (Exception e)
            {
                throw new CellularConnectivityException(e);
            }
            return response;
        }

        public bool ReconnectTerminal(string iccid)
        {
            var proxy = BuildJasperTerminalClientProxy();
            SendCancelLocationResponse resonse;
            bool success;
            try
            {
                resonse = proxy.CancelLocation(iccid);
                success = resonse.status == "Success";
            }
            catch(Exception e)
            {
                throw new CellularConnectivityException(e);
            }
            return success;
        }

        /// <summary>
        /// The API does not have a way to validate credentials so this method calls
        /// GetTerminals() and checks the response for validation errors.
        /// </summary>
        /// <returns>True if valid. False if not valid</returns>
        public bool ValidateCredentials()
        {
            var isValid = false;
            var validationError = false;

            // make the simple API call
            try
            {
                GetTerminals();
            }
            catch (CellularConnectivityException exception)
            {
                //Check for validation errors
                if (exception.Message.Contains("The remote name could not be resolved") ||
                    exception.Message == CellularInvalidCreds ||
                    exception.Message == CellularInvalidLicense)
                {
                    validationError = true;
                }
                else
                {
                    validationError = false;
                }
            }

            // if no errors then credentials are valid
            if (!validationError)
            {
                isValid = true;
            }
            return isValid;
        }

        public List<SimState> GetValidTargetSimStates(string iccid, string currentStateId)
        {
            return GetAvailableSimStates(iccid, currentStateId);
        }

        public List<SimState> GetAvailableSimStates(string iccid, string currentStateId = null)
        {
            List<SimState> simStates;
            try
            {
                simStates = new List<SimState>()
                {
                    new SimState()
                    {
                        Name = JasperApiConstants.TerminalStates.ACTIVATED_LABEL,
                        Id = JasperApiConstants.TerminalStates.ACTIVATED_CODE
                    },
                    new SimState()
                    {
                        Name = JasperApiConstants.TerminalStates.DEACTIVATED_LABEL,
                        Id = JasperApiConstants.TerminalStates.DEACTIVATED_CODE
                    },
                    new SimState()
                    {
                        Name = currentStateId,
                        Id = currentStateId
                    }
                };
            }
            catch (Exception e)
            {
                throw new CellularConnectivityException(e);
            }

            return simStates.Select(simState => new SimState()
            {
                Id = simState.Id,
                Name = simState.Name,
                IsActive = currentStateId == simState.Id
            })
            .ToList();
        }

        /// <summary>
        /// Gets the available subscription packages from the appropriate api provider
        /// </summary>
        /// <returns>SubscriptionPackage Model</returns>
        public List<SubscriptionPackage> GetAvailableSubscriptionPackages(string iccid, string currentSubscription)
        {
            return new List<SubscriptionPackage>()
            {
                new SubscriptionPackage()
                {
                    Name = currentSubscription,
                    IsActive = true
                }
            };
        }

        public EditTerminalResponse EditTerminal(Iccid iccid, int changeType, string targetValue)
        {
            var proxy = BuildJasperTerminalClientProxy();

            EditTerminalResponse response;
            try
            {
                response = proxy.EditTerminal(iccid, changeType, targetValue);
            }
            catch (Exception e)
            {
                throw new CellularConnectivityException(e);
            }
            return response;
        }

        public EditTerminalResponse UpdateSimState(string iccid, string newStatus)
        {
            var proxy = BuildJasperTerminalClientProxy();
            EditTerminalResponse response;
            try
            {
                response = proxy.EditTerminal(new Iccid(iccid), 3, newStatus);
            }
            catch (Exception e)
            {
                throw new CellularConnectivityException(e);
            }
            return response;
        }

        public EditTerminalResponse UpdateSubscriptionPackage(string iccid, string ratePlan)
        {
            var proxy = BuildJasperTerminalClientProxy();
            EditTerminalResponse response;

            try
            {
                response = proxy.EditTerminal(new Iccid(iccid), 3, ratePlan);
            }
            catch (Exception e)
            {
                throw new CellularConnectivityException(e);
            }

            return response;
        }

        private bool isAnActiveState(string stateString)
        {
            switch (stateString)
            {
                case JasperApiConstants.TerminalStates.ACTIVATED_CODE:
                    {
                        return true;
                    }
                case JasperApiConstants.TerminalStates.DEACTIVATED_CODE:
                    {
                        return false;
                    }
                default:
                    {
                        return false;
                    }
            }
        }

        private IJasperTerminalClientProxy BuildJasperTerminalClientProxy()
        {
            return new JasperTerminalClientProxy(_credentialProvider.Provide());
        }

        private IJasperSmsClientProxy BuildJasperSmsClientProxy()
        {
            return new JasperSmsClientProxy(_credentialProvider.Provide());
        }

    }
}