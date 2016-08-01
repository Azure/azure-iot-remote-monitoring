using System.Collections.Generic;

namespace DeviceManagement.Infrustructure.Connectivity.Models.Billing
{
    public class DataUsage
    {
        public int AvailableSessions { get; set; }
        public List<DataUsageDetail> DataUsageDetails { get; set; }
    }
}