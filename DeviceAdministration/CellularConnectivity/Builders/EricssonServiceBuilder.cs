using System;
using System.Runtime.Serialization;
using System.ServiceModel;
using System.ServiceModel.Channels;
using DeviceManagement.Infrustructure.Connectivity.DeviceReconnect;
using DeviceManagement.Infrustructure.Connectivity.EricssonApiService;
using DeviceManagement.Infrustructure.Connectivity.EricssonSubscriptionService;
using DeviceManagement.Infrustructure.Connectivity.EricssonTrafficManagment;
using DeviceManagement.Infrustructure.Connectivity.Models.Security;

namespace DeviceManagement.Infrustructure.Connectivity.Builders
{
    public class EricssonServiceBuilder
    {

        public static ApiStatusClient GetApiStatusClient(ICredentials credentials)
        {
            // create custom auth endpoint and also programatically add bindings (you need to do this for each type of client)
            var endpointAddress = GetAuthorizedEndpoint(credentials, $"{credentials.BaseUrl}/dcpapi/ApiStatus");
            var binding = GetBasicHttpBinding();
            //end

            return new ApiStatusClient(binding, endpointAddress);
        }

        public static SubscriptionManagementClient GetSubscriptionManagementClient(ICredentials credentials)
        {
            // create custom auth endpoint and also programatically add bindings (you need to do this for each type of client)
            var endpointAddress = GetAuthorizedEndpoint(credentials, $"{credentials.BaseUrl}/dcpapi/SubscriptionManagement");
            var binding = GetBasicHttpBinding();
            //end

            return new SubscriptionManagementClient(binding, endpointAddress);
        }

        public static SubscriptionTrafficClient GetSubscriptionTrafficClient(ICredentials credentials)
        {
            var endpointAddress = GetAuthorizedEndpoint(credentials, $"{credentials.BaseUrl}/dcpapi/SubscriptionTraffic");
            var binding = GetBasicHttpBinding();
            return new SubscriptionTrafficClient(binding, endpointAddress);
        }

        public static DeviceReconnectClient GetDeviceReconnectClient(ICredentials credentials)
        {
            // create custom auth endpoint and also programatically add bindings (you need to do this for each type of client)
            var endpointAddress = GetAuthorizedEndpoint(credentials, $"{credentials.BaseUrl}/dcpapi/DeviceReconnect");
            var binding = GetBasicHttpBinding();
            //end

            return new DeviceReconnectClient(binding, endpointAddress);
        }

        //STATIC HELPERS FOR THIS CLASS
        private static EndpointAddress GetAuthorizedEndpoint(ICredentials credentials, string endpointUrl)
        {
            //Create wsse security object
            var usernameToken = new EricssonUsernameToken { Password = credentials.Password, Username = credentials.Username };
            var security = new EricssonSecurity { UsernameToken = usernameToken };

            //Serialize object to xml
            var xmlObjectSerializer = new DataContractSerializer(typeof(EricssonSecurity), "Security", "");

            //Create address header with security header
            var addressHeader = AddressHeader.CreateAddressHeader("Security", "http://docs.oasis-open.org/wss/2004/01/oasis-200401-wss-wssecurity-secext-1.0.xsd", security, xmlObjectSerializer);
            return new EndpointAddress(new Uri(endpointUrl), new[] { addressHeader });
        }

        private static BasicHttpBinding GetBasicHttpBinding()
        {
            return new BasicHttpBinding { Security = { Mode = BasicHttpSecurityMode.Transport } };
        }
    }
}
