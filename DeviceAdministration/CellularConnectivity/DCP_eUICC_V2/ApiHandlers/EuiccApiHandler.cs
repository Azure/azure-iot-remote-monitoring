using System.Collections.Generic;
using Newtonsoft.Json;
using RestSharp;

namespace DCP_eUICC_V2
{
    // Implements an API call that retrieves Login Credentials etc.
    public class EuiccApiHandler : ApiHandler
    {
        // Mock data: Current locale setting
        static public Dictionary<string, string> _localeBinds = new Dictionary<string, string>();

        // Api Action
        public EuiccV1 get(string eUICCid)
        {
            // Check if simulated environment or real API
            if (ApiHandler.simulatedApiCalls)
            {
                // Simulated API call
                EuiccV1 myEuicc = new EuiccV1()
                {
                    EuiccId = "999870A0B0C0D0E0F100973000012678",
                    CompanyId = "97000002",
                    CompanyName = "Grundfos",
                    State = "LOCALIZED",
                    Label = null,
                    LocaleName = "France",
                    BootstrapIcc = "99987100973000012678",
                    EnabledIcc = "99987100973000012678",
                    BootstrapCompanyId = "97000002",
                    LocalizationTableId = 10040
                };
                List<EuiccSubscriptionV1> myEuiccSubscriptions = new List<EuiccSubscriptionV1>();
                List<EuiccLocaleV1> myLocales = new List<EuiccLocaleV1>();
                myLocales.Add(new EuiccLocaleV1() { Id = "FR01", Name = "France" });
                myLocales.Add(new EuiccLocaleV1() { Id = "FR02", Name = "France2" });
                myEuiccSubscriptions.Add(new EuiccSubscriptionV1()
                {
                    Imsi = "100973000012678",
                    State = "ACTIVE",
                    OperatorId = "97000001",
                    OperatorName = "Telia",
                    Msisdn = "200973000012678",
                    Iccid = "99987100973000012678",
                    LocaleList = myLocales,
                });
                myEuicc.Subscriptions = myEuiccSubscriptions;

                // Mock code: use mocked locale
                string localeName;
                if (_localeBinds.TryGetValue(eUICCid, out localeName))
                {
                    myEuicc.LocaleName = localeName;
                }

                return myEuicc;
            }
            else
            {
                // Real API call

                // Prepare the API request
                RestRequest request = new RestRequest("/euiccs/{id}", Method.GET);
                request.AddUrlSegment("id", eUICCid);
                request.AddHeader("Accept", "application/vnd.dcp-v1+json");

                // Call the API and take care of the response
                if (this.send(request))
                    // Successful API request
                    return JsonConvert.DeserializeObject<EuiccV1>(this.response.Content);
                else
                    // API call failed, error info found in the Error object
                    return null;
            }
        }
    }
}
