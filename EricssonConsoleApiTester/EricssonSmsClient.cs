using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace EricssonConsoleApiTester
{
    public class EricssonSmsClient
    {
        private readonly string _endpointUrl;
        public EricssonSmsClient(string endPointUrl)
        {
            _endpointUrl = endPointUrl;
        }

        public async void DownloadPageAsync()
        {
            // ... Target page.
            string page = "http://en.wikipedia.org/";

            // ... Use HttpClient.
            using (HttpClient client = new HttpClient())
            using (HttpResponseMessage response = await client.GetAsync(page))
            using (HttpContent content = response.Content)
            {
                // ... Read the string.
                string result = await content.ReadAsStringAsync();

                // ... Display the result.
                if (result != null &&
                result.Length >= 50)
                {
                    Console.WriteLine(result.Substring(0, 50) + "...");
                }
            }
        }

        public bool SendSms()
        {
            return true;
        }
    }
}
