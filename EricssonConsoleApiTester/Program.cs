using System;
using System.Net;
using System.Runtime.Serialization;
using System.ServiceModel;
using System.ServiceModel.Channels;
using System.ServiceModel.Description;
using System.Web.Services.Protocols;
using EricssonConsoleApiTester.ApiStatus;
using EricssonConsoleApiTester.SubscriptionManagement;

namespace EricssonConsoleApiTester
{
    class Program
    {
        static void Main(string[] args)
        {
            //echo tester

            var binding = new BasicHttpBinding {Security = {Mode = BasicHttpSecurityMode.Transport}};
            EndpointAddress endpointAddress = EricssonEndpointBuilder.GetAuthorizedEndpoint("https://serviceportal.telenorconnexion.com/dcpapi/ApiStatus");

            var apiStatusClient = new ApiStatusClient(binding, endpointAddress);

            try
            {
                var response1 = apiStatusClient.echo(new echo());
            }
            catch (Exception)
            {

                throw;
            }



            //sub tester -get a single sim information
            var subscriptionManClient = new SubscriptionManagementClient();
            subscriptionManClient.Endpoint.Address =
                EricssonEndpointBuilder.GetAuthorizedEndpoint(
                    "https://serviceportal.telenorconnexion.com/dcpapi/SubscriptionManagement");

            //var response2 = subscriptionManClient.QuerySimResource(new QuerySimResource()
            //{
            //    resource = new resource()
            //    {
            //        id = "89460800000105696001",
            //        type = "icc"
            //    }

            //});

   


            var resp = subscriptionManClient.QuerySubscriptions(new QuerySubscriptionsRequest()
            {
                maxResults = 10,
                resource = new resource()
                {
                    id = "240080000569600",
                    type = "imsi"
                }

            });



        }

    }
}

