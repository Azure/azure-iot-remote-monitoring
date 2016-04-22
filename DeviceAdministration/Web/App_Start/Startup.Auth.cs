using Microsoft.Azure.Devices.Applications.RemoteMonitoring.Common.Configurations;
using Microsoft.Owin.Security;
using Microsoft.Owin.Security.Cookies;
using Owin;
using System.Configuration;
using Microsoft.Owin.Security.OpenIdConnect;
using System.Globalization;
using System.Threading.Tasks;

namespace Microsoft.Azure.Devices.Applications.RemoteMonitoring.DeviceAdmin.Web
{
    public partial class Startup
    {
        public void ConfigureAuth(IAppBuilder app, IConfigurationProvider configProvider)
        {
            string aadClientId = configProvider.GetConfigurationSettingValue("ida.AADClientId");
            string aadInstance = configProvider.GetConfigurationSettingValue("ida.AADInstance");
            string aadTenant = configProvider.GetConfigurationSettingValue("ida.AADTenant");
            string authority = string.Format(CultureInfo.InvariantCulture, aadInstance, aadTenant);

            app.SetDefaultSignInAsAuthenticationType(CookieAuthenticationDefaults.AuthenticationType);

            app.UseCookieAuthentication(new CookieAuthenticationOptions());

            app.UseOpenIdConnectAuthentication(
                new OpenIdConnectAuthenticationOptions
                {
                    ClientId = aadClientId,
                    Authority = authority,
                    Notifications = new OpenIdConnectAuthenticationNotifications
                    {
                        AuthenticationFailed = context =>
                        {
                            string appBaseUrl = context.Request.Scheme + "://" + context.Request.Host + context.Request.PathBase;

                            context.ProtocolMessage.RedirectUri = appBaseUrl + "/";
                            context.HandleResponse();
                            context.Response.Redirect(context.ProtocolMessage.RedirectUri);

                            return Task.FromResult(0);
                        }
                    }
                });
        }
    }
}