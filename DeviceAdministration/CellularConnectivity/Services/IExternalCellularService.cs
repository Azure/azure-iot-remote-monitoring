using System.Collections.Generic;
using System.Threading.Tasks;
using DeviceManagement.Infrustructure.Connectivity.Models.Other;
using DeviceManagement.Infrustructure.Connectivity.Models.TerminalDevice;

namespace DeviceManagement.Infrustructure.Connectivity.Services
{
    /// <summary>
    ///     Temp interface structure for implementing resources that are required from the IoT Suite
    /// </summary>
    public interface IExternalCellularService
    {
        List<Iccid> GetTerminals();
        bool ValidateCredentials();
        Terminal GetSingleTerminalDetails(Iccid iccid);
        List<SessionInfo> GetSingleSessionInfo(Iccid iccid);
        SimState GetCurrentSimState(string iccid);
        List<SimState> GetAvailableSimStates(string iccid);
        List<SubscriptionPackage> GetAvailableSubscriptionPackages();
        Task<bool> UpdateSimState(string iccid, string updatedState);
        Task<bool> UpdateSubscriptionPackage(string iccid, string updatedPackage);
        Task<bool> ReconnectTerminal(string iccid);
        Task<bool> SendSms(string iccid, string smsText);
    }
}