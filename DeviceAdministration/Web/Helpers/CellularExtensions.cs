using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using DeviceManagement.Infrustructure.Connectivity.Models.Constants;
using DeviceManagement.Infrustructure.Connectivity.Models.Other;
using DeviceManagement.Infrustructure.Connectivity.Models.TerminalDevice;
using DeviceManagement.Infrustructure.Connectivity.Services;
using Microsoft.Azure.Devices.Applications.RemoteMonitoring.Common.Models;
using Microsoft.Azure.Devices.Applications.RemoteMonitoring.DeviceAdmin.Infrastructure.Repository;

namespace Microsoft.Azure.Devices.Applications.RemoteMonitoring.DeviceAdmin.Web.Helpers
{
    public class CellularExtensions : ICellularExtensions
    {
        private readonly IExternalCellularService _cellularService;
        private readonly IIccidRepository _iccidRepository;

        public CellularExtensions(IExternalCellularService cellularService, IIccidRepository iccidRepository)
        {
            if (cellularService == null)
            {
                throw new ArgumentNullException(nameof(cellularService));
            }
            _cellularService = cellularService;
            _iccidRepository = iccidRepository;
        }

        public List<Iccid> GetTerminals()
        {
            return this._cellularService.GetTerminals();
        }

        public Terminal GetSingleTerminalDetails(Iccid iccid)
        {
            return this._cellularService.GetSingleTerminalDetails(iccid);
        }

        public List<SessionInfo> GetSingleSessionInfo(Iccid iccid)
        {
            return this._cellularService.GetSingleSessionInfo(iccid);
        }

        public IEnumerable<string> GetListOfAvailableIccids(IList<DeviceModel> devices, string providerName)
        {
            var fullIccidList = providerName == ApiRegistrationProviderTypes.Ericsson ?
                _iccidRepository.GetIccids().Select(e => e.Id) :
                _cellularService.GetTerminals().Select(i => i.Id);

            var usedIccidList = GetUsedIccidList(devices).Select(i => i.Id);
            return fullIccidList.Except(usedIccidList);
        }

        public IEnumerable<string> GetListOfAvailableDeviceIDs(IList<DeviceModel> devices)
        {
            return (from device in devices
                    where (device.DeviceProperties != null && device.DeviceProperties.DeviceID != null) &&
                          (device.SystemProperties == null || device.SystemProperties.ICCID == null)
                    select device.DeviceProperties.DeviceID
                   ).Cast<string>().ToList();
        }

        public IEnumerable<string> GetListOfConnectedDeviceIds(IList<DeviceModel> devices)
        {
            return (from device in devices
                    where (device.DeviceProperties != null && device.DeviceProperties.DeviceID != null) &&
                        (device.SystemProperties == null || device.SystemProperties.ICCID != null)
                    select device.DeviceProperties.DeviceID
                    ).ToList();
        }

        public bool ValidateCredentials()
        {
            return _cellularService.ValidateCredentials();
        }

        public List<SimState> GetAllAvailableSimStates(string iccid, string currentState)
        {
            var availableSimStates = _cellularService.GetAllAvailableSimStates(currentState);
            return availableSimStates;
        }

        public List<SimState> GetValidTargetSimStates(string iccid, string currentStateId)
        {
            var availableSimStates = _cellularService.GetValidTargetSimStates(iccid, currentStateId);
            return availableSimStates;
        }

        public List<SubscriptionPackage> GetAvailableSubscriptionPackages(string iccid, string currentSubscription)
        {
            var availableSubscriptions = _cellularService.GetAvailableSubscriptionPackages(iccid, currentSubscription);
            return markActiveSubscriptionPackage(currentSubscription, availableSubscriptions);
        }

        public bool UpdateSimState(string iccid, string updatedState)
        {
            return _cellularService.UpdateSimState(iccid, updatedState);
        }

        public bool UpdateSubscriptionPackage(string iccid, string updatedPackage)
        {
            return _cellularService.UpdateSubscriptionPackage(iccid, updatedPackage);
        }

        public bool ReconnectDevice(string iccid)
        {
            return _cellularService.ReconnectTerminal(iccid);
        }

        public async Task<bool> SendSms(string iccid, string smsText)
        {
            var terminal = GetSingleTerminalDetails(new Iccid(iccid));
            return await _cellularService.SendSms(iccid, terminal.Msisdn?.Id, smsText);
        }

        public string GetLocale(string iccid, out IEnumerable<string> availableLocaleNames)
        {
            return _cellularService.GetLocale(iccid, out availableLocaleNames);
        }

        public bool SetLocale(string iccid, string localeName)
        {
            var serviceRequestId = _cellularService.SetLocale(iccid, localeName);

            if (!string.IsNullOrWhiteSpace(serviceRequestId))
            {
                _iccidRepository.SetLastSetLocaleServiceRequestId(iccid, serviceRequestId);
                return true;
            }
            else
            {
                return false;
            }
        }

        public string GetLastSetLocaleServiceRequestState(string iccid)
        {
            var serviceRequestId = _iccidRepository.GetLastSetLocaleServiceRequestId(iccid);

            if (!string.IsNullOrWhiteSpace(serviceRequestId))
            {
                return _cellularService.GetLastSetLocaleServiceRequestState(serviceRequestId);
            }
            else
            {
                return null;
            }
        }

        private IEnumerable<Iccid> GetUsedIccidList(IList<DeviceModel> devices)
        {
            return (from device in devices
                    where device.DeviceProperties?.DeviceID != null &&
                        device.SystemProperties?.ICCID != null
                    select new Iccid(device.SystemProperties.ICCID)
                   ).ToList();
        }

        private List<SubscriptionPackage> markActiveSubscriptionPackage(string selectedSubscriptionName, List<SubscriptionPackage> availableSubscriptionPackages)
        {
            return availableSubscriptionPackages.Select(s =>
            {
                if (s.Name == selectedSubscriptionName) s.IsActive = true;
                return s;
            }).ToList();
        }

        private List<SimState> markActiveSimState(string selectedSubscriptionName, List<SimState> availableSimStates)
        {
            return availableSimStates.Select(s =>
            {
                if (s.Name == selectedSubscriptionName) s.IsActive = true;
                return s;
            }).ToList();
        }
    }
}