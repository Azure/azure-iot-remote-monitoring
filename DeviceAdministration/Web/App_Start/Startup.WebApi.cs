using System.Web.Http;
﻿using System.Net.Http.Formatting;
using Owin;

namespace Microsoft.Azure.Devices.Applications.RemoteMonitoring.DeviceAdmin.Web
{
    public partial class Startup
    {
        public void ConfigureWebApi(IAppBuilder app)
        {
            app.UseWebApi(Startup.HttpConfiguration);
            Startup.HttpConfiguration.MapHttpAttributeRoutes();
        }
    }
}