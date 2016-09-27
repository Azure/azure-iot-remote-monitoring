using System;
using System.Collections.Generic;
using System.Linq;
using DeviceManagement.Infrustructure.Connectivity.Models.Constants;
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

        public bool ReconnectDevice(DeviceModel device)
        {
            return _cellularService.ReconnectTerminal(device.SystemProperties.ICCID);
        }

        public Iccid GetAssociatedDeviceTerminalIccid(IList<DeviceModel> allDevices, string deviceId)
        {
            return (from device in allDevices
                    where device.DeviceProperties?.DeviceID != null && 
                        device.SystemProperties?.ICCID != null && 
                        (device.id != null && device.id == deviceId)
                    select new Iccid(device.SystemProperties.ICCID)
                   ).FirstOrDefault();
        }

        public SimStateModel GetCurrentSimState()
        {
            return GetSampleSimStatusList().FirstOrDefault(s => s.Name == "Active");
        }

        public List<SimStateModel> GetAvailableSimStates()
        {
            var availableSimStates = GetAvailableSimStates();
            var selectedSubscription = GetCurrentSubscriptionPackage();
            return markActiveSimState(selectedSubscription.Id, availableSimStates);
        }

        public SubscriptionPackageModel GetCurrentSubscriptionPackage()
        {
            return GetSampleSubscriptionPackages().FirstOrDefault(s => s.Name == "Basic");
        }

        public List<SubscriptionPackageModel> GetAvailableSubscriptionPackages()
        {
            var availableSubscriptions = GetSampleSubscriptionPackages();
            var selectedSubscription = GetCurrentSubscriptionPackage();
            return markActiveSubscriptionPackage(selectedSubscription.Id, availableSubscriptions);
        }

        private IEnumerable<Iccid> GetUsedIccidList(IList<DeviceModel> devices)
        {
            return (from device in devices
                    where device.DeviceProperties?.DeviceID != null && 
                        device.SystemProperties?.ICCID != null
                    select new Iccid(device.SystemProperties.ICCID)
                   ).ToList();
        }

        /// <summary>
        /// TODO replace this. Only for mocking
        /// </summary>
        /// <returns></returns>
        private List<SimStateModel> GetSampleSimStatusList()
        {
            return new List<SimStateModel>()
            {
                new SimStateModel()
                {
                    Id = "1",
                    Name = "Active"
                },
                new SimStateModel()
                {
                    Id = "2",
                    Name = "Disabled"
                }
            };
        }

        /// <summary>
        /// TODO replace this. Only for mocking
        /// </summary>
        /// <returns></returns>
        private List<SubscriptionPackageModel> GetSampleSubscriptionPackages()
        {
            return new List<SubscriptionPackageModel>()
            {
                new SubscriptionPackageModel()
                {
                    Id = "1",
                    Name = "Basic"
                },
                new SubscriptionPackageModel()
                {
                    Id = "2",
                    Name = "Expensive"
                }
            };
        }

        private List<SubscriptionPackageModel> markActiveSubscriptionPackage(string selectedSubscriptionId, List<SubscriptionPackageModel> availableSubscriptionPackages)
        {
            return availableSubscriptionPackages.Select(s =>
            {
                if (s.Id == selectedSubscriptionId) s.IsActive = true;
                return s;
            }).ToList();
        }

        private List<SimStateModel> markActiveSimState(string selectedSubscriptionId, List<SimStateModel> availableSimStates)
        {
            return availableSimStates.Select(s =>
            {
                if (s.Id == selectedSubscriptionId) s.IsActive = true;
                return s;
            }).ToList();
        }
    }
}