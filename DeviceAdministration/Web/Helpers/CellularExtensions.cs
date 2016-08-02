using System;
using System.Collections.Generic;
using System.Linq;
using DeviceManagement.Infrustructure.Connectivity.Models.TerminalDevice;
using DeviceManagement.Infrustructure.Connectivity.Services;
using Microsoft.Azure.Devices.Applications.RemoteMonitoring.Common.Models;

namespace Microsoft.Azure.Devices.Applications.RemoteMonitoring.DeviceAdmin.Web.Helpers
{
    public class CellularExtensions : ICellularExtensions
    {
        private readonly IExternalCellularService cellularService;

        public CellularExtensions(IExternalCellularService cellularService)
        {
            if (cellularService == null)
            {
                new ArgumentNullException("cellularService");
            }

            this.cellularService = cellularService;
        }

        public List<Iccid> GetTerminals()
        {
            return this.cellularService.GetTerminals();
        }

        public Terminal GetSingleTerminalDetails(Iccid iccid)
        {
            return this.cellularService.GetSingleTerminalDetails(iccid);
        }

        public List<SessionInfo> GetSingleSessionInfo(Iccid iccid)
        {
            return this.cellularService.GetSingleSessionInfo(iccid);
        }

        public IEnumerable<string> GetListOfAvailableIccids(IList<DeviceModel> devices)
        {
            var fullIccidList = this.cellularService.GetTerminals().Select(i => i.Id);
            var usedIccidList = this.GetUsedIccidList(devices).Select(i => i.Id);
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