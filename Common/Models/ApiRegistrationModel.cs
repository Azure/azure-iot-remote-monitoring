namespace Microsoft.Azure.Devices.Applications.RemoteMonitoring.Common.Models
{
    public class ApiRegistrationModel
    {
        public string BaseUrl { get; set; }
        public string LicenceKey { get; set; }
        public string Username { get; set; }
        public string Password { get; set; }
        public DeviceManagement.Infrustructure.Connectivity.Models.Enums.ApiRegistrationProviderType? ApiRegistrationProvider { get; set; }
    }
}