using System;
using System.Text;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.Serialization;
using Newtonsoft.Json;
using RestSharp;

namespace DCP_eUICC_V2
{
    // Implements an API call
    public class CompanyApiHandler : ApiHandler
    {
        // Api Action
        public List<CompanyV1> getCompanies()
        {
            // Check if simulated environment or real API
            if (ApiHandler.simulatedApiCalls)
            {
                // Simulated API call
                List<CompanyV1> myList = new List<CompanyV1>();
                myList.Add(new CompanyV1() { Id = "Id1", Name = "Company 1" });
                myList.Add(new CompanyV1() { Id = "Id2", Name = "Company 2" });
                myList.Add(new CompanyV1() { Id = "Id3", Name = "Company 3" });
                return myList;
            }
            else
            {
                // Real API call

                // Prepare the API request
                RestRequest request = new RestRequest("/allowedCompanies", Method.GET);
                request.AddHeader("Accept", "application/vnd.dcp-v1+json");
                request.AddQueryParameter("companyId", ApiHandler.companyId);

                // Call the API and take care of the response
                if (this.send(request))
                    // Successful API request
                    return JsonConvert.DeserializeObject<List<CompanyV1>>(response.Content);
                else
                    // API call failed, error info found in the Error object
                    return null;
            }
        }
    }
}
