using System.Collections.Generic;
using DeviceManagement.Infrustructure.Connectivity.Models.TerminalDevice;

namespace DeviceManagement.Infrustructure.Connectivity.Services
{
    /// <summary>
    ///     Temp interface structure for implementing resources that are required from the IoT Suite
    /// </summary>
    public interface IExternalCellularService
    {
        List<Iccid> GetTerminals();
        Terminal GetSingleTerminalDetails(Iccid iccid);
        List<SessionInfo> GetSingleSessionInfo(Iccid iccid);    
    }
}