using System.Collections.Generic;
using System.Linq;
using DeviceManagement.Infrustructure.Connectivity.Models.TerminalDevice;
using DeviceManagement.Infrustructure.Connectivity.Services;

namespace Microsoft.Azure.Devices.Applications.RemoteMonitoring.DeviceAdmin.Web.Helpers
{
    public static class CellularExtensions
    {
        public static IEnumerable<string> GetListOfAvailableIccids(this IExternalCellularService cellularService, List<dynamic> devices)
        {
            var fullIccidList = cellularService.GetTerminals().Select(i => i.Id);
            var usedIccidList = GetUsedIccidList(devices).Select(i => i.Id);
            return fullIccidList.Except(usedIccidList);
        }

        public static IEnumerable<string> GetListOfAvailableDeviceIDs(this IExternalCellularService cellularService, List<dynamic> devices)
        {
            return (from device in devices where device.DeviceProperties != null && 
                        device.DeviceProperties.ICCID == null select device.DeviceProperties.DeviceID.Value).
                        Cast<string>().ToList();
        }

        private static IEnumerable<Iccid> GetUsedIccidList(List<dynamic> devices)
        {
            return (from d in devices where d.DeviceProperties != null && d.DeviceProperties.ICCID != null &&
                        d.DeviceProperties.DeviceID != null select new Iccid(d.DeviceProperties.ICCID.Value)).ToList();
        }
    }
}