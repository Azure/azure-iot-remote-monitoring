using System.Collections.Generic;
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
        bool ReconnectDevice(DeviceModel device);
        Iccid GetAssociatedDeviceTerminalIccid(IList<DeviceModel> allDevices, string deviceId);
        SimStateModel GetCurrentSimState();
        List<SimStateModel> GetAvailableSimStates();
        SubscriptionPackageModel GetCurrentSubscriptionPackage();
        List<SubscriptionPackageModel> GetAvailableSubscriptionPackages();
    }
}