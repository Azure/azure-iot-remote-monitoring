using System;
using System.IdentityModel.Tokens;
using Microsoft.Azure.Devices.Applications.RemoteMonitoring.Common.Configurations;
using Microsoft.Owin.Security;
using Microsoft.Owin.Security.ActiveDirectory;
using Microsoft.Owin.Security.Cookies;
using Microsoft.Owin.Security.WsFederation;
using Owin;

namespace Microsoft.Azure.Devices.Applications.RemoteMonitoring.DeviceAdmin.Web
{
    public partial class Startup
    {
        public void ConfigureAuth(IAppBuilder app, IConfigurationProvider configProvider)
        {
            app.SetDefaultSignInAsAuthenticationType(CookieAuthenticationDefaults.AuthenticationType);

            // Primary authentication method for web site to Azure AD via the WsFederation below
            app.UseCookieAuthentication(new CookieAuthenticationOptions());

            string federationMetadataAddress = configProvider.GetConfigurationSettingValue("ida.FederationMetadataAddress");
            string federationRealm = configProvider.GetConfigurationSettingValue("ida.FederationRealm");

            if (string.IsNullOrEmpty(federationMetadataAddress) || string.IsNullOrEmpty(federationRealm))
            {
                throw new ApplicationException("Config issue: Unable to load required federation values from web.config or other configuration source.");
            }

            // check for default values that will cause app to fail to startup with an unhelpful 404 exception
            if (federationMetadataAddress.StartsWith("-- ", StringComparison.Ordinal) || 
                federationRealm.StartsWith("-- ", StringComparison.Ordinal))
            {
                throw new ApplicationException("Config issue: Default federation values from web.config need to be overridden or replaced.");
            }

            app.UseWsFederationAuthentication(
                new WsFederationAuthenticationOptions
                {
                    MetadataAddress = federationMetadataAddress,
                    Wtrealm = federationRealm
                });

            string aadTenant = configProvider.GetConfigurationSettingValue("ida.AADTenant");
            string aadAudience = configProvider.GetConfigurationSettingValue("ida.AADAudience");

            if (string.IsNullOrEmpty(aadTenant) || string.IsNullOrEmpty(aadAudience))
            {
                throw new ApplicationException("Config issue: Unable to load required AAD values from web.config or other configuration source.");
            }

            // check for default values that will cause failure
            if (aadTenant.StartsWith("-- ", StringComparison.Ordinal) || 
                aadAudience.StartsWith("-- ", StringComparison.Ordinal))
            {
                throw new ApplicationException("Config issue: Default AAD values from web.config need to be overridden or replaced.");
            }

            // Fallback authentication method to allow "Authorization: Bearer <token>" in the header for WebAPI calls
            app.UseWindowsAzureActiveDirectoryBearerAuthentication(
                new WindowsAzureActiveDirectoryBearerAuthenticationOptions
                {
                    Tenant = aadTenant,
                    TokenValidationParameters = new TokenValidationParameters
                    {
                        ValidAudience = aadAudience,
                        RoleClaimType = "http://schemas.microsoft.com/identity/claims/scope" // Used to unwrap token roles and provide them to [Authorize(Roles="")] attributes
                    },
                });
        }
    }
}