using DeviceManagement.Infrustructure.Connectivity.Models.TerminalDevice;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DeviceManagement.Infrustructure.Connectivity.Services
{
    public interface IJasperCellularService
    {
        List<Iccid> GetTerminals();
        bool ValidateCredentials();
        Terminal GetSingleTerminalDetails(Iccid iccid);
        List<SessionInfo> GetSingleSessionInfo(Iccid iccid);
    }
}
