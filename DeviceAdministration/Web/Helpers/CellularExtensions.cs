using System;
using System.Collections.Generic;
using System.Linq;
using DeviceManagement.Infrustructure.Connectivity;
using DeviceManagement.Infrustructure.Connectivity.Models.Constants;
using DeviceManagement.Infrustructure.Connectivity.Models.TerminalDevice;
using Microsoft.Azure.Devices.Applications.RemoteMonitoring.DeviceAdmin.Web.Models;
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

        private IEnumerable<Iccid> GetUsedIccidList(IList<DeviceModel> devices)
        {
            return (from device in devices
                    where (device.DeviceProperties != null && device.DeviceProperties.DeviceID != null) &&
                          (device.SystemProperties != null && device.SystemProperties.ICCID != null)
                    select new Iccid(device.SystemProperties.ICCID)
                   ).ToList();
        }
    }
}