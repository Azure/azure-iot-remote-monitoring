using System;

namespace DeviceManagement.Infrustructure.Connectivity.Models.TerminalDevice
{
    public class SessionInfo
    {
        public Iccid Iccid { get; set; }
        public DateTime? DateSessionEnded { get; set; }
        public DateTime? DateSessionStarted { get; set; }
        public string IpAddress { get; set; }
    }
}