using System.Linq;
using Newtonsoft.Json;
using RestSharp;

namespace DCP_eUICC_V2
{
    // Implements an API call that retrieves Login Credentials etc.
    public class EuiccLocalizationApiHandler : ApiHandler
    {
        // Api Action
        public ServiceRequestV1 localize(string eUICCid, string localeId, string localizationTableId)
        {
            // Check if simulated environment or real API
            if (ApiHandler.simulatedApiCalls)
            {
                // Mock code: save locale setting
                var locales = new EuiccLocalesApiHandler().getLocales(localizationTableId);
                var localeName = locales.FirstOrDefault(l => l.Id == localeId)?.Name ?? string.Empty;
                EuiccApiHandler._localeBinds[eUICCid] = localeName;

                // Simulated API call
                return new ServiceRequestV1()
                {
                    CompanyId = "97000002",
                    CompanyName = "Grundfos",
                    CreatedBy = "user@grundfos.com",
                    LastUpdated = new Instant() { Nano = 779000000, EpochSecond = 1480599693 },
                    ServiceRequestId = "REQ0001",
                    ServiceRequestState = "In progress",
                    ServiceRequestType = string.Empty,
                    Size = 0,
                    TimeCreated = new Instant() { Nano = 779000000, EpochSecond = 1480599693 }
                };
            }
            else
            {
                // Real API call

                // Prepare the API request
                RestRequest request = new RestRequest("/euiccs/{id}/localization", Method.POST);
                request.AddUrlSegment("id", eUICCid);
                request.AddQueryParameter("localeId", localeId);
                request.AddQueryParameter("localizationTableId", localizationTableId);
                request.AddQueryParameter("companyId", ApiHandler.companyId);
                request.AddHeader("Accept", "application/vnd.dcp-v1+json");

                // Call the API and take care of the response
                if (this.send(request))
                    // Successful API request
                    return JsonConvert.DeserializeObject<ServiceRequestV1>(response.Content);
                else
                    // API call failed, error info found in the Error object
                    return null;
            }
        }
    }
}
