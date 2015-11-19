using System;
using DeviceManagement.Infrustructure.Connectivity.com.jasperwireless.spark.billing;
using DeviceManagement.Infrustructure.Connectivity.Models.TerminalDevice;

namespace DeviceManagement.Infrustructure.Connectivity.Proxies
{
    public interface IJasperBillingClientProxy
    {
        GetTerminalUsageResponse GetTerminalUsage(Iccid iccid, DateTime cycleStartDate);
        GetTerminalUsageDataDetailsResponse GetTerminalUsageDataDetails(Iccid iccid, DateTime cycleStartDate);
        GetTerminalUsageSmsDetailsResponse GetTerminalUsageSmsDetails(Iccid iccid, DateTime cycleStartDate);
        GetTerminalUsageVoiceDetailsResponse GetTerminalUsageVoiceDetails(Iccid iccid, DateTime cycleStartDate);
    }
}