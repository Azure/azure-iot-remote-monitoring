using System;
using System.Collections.Generic;
using System.Linq;
using DeviceManagement.Infrustructure.Connectivity;
using DeviceManagement.Infrustructure.Connectivity.Models.TerminalDevice;
using Microsoft.Azure.Devices.Applications.RemoteMonitoring.DeviceAdmin.Web.Models;
using DeviceManagement.Infrustructure.Connectivity.Models.Enums;
using DeviceManagement.Infrustructure.Connectivity.Services;
using Microsoft.Azure.Devices.Applications.RemoteMonitoring.Common.Models;

namespace Microsoft.Azure.Devices.Applications.RemoteMonitoring.DeviceAdmin.Web.Helpers
{
    public static class CellularExtensions
    {
        public static IEnumerable<string> GetListOfAvailableIccids(this IExternalCellularService cellularService, List<dynamic> devices)
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

        private static IEnumerable<Iccid> GetUsedIccidList(List<dynamic> devices)
        {
            return (from device in devices
                    where (device.DeviceProperties != null && device.DeviceProperties.DeviceID != null) &&
                          (device.SystemProperties != null && device.SystemProperties.ICCID != null)
                    select new Iccid(device.SystemProperties.ICCID)
                   ).ToList();
        }
    }
}