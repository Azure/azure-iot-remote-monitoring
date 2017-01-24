using System.Collections.Generic;
using DeviceManagement.Infrustructure.Connectivity.Models.TerminalDevice;
using Microsoft.Azure.Devices.Applications.RemoteMonitoring.Common.Models;

namespace Microsoft.Azure.Devices.Applications.RemoteMonitoring.DeviceAdmin.Web.Helpers
{
    public interface ICellularExtensions
    {
        List<Iccid> GetTerminals();
        Terminal GetSingleTerminalDetails(Iccid iccid, ApiRegistrationProviderType? cellularProvider);
        List<SessionInfo> GetSingleSessionInfo(Iccid iccid, ApiRegistrationProviderType? cellularProvider);
        bool ValidateCredentials(ApiRegistrationProviderType? cellularProvider);
        IEnumerable<string> GetListOfAvailableIccids(IList<DeviceModel> devices);
        IEnumerable<string> GetListOfAvailableDeviceIDs(IList<DeviceModel> devices);
    }
}