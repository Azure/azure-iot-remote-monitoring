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
    public class ServiceRequestApiHandler : ApiHandler
    {
        // Api Action
        public ServiceRequestV1 get(string servicerequestid)
        {
            // Prepare the API request
            RestRequest request = new RestRequest("/serviceRequests/{id}", Method.GET);
            request.AddUrlSegment("id", servicerequestid);
            request.AddHeader("Accept", "application/vnd.dcp-v1+json");

            // Call the API and take care of the response
            if (this.send(request))
                // Successful API request
                return JsonConvert.DeserializeObject<ServiceRequestV1>(this.response.Content);
            else
                // API call failed, error info found in the Error object
                return null;
        }
    }
}
