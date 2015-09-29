using System.Web.Mvc;
using GlobalResources;
using Microsoft.Azure.Devices.Applications.RemoteMonitoring.Common.Configurations;

namespace Microsoft.Azure.Devices.Applications.RemoteMonitoring.DeviceAdmin.Web.Helpers
{
    public static class HeaderHelper
    {
        public static string GetHeaderTitle()
        {
            var config = DependencyResolver.Current.GetService<IConfigurationProvider>();
            var defaultSolutionName = Strings.DefaultSolutionName;
            string solutionName = config.GetConfigurationSettingValueOrDefault("SolutionName", defaultSolutionName);
            return solutionName;
        }
    }
}