using System;
using System.Text;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.Serialization;
using Newtonsoft.Json;
using RestSharp;
using System.Threading.Tasks;
using System.Threading;

namespace DCP_eUICC_V2
{
    // Implements an API call
    public class LoginApiHandler : ApiHandler
    {
        // Some constants for the keepAliveTask
        const int DefaultDelay = 1;             // Seconds
        const int MaxDelay = 5 * 60;            // Seconds
        const int TokenExpirationTimeMargin = 5;     // Seconds
        const int TokenReissueTimeMargin = TokenExpirationTimeMargin + 5;     // Seconds

        // Constructors
        public LoginApiHandler(string baseaddress, string companyid, string username, string password, bool simulateApi) : base(baseaddress, companyid, username, password, simulateApi) { }

        // This task tries to login to API and then regularly keep it alive
        private Task KeepAliveTask()
        {
            // Set a start delay value
            int backoffdelay = 1;

            // Loop forever, i.e. until calling process goes out of scope
            while (true)
            {
                // Try to login
                if (!this.login())
                {   // Login failed, back-off and wait and then try again

                    Console.WriteLine("Loging failed!");

                    // Calculate a delay value
                    if (backoffdelay < MaxDelay)
                        backoffdelay *= 2;

                    // Sleep (milliseconds)
                    Thread.Sleep(backoffdelay * 1000);
                }
                else
                {   // Login succeeded


                    Console.WriteLine("Loging Succeded!");

                    // Reset the backoff delay value
                    backoffdelay = 1;

                    // Wait almost the timeout value
                    long myExpirationtime = 0;
                    if (ApiHandler.LoginCredentials.ExpirationTime != null)
                    {
                        // Calcualte a value to use
                        myExpirationtime = (long)ApiHandler.LoginCredentials.ExpirationTime - (1000 * TokenExpirationTimeMargin);

                        // Handle the case that 'Sleep' only takes 'int' and not long
                        while (myExpirationtime > 0)
                        {
                            // Check for valid 'int' value for the sleep function
                            if (myExpirationtime > int.MaxValue)
                            {
                                // Sleep (milliseconds)
                                Thread.Sleep(int.MaxValue);

                                // Subtract the delay value
                                myExpirationtime -= int.MaxValue;
                            }
                            else
                            {
                                // Sleep (milliseconds)
                                Thread.Sleep((int)myExpirationtime);

                                // We are done - Reset the delay value
                                myExpirationtime = 0;
                            }
                        }
                    }
                }
            }
        }

        // Checks if we are logged, in otherwise tries to login
        public bool isLoggedIn()
        {
            // Keep track of the time now
            DateTime timestampNow = DateTime.UtcNow;

            // Check if we need to login, if so try to login
            if (timestampNow > ApiHandler.loginTokenExpirationTime.AddSeconds(-TokenExpirationTimeMargin))
                return this.login();

            // Check if we need to reisse a new token
            if (timestampNow > ApiHandler.loginTokenExpirationTime.AddSeconds(-TokenReissueTimeMargin))
                return this.tokenReissue();

            // Token is still valid, return
            return true;
        }

        // Api Action
        private bool login()
        {
            // Prepare the API request
            RestRequest request = new RestRequest("/login", Method.POST);
            request.RequestFormat = DataFormat.Json;
            request.AddBody(new LoginSendBody() { email = ApiHandler.userName, password = ApiHandler.passWord });
            request.AddHeader("Accept", "application/vnd.dcp-v1+json");

            // Call the API and take care of the response
            if (this.sendWithoutLogin(request))
            {
                // Successful API request
                ApiHandler.LoginCredentials = JsonConvert.DeserializeObject<LoginV1>(response.Content);
                ApiHandler.loginTokenExpirationTime = DateTime.UtcNow.AddMilliseconds((long)ApiHandler.LoginCredentials.ExpirationTime);
                return true;
            }
            else
                // API call failed, error info found in the Error object
                return false;
        }

        // Api Action
        private bool tokenReissue()
        {
            // Prepare the API request
            RestRequest request = new RestRequest("/token-reissue", Method.POST);
            request.AddHeader("Accept", "application/vnd.dcp-v1+json");
            //request.RequestFormat = DataFormat.Json;
            //request.AddBody(new LoginSendBody());

            // Call the API and take care of the response
            if (this.sendWithoutLogin(request))
            {
                // Successful API request
                ApiHandler.LoginCredentials = JsonConvert.DeserializeObject<LoginV1>(response.Content);
                ApiHandler.loginTokenExpirationTime = DateTime.UtcNow.AddMilliseconds((long)ApiHandler.LoginCredentials.ExpirationTime);
                return true;
            }
            else
                // API call failed, error info found in the Error object
                return false;
        }

        // Class that models a POST BODY for login
        private class LoginSendBody
        {
            // Properties
            public string email;
            public string password;

            // Constructor
            public LoginSendBody()
            {
                this.email = ApiHandler.userName;
                this.password = ApiHandler.passWord;
            }
        }
    }
}
