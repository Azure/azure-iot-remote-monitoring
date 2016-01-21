using System;
using System.ServiceModel.Security;
using System.Diagnostics;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Client;
using CrmTypes = Microsoft.Crm.Sdk.Types;
using Microsoft.Azure.Devices.Applications.RemoteMonitoring.Common.Configurations;

namespace Microsoft.Crm.Sdk.Helper
{
    public static class CrmActionProcessor
    {
        static int count = 1;

        private static IOrganizationService GetOrgService(ServerConnection serverConnection, bool reAuthenticate = false)
        {
            OrganizationServiceProxy serviceProxy;
            ServerConnection.Configuration serverConfig = serverConnection.GetServerConfiguration(reAuthenticate);

            // Connect to the Organization service. 
            // The using statement assures that the service proxy will be properly disposed.
            using (serviceProxy = new OrganizationServiceProxy(serverConfig.OrganizationUri, serverConfig.HomeRealmUri, serverConfig.Credentials, serverConfig.DeviceCredentials))
            {
                // This statement is required to enable early-bound type support.
                serviceProxy.EnableProxyTypes();
                return (IOrganizationService)serviceProxy;
            }
        }

        private static IOrganizationService TryGetOrgService(IConfigurationProvider configurationProvider)
        {
            IOrganizationService service;

            ServerConnection serverConnection = ServerConnection.Get(configurationProvider);
            try
            {
                service = GetOrgService(serverConnection);
            }
            catch (ExpiredSecurityTokenException ex)
            {
                Trace.TraceError("Server connection terminated with an expired token error.");
                Trace.TraceError(ex.Message);

                // Try again with reauthentication.
                service = GetOrgService(serverConnection, true);
            }

            return service;
        }

        public static void CreateServiceAlert(IConfigurationProvider configurationProvider,
                                        Guid eventToken, string deviceId, string actionId)
        {
            Trace.TraceInformation("ActionProcessor: In CreateAlert V3");
            
            IOrganizationService service = TryGetOrgService(configurationProvider);
            
            EntityReference asset = new EntityReference(CrmTypes.f1_customerasset.EntityLogicalName,
                                                        new Guid(deviceId));

            CrmTypes.new_servicealert serviceAlert = new CrmTypes.new_servicealert
            {
                new_name = String.Format("Az_SampleAlert {0} {1}", actionId, count.ToString()),
                new_AlertToken = eventToken.ToString(),
                new_Asset = asset
            };

            service.Create(serviceAlert);
            count++;
        }
    }
}
