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
    public class EuiccLocalesApiHandler : ApiHandler
    {
        // Api Action
        public List<EuiccLocaleV1> getLocales(string localizationTableId)
        {
            // Check if simulated environment or real API
            if (ApiHandler.simulatedApiCalls)
            {
                // Simulated API call
                List<EuiccLocaleV1> myLocaleList = new List<EuiccLocaleV1>();
                myLocaleList.Add(new EuiccLocaleV1() { Id = "FR01", Name = "France" });
                myLocaleList.Add(new EuiccLocaleV1() { Id = "FR02", Name = "France2" });
                myLocaleList.Add(new EuiccLocaleV1() { Id = "SE01", Name = "Sweden" });
                myLocaleList.Add(new EuiccLocaleV1() { Id = "DK01", Name = "Denmark" });
                myLocaleList.Add(new EuiccLocaleV1() { Id = "CH01", Name = "China" });
                myLocaleList.Add(new EuiccLocaleV1() { Id = "SP01", Name = "Spain" });
                return myLocaleList;
            }
            else
            {
                // Real API call

                // Prepare the API request
                RestRequest request = new RestRequest("/locales", Method.GET);
                request.AddHeader("Accept", "application/vnd.dcp-v1+json");
                request.AddQueryParameter("localizationTableId", localizationTableId);

                // Call the API and take care of the response
                if (this.send(request))
                    // Successful API request
                    return JsonConvert.DeserializeObject<List<EuiccLocaleV1>>(response.Content);
                else
                    // API call failed, error info found in the Error object
                    return null;
            }
        }
    }
}
