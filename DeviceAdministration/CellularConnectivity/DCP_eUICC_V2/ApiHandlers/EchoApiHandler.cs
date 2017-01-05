using RestSharp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DCP_eUICC_V2
{
    class EchoApiHandler : ApiHandler
    {
        // Constructor
        public EchoApiHandler(string baseaddress) : base(baseaddress, string.Empty, string.Empty, string.Empty, ApiHandler.simulatedApiCalls) { }

        // Api Action
        public string echo(string echomessage)
        {
            // Check if simulated environment or real API
            if (ApiHandler.simulatedApiCalls)
                // Simulated API call
                return echomessage;
            else
            {
                // Real API call

                // Prepare the API request
                RestRequest request = new RestRequest("/monitor/echo/{msg}", Method.GET);
                request.AddUrlSegment("msg", echomessage);
                request.AddHeader("Accept", "text/plain");

                // Call the API and take care of the response
                if (this.sendWithoutLogin(request))
                    // Successful API request
                    return this.response.Content;
                else
                    // API call failed, error info found in the Error object
                    return string.Empty;
            }
        }
    }
}
