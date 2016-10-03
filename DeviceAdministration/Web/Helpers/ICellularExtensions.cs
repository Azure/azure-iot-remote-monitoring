using System.Collections.Generic;
using System.Threading.Tasks;
using DeviceManagement.Infrustructure.Connectivity.Models.Other;
using DeviceManagement.Infrustructure.Connectivity.Models.TerminalDevice;
using Microsoft.Azure.Devices.Applications.RemoteMonitoring.Common.Models;
using Microsoft.Azure.Devices.Applications.RemoteMonitoring.DeviceAdmin.Web.Models;

namespace Microsoft.Azure.Devices.Applications.RemoteMonitoring.DeviceAdmin.Web.Helpers
{
    public interface ICellularExtensions
    {
        List<Iccid> GetTerminals();
        Terminal GetSingleTerminalDetails(Iccid iccid);
        bool ValidateCredentials();
        List<SessionInfo> GetSingleSessionInfo(Iccid iccid);
        IEnumerable<string> GetListOfAvailableIccids(IList<DeviceModel> devices, string providerName);
        IEnumerable<string> GetListOfAvailableDeviceIDs(IList<DeviceModel> devices);
        IEnumerable<string> GetListOfConnectedDeviceIds(IList<DeviceModel> devices);
        List<SimState> GetAllAvailableSimStates(string iccid, string currentState);
        List<SimState> GetValidTargetSimStates(string currentState);
        List<SubscriptionPackage> GetAvailableSubscriptionPackages(string currentSubscriptionPackageName);
        Task<bool> UpdateSimState(string iccid, string updatedState);
        Task<bool> UpdateSubscriptionPackage(string iccid, string updatedPackage);
        Task<bool> ReconnectDevice(string iccid);
        Task<bool> SendSms(string iccid, string smsText);
    }
}