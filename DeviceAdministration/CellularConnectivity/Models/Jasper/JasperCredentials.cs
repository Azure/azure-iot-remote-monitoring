using DeviceManagement.Infrustructure.Connectivity.Models.Enums;
using DeviceManagement.Infrustructure.Connectivity.Models.Security;

namespace DeviceManagement.Infrustructure.Connectivity.Models.Jasper
{
    public class JasperCredentials : ICredentials
    {
        //todo : how can I structure this to allow multiple telcos?
        public string LicenceKey { get; set; }
        public string Username { get; set; }
        public string Password { get; set; }
        public string BaseUrl { get; set; }
        public ApiRegistrationProviderType ApiRegistrationProviderType { get; set; }
    }
}