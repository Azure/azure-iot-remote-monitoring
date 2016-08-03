using System.Collections.Generic;
using DeviceManagement.Infrustructure.Connectivity.Models.TerminalDevice;
using Microsoft.Azure.Devices.Applications.RemoteMonitoring.DeviceAdmin.Web.Models;

namespace DeviceManagement.Infrustructure.Connectivity.Services
{
    /// <summary>
    ///     Temp interface structure for implementing resources that are required from the IoT Suite
    /// </summary>
    public interface IExternalCellularService
    {
        List<Iccid> GetTerminals(CellularProviderEnum cellularProvider);
        bool ValidateCredentials(CellularProviderEnum cellularProvider);
        Terminal GetSingleTerminalDetails(Iccid iccid, CellularProviderEnum cellularProvider);
        List<SessionInfo> GetSingleSessionInfo(Iccid iccid, CellularProviderEnum cellularProvider);    
    }
}