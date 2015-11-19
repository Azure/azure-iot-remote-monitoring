using DeviceManagement.Infrustructure.Connectivity.com.jasperwireless.spark.terminal;
using DeviceManagement.Infrustructure.Connectivity.Models.TerminalDevice;

namespace DeviceManagement.Infrustructure.Connectivity.Proxies
{
    public interface IJasperTerminalClientProxy
    {
        GetModifiedTerminalsResponse GetModifiedTerminals();
        GetTerminalDetailsResponse GetSingleTerminalDetails(Iccid iccidList);
        GetSessionInfoResponse GetSingleSessionInfo(Iccid iccid);
    }
}