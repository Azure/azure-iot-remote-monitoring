﻿using System.Collections.Generic;
using System.Linq;
using DeviceManagement.Infrustructure.Connectivity.Models.TerminalDevice;
using DeviceManagement.Infrustructure.Connectivity.Services;
using Microsoft.Azure.Devices.Applications.RemoteMonitoring.Common.Models;

namespace Microsoft.Azure.Devices.Applications.RemoteMonitoring.DeviceAdmin.Web.Helpers
{
    public static class CellularExtensions
    {
        public static IEnumerable<string> GetListOfAvailableIccidsND(this IExternalCellularService cellularService, IList<Common.Models.Device> devices)
        {
            var fullIccidList = cellularService.GetTerminals().Select(i => i.Id);
            var usedIccidList = GetUsedIccidListND(devices).Select(i => i.Id);
            return fullIccidList.Except(usedIccidList);
        }

        public static IEnumerable<string> GetListOfAvailableDeviceIDs(this IExternalCellularService cellularService, IList<Common.Models.Device> devices)
        {
            return (from device in devices
                    where (device.DeviceProperties != null && device.DeviceProperties.DeviceID != null) &&
                        (device.SystemProperties == null || device.SystemProperties.ICCID == null)
                    select device.DeviceProperties.DeviceID
                    ).Cast<string>().ToList();
        }

        private static IEnumerable<Iccid> GetUsedIccidListND(IList<Common.Models.Device> devices)
        {
            return (from device in devices
                    where (device.DeviceProperties != null && device.DeviceProperties.DeviceID != null) &&
                        (device.SystemProperties != null && device.SystemProperties.ICCID != null)
                    select new Iccid(device.SystemProperties.ICCID)
                    ).ToList();
        }
    }
}