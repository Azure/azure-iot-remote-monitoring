using System.Collections.Generic;
using System.Linq;
using DeviceManagement.Infrustructure.Connectivity;
using DeviceManagement.Infrustructure.Connectivity.Models.TerminalDevice;
using Microsoft.Azure.Devices.Applications.RemoteMonitoring.DeviceAdmin.Web.Models;
using DeviceManagement.Infrustructure.Connectivity.Models.Enums;

namespace Microsoft.Azure.Devices.Applications.RemoteMonitoring.DeviceAdmin.Web.Helpers
{
    public static class CellularExtensions
    {
        public static ApiRegistrationProviderType GetCellularProvider(this IExternalCellularService cellularService)
        {

            return ApiRegistrationProviderType.Jasper;
        }

        public static IEnumerable<string> GetListOfAvailableIccids(this IExternalCellularService cellularService, List<dynamic> devices)
        {
            var fullIccidList = cellularService.GetTerminals().Select(i => i.Id);
            var usedIccidList = GetUsedIccidList(devices).Select(i => i.Id);
            return fullIccidList.Except(usedIccidList);
        }

        public static IEnumerable<string> GetListOfAvailableDeviceIDs(this IExternalCellularService cellularService, List<dynamic> devices)
        {
            return (from device in devices
                    where (device.DeviceProperties != null && device.DeviceProperties.DeviceID != null) &&
                        (device.SystemProperties == null || device.SystemProperties.ICCID == null)
                    select device.DeviceProperties.DeviceID.Value
                    ).Cast<string>().ToList();
        }

        public static IEnumerable<string> GetListOfConnectedDeviceIds(this IExternalCellularService cellularService, List<dynamic> devices)
        {
            return (from device in devices
                    where (device.DeviceProperties != null && device.DeviceProperties.DeviceID != null) &&
                        (device.SystemProperties == null || device.SystemProperties.ICCID != null)
                    select device.DeviceProperties.DeviceID.Value
                    ).Cast<string>().ToList();
        }

        private static IEnumerable<Iccid> GetUsedIccidList(List<dynamic> devices)
        {
            return (from device in devices
                    where (device.DeviceProperties != null && device.DeviceProperties.DeviceID != null) &&
                        (device.SystemProperties != null && device.SystemProperties.ICCID != null)
                    select new Iccid(device.SystemProperties.ICCID.Value)
                    ).ToList();
        }
    }
}