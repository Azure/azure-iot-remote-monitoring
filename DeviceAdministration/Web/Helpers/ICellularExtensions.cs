using System.Collections.Generic;
using System.Threading.Tasks;
using DeviceManagement.Infrustructure.Connectivity.Models.Other;
using DeviceManagement.Infrustructure.Connectivity.Models.TerminalDevice;
using Microsoft.Azure.Devices.Applications.RemoteMonitoring.Common.Models;

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
        List<SimState> GetValidTargetSimStates(string iccid, string currentState);
        List<SubscriptionPackage> GetAvailableSubscriptionPackages(string iccid, string currentSubscription);
        bool UpdateSimState(string iccid, string updatedState);
        bool UpdateSubscriptionPackage(string iccid, string updatedPackage);
        bool ReconnectDevice(string iccid);
        Task<bool> SendSms(string iccid, string smsText);
        string GetLocale(string iccid, out IEnumerable<string> availableLocaleNames);
        bool SetLocale(string iccid, string localeName);
        string GetLastSetLocaleServiceRequestState(string iccid);
    }
}