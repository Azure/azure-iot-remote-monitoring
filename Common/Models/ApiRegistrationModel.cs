using Microsoft.Azure.Devices.Applications.RemoteMonitoring.Common.Constants;

namespace Microsoft.Azure.Devices.Applications.RemoteMonitoring.Common.Models
{
    public class ApiRegistrationModel
    {
        public string BaseUrl { get; set; }
        public string LicenceKey { get; set; }
        public string Username { get; set; }
        public string Password { get; set; }
        public string ApiRegistrationProvider { get; set; }

        public string EnterpriseSenderNumber { get; set; }
        public string RegistrationID { get; set; }
        public string SmsEndpointBaseUrl { get; set; }
    }
}