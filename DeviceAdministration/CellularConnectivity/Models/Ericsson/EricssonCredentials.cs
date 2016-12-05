using DeviceManagement.Infrustructure.Connectivity.Models.Other;
using DeviceManagement.Infrustructure.Connectivity.Models.Security;

namespace DeviceManagement.Infrustructure.Connectivity.Models.Jasper
{
    public class EricssonCredentials : CellularCredentials
    {
        public string EnterpriseSenderNumber { get; set; }
        public string RegistrationID { get; set; }
        public string SmsEndpointBaseUrl { get; set; }
    }
}