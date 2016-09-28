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
        SimState GetCurrentSimState();
        List<SimState> GetAvailableSimStates();
        SubscriptionPackage GetCurrentSubscriptionPackage();
        List<SubscriptionPackage> GetAvailableSubscriptionPackages();
        bool UpdateSimState(DeviceModel device);
        bool UpdateSubscriptionPackage(DeviceModel device);
        bool ReconnectDevice(DeviceModel device);
        bool SendSms(DeviceModel device);
    }
}