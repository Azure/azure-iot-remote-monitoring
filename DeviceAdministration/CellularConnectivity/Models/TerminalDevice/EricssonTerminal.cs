using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DeviceManagement.Infrustructure.Connectivity.Models.TerminalDevice
{
    public class EricssonTerminal : Terminal
    {
        public string PriceProfileName { get; set; }
        public string PdpContextProfileName { get; set; }
    }
}
