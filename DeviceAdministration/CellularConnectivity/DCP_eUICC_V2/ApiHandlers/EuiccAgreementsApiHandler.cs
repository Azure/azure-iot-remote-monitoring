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
    public class EuiccAgreementsApiHandler : ApiHandler
    {
        // Properties
        //public EuiccAgreementList euiccAgreements = null;   // Populated after a successful API call

        // Api Action
        public EuiccAgreementListV1 getAgreements()
        {
            // Check if simulated environment or real API
            if (ApiHandler.simulatedApiCalls)
            {
                // Simulated API call
                EuiccAgreementListV1 myList = new EuiccAgreementListV1();
                myList.EuiccAgreements = new List<EuiccAgreementV1>();
                myList.EuiccAgreements.Add(new EuiccAgreementV1() { FormSpecification = new FormSpecificationV1(), Id = "Id1", LeadOperator = "Telia", Name = "Telia", ProfileSpecifications = new List<ProfileSpecification>() });
                myList.ServiceContracts = new List<ServiceContractV1>();
                myList.ServiceContracts.Add(new ServiceContractV1() { Id = "Id1", Description = "ServiceContract01", Name = "SC 01", SubscriptionPackages = new List<SubscriptionPackageV1>() });
                return myList;
            }
            else
            {
                // Real API call

                // Prepare the API request
                RestRequest request = new RestRequest("/euiccAgreements", Method.GET);
                request.AddHeader("Accept", "application/vnd.dcp-v1+json");
                request.AddQueryParameter("companyId", ApiHandler.companyId);

                // Call the API and take care of the response
                if (this.send(request))
                    // Successful API request
                    return JsonConvert.DeserializeObject<EuiccAgreementListV1>(response.Content);
                else
                    // API call failed, error info found in the Error object
                    return null;
            }
        }
    }
}
