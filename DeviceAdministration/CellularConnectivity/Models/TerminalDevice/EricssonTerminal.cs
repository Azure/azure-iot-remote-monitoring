using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DeviceManagement.Infrustructure.Connectivity.Models.TerminalDevice
{
    public class EricssonTerminal : Terminal
    {
        public string OperatorCode { get; set; }
        public string CountryCode { get; set; }
        public string LastData { get; set; }
        public string LastPDPContext { get; set; }
        public string PriceProfileName { get; set; }
        public string PdpContextProfileName { get; set; }
        public string PricePlan { get; set; }
        public string AggregatedSimUsage { get; set; }
        public string CurrentLimits { get; set; }
    }
}
