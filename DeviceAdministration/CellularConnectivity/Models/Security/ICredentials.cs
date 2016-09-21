namespace DeviceManagement.Infrustructure.Connectivity.Models.Security
{
    public interface ICredentials
    {
        string LicenceKey { get; set; }
        string Username { get; set; }
        string Password { get; set; }
        string BaseUrl { get; set; }
        string ApiRegistrationProvider { get; set; }
    }
}