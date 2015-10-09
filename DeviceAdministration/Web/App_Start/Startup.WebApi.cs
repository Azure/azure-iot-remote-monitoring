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

            // WebApi end points can return JSON.net instances or ones of types 
            // that contain them.  These do not work with the all formatters--they 
            // have issues with the DataContractSerializer.  Use JSON formatting only.
            Startup.HttpConfiguration.Formatters.Clear();
            Startup.HttpConfiguration.Formatters.Add(new JsonMediaTypeFormatter());
        }
    }
}