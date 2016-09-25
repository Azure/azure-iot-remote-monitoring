using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DeviceManagement.Infrustructure.Connectivity.Models.TerminalDevice;

namespace Microsoft.Azure.Devices.Applications.RemoteMonitoring.DeviceAdmin.Infrastructure.Repository
{
    public interface IIccidRepository
    {
        bool AddIccid(Iccid iccid);
        bool AddIccids(List<Iccid> iccids);
        bool RemoveAllIccids();
        bool GetIccids();
    }
}
