using System;
using System.Net;
using System.Runtime.Serialization;
using System.ServiceModel;
using System.ServiceModel.Channels;
using System.ServiceModel.Description;
using System.Web.Services.Protocols;
using EricssonConsoleApiTester.ApiStatus;
using EricssonConsoleApiTester.RealtimeTrace;
using EricssonConsoleApiTester.SubscriptionManagement;
using EricssonConsoleApiTester.SubscriptionTraffic;

namespace EricssonConsoleApiTester
{
    class Program
    {
        static void Main(string[] args)
        {
            //echo tester

            var binding = new BasicHttpBinding { Security = { Mode = BasicHttpSecurityMode.Transport } };
            EndpointAddress endpointAddress = EricssonEndpointBuilder.GetAuthorizedEndpoint("https://orange.dcp.ericsson.net/dcpapi/ApiStatus");



            //SUBSCRIPTION MANAGEMENT
            var subscriptionManClient = new SubscriptionManagementClient();
            subscriptionManClient.Endpoint.Address =
                EricssonEndpointBuilder.GetAuthorizedEndpoint(
                    "https://orange.dcp.ericsson.net/dcpapi/SubscriptionManagement");

            var response1 = subscriptionManClient.QuerySimResource(new QuerySimResource()
            {
                resource = new SubscriptionManagement.resource()
                {
                    id = "89883011539830007560",
                    type = "icc"
                }

            });

            var resp = subscriptionManClient.QuerySubscriptions(new QuerySubscriptionsRequest()
            {
                maxResults = 10,
                resource = new SubscriptionManagement.resource()
                {
                    id = "901312000000466",
                    type = "imsi"
                }
            });


            //Subscription Traffic - potential for activity.
            var subscriptionTrafficClient = new SubscriptionTrafficClient();
            subscriptionTrafficClient.Endpoint.Address = EricssonEndpointBuilder.GetAuthorizedEndpoint(
                    "https://orange.dcp.ericsson.net/dcpapi/SubscriptionTraffic");

            var subResp = subscriptionTrafficClient.query(new query()
            {
                resource = new SubscriptionTraffic.resource()
                {
                    id = "901312000000466",
                    type = resourceType.imsi
                }

            });

            //Realtime Trace - PDP? HLR?
            var realTimeClient = new RealtimeTraceClient();
            realTimeClient.Endpoint.Address = EricssonEndpointBuilder.GetAuthorizedEndpoint(
                "https://orange.dcp.ericsson.net/dcpapi/RealtimeTrace");

            var traceResp = realTimeClient.query(new Query()
            {
                traceLength = 5,
                filter = new Filter()
                {
                    
                    resourceFilter = new ResourceFilter()
                    {
                        resourceId = "901312000000466",
                        resourceType = "imsi",
                        range = 5
                    }

                }

            });

        }

    }
}

