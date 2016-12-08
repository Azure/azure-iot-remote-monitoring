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
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;
using System.Web.Script.Serialization;
using DeviceManagement.Infrustructure.Connectivity.Models.Jasper;
using Newtonsoft.Json;

namespace EricssonConsoleApiTester
{
    class Program
    {
        public static async Task<bool> SendSMS(string msisdn, string messageContent)
        {
            var creds = new EricssonCredentials()
            {
                Username = "everest.demo1234@gmail.com",
                Password = "demDCP12345",
                BaseUrl = "https://orange.dcp.ericsson.net/",
                LicenceKey = "",
                ApiRegistrationProvider = "Ericsson",
                EnterpriseSenderNumber = "33604",
                RegistrationID = "test",
                SmsEndpointBaseUrl = "exposureapi.dcp.ericsson.net"
            };
            var senderAddress = creds.EnterpriseSenderNumber;
            var smsEndpointBaseUrl = creds.SmsEndpointBaseUrl;
            var basicAuthPassword = Base64Encode($"{creds.Username}:{creds.Password}");
            if (string.IsNullOrWhiteSpace(senderAddress) || string.IsNullOrWhiteSpace(smsEndpointBaseUrl))
            {
                throw new ApplicationException("You have not provided an EnterpriseSenderAddress and/or a SmsEndpointBaseUrl");
            }
            var requestBodyModel = new SendSmsRequest()
            {
                outboundSMSMessageRequest = new Outboundsmsmessagerequest()
                {
                    address = new string[] { $"tel:{msisdn}" },
                    senderAddress = $"tel:{senderAddress}",
                    outboundSMSTextMessage = new Outboundsmstextmessage()
                    {
                        message = messageContent
                    }
                }
            };
            var requestBody = new JavaScriptSerializer().Serialize(requestBodyModel);
            var content = new StringContent(requestBody, Encoding.UTF8, "application/json");
            var endpointUrl = $"https://{smsEndpointBaseUrl}/dcpapi/smsmessaging/v1/outbound/tel:{senderAddress}/requests";

            using (var client = new HttpClient())
            {
                client.DefaultRequestHeaders.Accept.Clear();
                client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
                client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Basic", basicAuthPassword);
                client.DefaultRequestHeaders.Add("Connection", "keep-alive");
                client.DefaultRequestHeaders.Host = $"{smsEndpointBaseUrl}:80";
                var httpRequestMessage = new HttpRequestMessage(HttpMethod.Post, endpointUrl)
                {
                    Version = HttpVersion.Version10,
                    Content = content
                };
                try
                {
                    var response = await client.SendAsync(httpRequestMessage);
                }
                catch (Exception exception)
                {
                    Console.WriteLine("error");
                }
            }

            return true;
        }

        static void Main(string[] args)
        {
            Task.Run(async () =>
            {
                var result = await SendSMS("883130200000456", "Test");
                Console.WriteLine(JsonConvert.SerializeObject(result));
            }).Wait();

            Console.WriteLine("yay");
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

        private class SendSmsRequest
        {
            public Outboundsmsmessagerequest outboundSMSMessageRequest { get; set; }
        }

        private class Outboundsmsmessagerequest
        {
            public string[] address { get; set; }
            public string senderAddress { get; set; }
            public Outboundsmstextmessage outboundSMSTextMessage { get; set; }
            public string senderName { get; set; }
        }

        private class Outboundsmstextmessage
        {
            public string message { get; set; }
        }

        private static string Base64Encode(string plainText)
        {
            var plainTextBytes = System.Text.Encoding.UTF8.GetBytes(plainText);
            return System.Convert.ToBase64String(plainTextBytes);
        }

    }
}

