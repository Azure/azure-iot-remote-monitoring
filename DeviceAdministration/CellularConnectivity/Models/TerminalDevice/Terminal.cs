using System;

namespace DeviceManagement.Infrustructure.Connectivity.Models.TerminalDevice
{
    public class Terminal
    {
        public Terminal()
        {
            Status = "Unknown";
        }

        public bool OverageLimitReached { get; set; }
        public string Status { get; set; }
        public decimal MonthToDateDataUsage { get; set; }
        public string RatePlan { get; set; }
        public long AccountId { get; set; }
        public Iccid Iccid { get; set; }
        public Imei Imei { get; set; }
        public Imsi Imsi { get; set; }
        public Msisdn Msisdn { get; set; }
        public DateTime DateOfActivation { get; set; }
    }
}