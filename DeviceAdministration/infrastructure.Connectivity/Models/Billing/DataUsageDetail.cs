using System;

namespace DeviceManagement.Infrustructure.Connectivity.Models.Billing
{
    public class DataUsageDetail
    {
        public decimal DataVolume { get; set; }
        public long Duration { get; set; }
        public DateTime SessionStartTime { get; set; }
    }
}