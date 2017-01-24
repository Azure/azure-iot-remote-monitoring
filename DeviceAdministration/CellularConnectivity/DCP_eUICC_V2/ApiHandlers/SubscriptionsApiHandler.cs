using System;
using System.Text;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.Serialization;
using Newtonsoft.Json;
using RestSharp;

namespace DCP_eUICC_V2
{
    // Implements an API call that retrieves Login Credentials etc.
    public class SubscriptionApiHandler : ApiHandler
    {
        // Api Action
        public SubscriptionV1 get(string imsi)
        {
            // Check if simulated environment or real API
            if (ApiHandler.simulatedApiCalls)
                // Simulated API call
                return new SubscriptionV1()
                {
                    Imsi = "100973000012678",
                    State = "ACTIVE",
                    OperatorId = "97000001",
                    OperatorName = "Telia",
                    Msisdn = "200973000012678",
                    Iccid = "99987100973000012678",
                    LocaleList = null,
                    Label = null,
                    CompanyId = "97000002",
                    CompanyName = "Grundfos",
                    Pin1 = "1234",
                    Pin2 = "4321",
                    Puk1 = "12345678",
                    Puk2 = "87654321",
                    Region = null,
                    PbrExitDate = null,
                    InstallationDate = new Instant() { Nano = 779000000, EpochSecond = 1480599693 },
                    Specification = "Specification 01",
                    SpecificationType = "PROFILE_SPECIFICATION",
                    ArpAssignMentDate = null,
                    ArpName = null,
                    EuiccId = "999870A0B0C0D0E0F100973000012678"
                };
            else
            {
                // Real API call






                // Prepare the API request
                RestRequest request = new RestRequest("/subscriptions/{id}", Method.GET);
                request.AddUrlSegment("id", imsi);
                request.AddHeader("Accept", "application/vnd.dcp-v1+json");

                // Call the API and take care of the response
                if (this.send(request))
                    // Successful API request
                    return JsonConvert.DeserializeObject<SubscriptionV1>(this.response.Content);
                else
                    // API call failed, error info found in the Error object
                    return null;
            }
        }
    }
}
