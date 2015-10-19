using System.Diagnostics;

namespace Microsoft.Azure.Devices.Applications.RemoteMonitoring.DeviceAdmin.Web
{
    public class MvcApplication : System.Web.HttpApplication
    {
        protected void Application_Start()
        {
            // Do nothing else here, need application class for host.
            Trace.TraceInformation("WebJobHost starting...");
        }
    }
}
