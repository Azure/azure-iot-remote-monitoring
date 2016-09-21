using DeviceManagement.Infrustructure.Connectivity.Models.Security;

namespace DeviceManagement.Infrustructure.Connectivity.Models.Other
{
    public class CellularCredentials : ICredentials
    {
        //todo : how can I structure this to allow multiple telcos?
        public string LicenceKey { get; set; }
        public string Username { get; set; }
        public string Password { get; set; }
        public string BaseUrl { get; set; }
        public string ApiRegistrationProvider { get; set; }
    }
}