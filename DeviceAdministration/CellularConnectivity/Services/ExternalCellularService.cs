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
        private const string CellularInvalidCreds = "400200";
        private const string CellularInvalidLicense = "400100";

        public ExternalCellularService(
            IJasperCellularService jasperCellularService,
            ICredentialProvider credentialProvider)
        {
            _credentialProvider = credentialProvider;
        }

        public List<Iccid> GetTerminals(CellularProviderEnum cellularProvider)
        {
            List<Iccid> terminals = new List<Iccid>();

            switch (cellularProvider)
            {
                case CellularProviderEnum.Jasper:
                    terminals = _jasperCellularService.GetTerminals();
                    break;
                case Models.Enums.CellularProviderEnum.Ericsson:
                    //TODO call ericsson service
                    break;
                default:
                    throw new IndexOutOfRangeException($"Could not find a service for '{cellularProvider.ToString()}' provider");
            }

            return terminals;
        }

        public Terminal GetSingleTerminalDetails(Iccid iccid, CellularProviderEnum cellularProvider)
        {
            Terminal terminal = null;

            switch (cellularProvider)
            {
                case CellularProviderEnum.Jasper:
                    terminal = _jasperCellularService.GetSingleTerminalDetails(iccid);
                    break;
                case CellularProviderEnum.Ericsson:
                    //TODO call ericsson service
                    break;
                default:
                    throw new IndexOutOfRangeException($"Could not find a service for '{cellularProvider.ToString()}' provider");
            }

            return terminal;
        }

        public List<SessionInfo> GetSingleSessionInfo(Iccid iccid, CellularProviderEnum cellularProvider)
        {
            List<SessionInfo> sessionInfo = null;

            switch (cellularProvider)
            {
                case CellularProviderEnum.Jasper:
                    sessionInfo = _jasperCellularService.GetSingleSessionInfo(iccid);
                    break;
                case CellularProviderEnum.Ericsson:
                    //TODO call ericsson service
                    break;
                default:
                    throw new IndexOutOfRangeException($"Could not find a service for '{cellularProvider.ToString()}' provider");
            }

            return sessionInfo;
        }

        /// <summary>
        /// The API does not have a way to validate credentials so this method calls
        /// GetTerminals() and checks the response for validation errors.
        /// </summary>
        /// <returns>True if valid. False if not valid</returns>
        public bool ValidateCredentials(CellularProviderEnum cellularProvider)
        {
            bool isValid = false;

            switch (cellularProvider)
            {
                case CellularProviderEnum.Jasper:
                    isValid = _jasperCellularService.ValidateCredentials();
                    break;
                case CellularProviderEnum.Ericsson:
                    //TODO call ericsson service
                    break;
                default:
                    throw new IndexOutOfRangeException($"Could not find a service for '{cellularProvider.ToString()}' provider");
            }

            return isValid;
        }
    }
}
