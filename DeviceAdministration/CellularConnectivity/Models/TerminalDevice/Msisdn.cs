using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DeviceManagement.Infrustructure.Connectivity.Models.TerminalDevice
{
    public class Msisdn
    {
        public Msisdn()
        {
        }

        public Msisdn(string id)
        {
            Id = id;
        }

        public string Id
        {
            get; set;
        }
    }
}
