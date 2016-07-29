using System.Collections.Generic;
using DeviceManagement.Infrustructure.Connectivity.Models.TerminalDevice;
using DeviceManagement.Infrustructure.Connectivity.Services;
using Microsoft.Azure.Devices.Applications.RemoteMonitoring.Common.Models;

namespace Microsoft.Azure.Devices.Applications.RemoteMonitoring.DeviceAdmin.Web.Helpers
{
    public interface ICellularExtensions
    {
        IEnumerable<string> GetListOfAvailableIccids(IExternalCellularService cellularService, IList<DeviceModel> devices);
        IEnumerable<string> GetListOfAvailableDeviceIDs(IExternalCellularService cellularService, IList<DeviceModel> devices);
    }
}