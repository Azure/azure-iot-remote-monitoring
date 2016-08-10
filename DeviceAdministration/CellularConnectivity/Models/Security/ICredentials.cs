using DeviceManagement.Infrustructure.Connectivity.Models.Enums;

namespace DeviceManagement.Infrustructure.Connectivity.Models.Security
{
    public interface ICredentials
    {
        string LicenceKey { get; set; }
        string Username { get; set; }
        string Password { get; set; }
        string BaseUrl { get; set; }

        ApiRegistrationProviderType? ApiRegistrationProvider
        {
            get; set;
        }

    }
}