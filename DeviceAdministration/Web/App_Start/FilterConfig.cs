using System.Web.Mvc;
using Microsoft.Azure.Devices.Applications.RemoteMonitoring.DeviceAdmin.Web.Filters;

namespace Microsoft.Azure.Devices.Applications.RemoteMonitoring.DeviceAdmin.Web
{
    public class FilterConfig
    {
        public static void RegisterGlobalFilters(GlobalFilterCollection filters)
        {
            filters.Add(new HandleErrorAttribute());
            filters.Add(new ErrorHandlingFilter());
        }
    }
}
