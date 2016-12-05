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
using System.Net.Http;
using System.Collections.Generic;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace EricssonConsoleApiTester
{
    class Program
    {
        static async Task<HttpResponseMessage> SendSMS()
        {
            using (var client = new HttpClient())
            {
                var values = new Dictionary<string, string>
                {
                };

                var content = new FormUrlEncodedContent(values);

                return await client.PostAsync("https://<https://orange.dcp.ericsson.net/dcpapi/smsmessaging/v1/outbound/tel: 33604/requests", content);
            }
        }

        static void Main(string[] args)
        {
            //var subscriptionTrafficClient = new SubscriptionTrafficClient();
            //subscriptionTrafficClient.Endpoint.Address =
            //    EricssonEndpointBuilder.GetAuthorizedEndpoint(
            //        "https://orange.dcp.ericsson.net/dcpapi/SubscriptionTraffic ");

            //var response = subscriptionTrafficClient.query(new query()
            //{
            //    resource = new SubscriptionTraffic.resource()
            //    {
            //        id = "901312000000466",
            //        type = resourceType.imsi
            //    }
            //});
            //Console.WriteLine("Done");

            //Task.Run(async () =>
            //{
            //    var response = await SendSMS();
            //    Console.WriteLine(JsonConvert.SerializeObject(response));
            //}).Wait();


            ////echo tester

            //var binding = new BasicHttpBinding { Security = { Mode = BasicHttpSecurityMode.Transport } };
            //EndpointAddress endpointAddress = EricssonEndpointBuilder.GetAuthorizedEndpoint("https://orange.dcp.ericsson.net/dcpapi/ApiStatus");

            ////SUBSCRIPTION MANAGEMENT
            //var subscriptionManClient = new SubscriptionManagementClient();
            //subscriptionManClient.Endpoint.Address =
            //    EricssonEndpointBuilder.GetAuthorizedEndpoint(
            //        "https://orange.dcp.ericsson.net/dcpapi/SubscriptionManagement");

            //var response1 = subscriptionManClient.QuerySimResource(new QuerySimResource()
            //{
            //    resource = new SubscriptionManagement.resource()
            //    {
            //        id = "89883011539830007560",
            //        type = "icc"
            //    }
            //});

            //var response1 = subscriptionManClient.QuerySubscriptions(new QuerySubscriptionsRequest()
            //{
            //    maxResults= 10,
            //    resource = new SubscriptionManagement.resource()
            //    {
            //        id = "901312000000466",
            //        type = "imsi"
            //    }
            //});



            //var reconnectClient = new DeviceReconnectClient();
            //reconnectClient.Endpoint.Address =
            //    EricssonEndpointBuilder.GetAuthorizedEndpoint(
            //        "https://orange.dcp.ericsson.net/dcpapi/DeviceReconnect");

            //reconnectClient.reconnect(new Reconnect()
            //{
            //    resource = new Resource()
            //    {
            //        id = "89883011539830007560",
            //        type = ResourceType.icc
            //    }
            //});

            //var subscriptionQuery = subscriptionManClient.QuerySubscriptionPackages(new QuerySubscriptionPackages());

            //var subscriptionStatusChangeResult = subscriptionManClient.RequestSubscriptionStatusChange(new RequestSubscriptionStatusChange()
            //{
            //    resource = new SubscriptionManagement.resource()
            //    {
            //        id = "89883011539830007560",
            //        type = "icc"
            //    },
            //    subscriptionStatus = subscriptionStatusRequest.Activate
            //});

            //QuerySubscriptionStatusChangeResponse subscriptionStatusChangeReqeustStatus =
            //    subscriptionManClient.QuerySubscriptionStatusChange(new QuerySubscriptionStatusChange()
            //    {
            //        serviceRequestId = subscriptionStatusChangeResult.serviceRequestId
            //    });

            //while (subscriptionStatusChangeReqeustStatus.statusRequestResponse != statusRequestResponse.Completed &&
            //       subscriptionStatusChangeReqeustStatus.statusRequestResponse != statusRequestResponse.Rejected)
            //{
            //    subscriptionStatusChangeReqeustStatus =
            //    subscriptionManClient.QuerySubscriptionStatusChange(new QuerySubscriptionStatusChange()
            //    {
            //        serviceRequestId = subscriptionStatusChangeResult.serviceRequestId
            //    });
            //    System.Threading.Thread.Sleep(20000);
            //}

            //var ericssonSmsClient = new EricssonSmsClient("https://serviceportal.telenorconnexion.com/dcpapi/smsmessaging/v1/outbound/<senderAddress>/requests");

            //ericssonSmsClient.DownloadPageAsync();

            //var resp = subscriptionManClient.QuerySubscriptions(new QuerySubscriptionsRequest()
            //{
            //    maxResults = 10,
            //    resource = new SubscriptionManagement.resource()
            //    {
            //        id = "901312000000466",
            //        type = "imsi"
            //    }
            //});

            ////Subscription Traffic - potential for activity.
            //var subscriptionTrafficClient = new SubscriptionTrafficClient();
            //subscriptionTrafficClient.Endpoint.Address = EricssonEndpointBuilder.GetAuthorizedEndpoint(
            //        "https://orange.dcp.ericsson.net/dcpapi/SubscriptionTraffic");

            //queryResponse subResp = subscriptionTrafficClient.query(new query()
            //{
            //    resource = new SubscriptionTraffic.resource()
            //    {
            //        id = "901312000000466",
            //        type = resourceType.imsi
            //    }
            //});
            //Console.WriteLine("Hello");

            ////Realtime Trace - PDP? HLR?
            //var realTimeClient = new RealtimeTraceClient();
            //realTimeClient.Endpoint.Address = EricssonEndpointBuilder.GetAuthorizedEndpoint(
            //    "https://orange.dcp.ericsson.net/dcpapi/RealtimeTrace");

            //var traceResp = realTimeClient.query(new Query()
            //{
            //    traceLength = 5,
            //    filter = new Filter()
            //    {
            //        resourceFilter = new ResourceFilter()
            //        {
            //            resourceId = "901312000000466",
            //            resourceType = "imsi",
            //            range = 5
            //        }
            //    }
            //});

            //var subscriptionManClient = new SubscriptionManagementClient();
            //subscriptionManClient.Endpoint.Address =
            //    EricssonEndpointBuilder.GetAuthorizedEndpoint(
            //        "https://orange.dcp.ericsson.net/dcpapi/SubscriptionManagement");

            //QuerySimResourceResponse response1 = subscriptionManClient.QuerySimResource(new QuerySimResource()
            //{
            //    resource = new SubscriptionManagement.resource()
            //    {
            //        id = "89883011539830007560",
            //        type = "icc"
            //    }
            //});

        }

    }
}

