using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RestSharp;
using RestSharp.Authenticators;
using Newtonsoft.Json;
using System.Net;

namespace DCP_eUICC_V2
{
    // Each action need to inherith
    public abstract class ApiHandler
    {
        // Properties
        protected static string userName = string.Empty;
        protected static string passWord = string.Empty;
        protected static string baseAddress = string.Empty;
        protected static string companyId = string.Empty;
        public static bool simulatedApiCalls = false;
        protected static RestClient ApiRestClient;
        protected IRestResponse response = null;
        protected static LoginV1 LoginCredentials;
        public Error Error = null;
        protected static DateTime loginTokenExpirationTime = DateTime.MinValue.AddYears(2);

        // Constructors
        public ApiHandler() { }
        public ApiHandler(string baseaddress, string companyid, string username, string password, bool simulateApiCalls) : this()
        {
            ApiHandler.baseAddress = baseaddress;
            ApiHandler.companyId = companyid;
            ApiHandler.userName = username;
            ApiHandler.passWord = password;
            ApiHandler.ApiRestClient = new RestClient(baseaddress);
            ApiHandler.simulatedApiCalls = simulateApiCalls;
        }

        //Sends an API request and checks for errors
        protected virtual bool send(RestRequest request)
        {
            // Make sure that we have logincredentials
            if (ApiHandler.LoginCredentials == null)
                ApiHandler.LoginCredentials = new LoginV1();

            // Check if we are logged in, try to login or renew if needed - if failure then exit
            LoginApiHandler myLoginApiHandler = new LoginApiHandler(ApiHandler.baseAddress, ApiHandler.companyId, ApiHandler.userName, ApiHandler.passWord, ApiHandler.simulatedApiCalls);
            if (!myLoginApiHandler.isLoggedIn())
                return this.isApiError();
            else
                return this.sendWithoutLogin(request);
        }

        //Sends an API request and checks for errors
        protected virtual bool sendWithoutLogin(RestRequest request)
        {
            // Initialize default values prior to the API call
            this.Error = null;
            this.response = null;

            // Add header parameters
            if ((LoginCredentials != null) && (LoginCredentials.Token != null) && (LoginCredentials.Token.Length > 0))
                request.AddHeader("X-Access-Token", ApiHandler.LoginCredentials.Token);

            // Send an API request and check the response for success
            //- if not successful then the property 'Error' contains info
            try
            {
                //this.response = this.ApiRestClient.Execute(request);
                this.response = ApiHandler.ApiRestClient.Execute(request);
            }
            catch (Exception ex)
            {
                this.Error = new Error() { Code = 0, HttpStatus = 0, Message = string.Format("Exception:\t{0}", ex.Message) };
                return false;
            }

            return this.isApiError();
        }

        // Checks for API errors - provide error message in 'Error' property
        protected bool isApiError()
        {
            // Check if we have a response value
            if (this.response != null)
            {
                // Check for success or different kinds of errors
                switch (this.response.ResponseStatus)
                {
                    case ResponseStatus.Completed:
                        {
                            switch (this.response.StatusCode)
                            {
                                case HttpStatusCode.OK:
                                    return true;
                                case HttpStatusCode.BadRequest:
                                case HttpStatusCode.InternalServerError:
                                case HttpStatusCode.ServiceUnavailable:
                                case HttpStatusCode.Unauthorized:
                                    this.Error = new Error();
                                    Error = JsonConvert.DeserializeObject<Error>(response.Content);
                                    break;
                                default:
                                    this.Error = new Error() { Code = 0, HttpStatus = (int)response.StatusCode, Message = System.Enum.GetName(typeof(System.Net.HttpStatusCode), response.StatusCode) };
                                    break;
                            }
                        }
                        break;
                    case ResponseStatus.Aborted:
                        this.Error = new Error() { Code = 0, HttpStatus = 0, Message = "API call aborted" };
                        break;
                    case ResponseStatus.TimedOut:
                        this.Error = new Error() { Code = 0, HttpStatus = 0, Message = "API call timeout" };
                        break;
                    case ResponseStatus.Error:
                    case ResponseStatus.None:
                    default:
                        this.Error = new Error() { Code = 0, HttpStatus = 0, Message = "Connectivity fault" };
                        break;
                }
            }
            else
                this.Error = new Error() { Code = 0, HttpStatus = 0, Message = "No content" };

            // Return the result value
            return false;

        }

        // Maybe later re-define the standard 'ToString() method
        public override string ToString()
        {
            return base.ToString();
        }
    }
}
