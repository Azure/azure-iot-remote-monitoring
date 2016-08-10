using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.ServiceModel;
using System.ServiceModel.Channels;
using System.Text;
using System.Threading.Tasks;

namespace EricssonConsoleApiTester
{
    public static class EricssonEndpointBuilder
    {
        public static EndpointAddress GetAuthorizedEndpoint(string endpointUrl)
        {
            //Create wsse security object
            var usernameToken = new UsernameToken { Password = "TesTing2345", Username = "demo.user18@ericsson.com" };
            var security = new EricssonSecurity { UsernameToken = usernameToken };

            //Serialize object to xml
            var xmlObjectSerializer = new DataContractSerializer(typeof(EricssonSecurity), "Security", "");

            //Create address header with security header
            var addressHeader = AddressHeader.CreateAddressHeader("Security", "http://docs.oasis-open.org/wss/2004/01/oasis-200401-wss-wssecurity-secext-1.0.xsd", security, xmlObjectSerializer);
            return new EndpointAddress(new Uri(endpointUrl), new[] { addressHeader });
        }

    }
}
