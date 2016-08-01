using System;
using System.Collections.Generic;
using System.Linq;
using DeviceManagement.Infrustructure.Connectivity.com.jasperwireless.spark.terminal;
using DeviceManagement.Infrustructure.Connectivity.Exceptions;
using DeviceManagement.Infrustructure.Connectivity.Models.Security;
using DeviceManagement.Infrustructure.Connectivity.Models.TerminalDevice;
using DeviceManagement.Infrustructure.Connectivity.Proxies;

namespace DeviceManagement.Infrustructure.Connectivity.Services
{
    public class JasperCellularService : IExternalCellularService
    {
        private readonly ICredentialProvider _credentialProvider;

        public JasperCellularService(ICredentialProvider credentialProvider)
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

            return response.iccids.Select(iccid => new Iccid {Id = iccid}).ToList();
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


        private IJasperTerminalClientProxy BuildJasperTerminalClientProxy()
        {
            return new JasperTerminalClientProxy(_credentialProvider.Provide());
        }
    }
}