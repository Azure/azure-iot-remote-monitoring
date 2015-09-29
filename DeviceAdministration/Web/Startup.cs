using System.Web.Http;
using Microsoft.Azure.Devices.Applications.RemoteMonitoring.Common.Configurations;
using Owin;

namespace Microsoft.Azure.Devices.Applications.RemoteMonitoring.DeviceAdmin.Web
{
    public partial class Startup
    {
        public static HttpConfiguration HttpConfiguration { get; private set; }

        public void Configuration(IAppBuilder app)
        {
            Startup.HttpConfiguration = new System.Web.Http.HttpConfiguration();
            ConfigurationProvider configProvider = new ConfigurationProvider();

            ConfigureAuth(app, configProvider);
            ConfigureAutofac(app);

            // WebAPI call must come after Autofac
            // Autofac hooks into the HttpConfiguration settings
            ConfigureWebApi(app);   

            ConfigureJson(app);
        }
    }
}