using System.Collections.Generic;
using DeviceManagement.Infrustructure.Connectivity.Models.TerminalDevice;
using DeviceManagement.Infrustructure.Connectivity.Models.Enums;

namespace DeviceManagement.Infrustructure.Connectivity.Services
{
    /// <summary>
    ///     Temp interface structure for implementing resources that are required from the IoT Suite
    /// </summary>
    public interface IExternalCellularService
    {
        List<Iccid> GetTerminals(ApiRegistrationProviderType registrationProvider);
        bool ValidateCredentials();
        Terminal GetSingleTerminalDetails(Iccid iccid, ApiRegistrationProviderType cellularProvider);
        List<SessionInfo> GetSingleSessionInfo(Iccid iccid, ApiRegistrationProviderType cellularProvider);
    }
}