namespace DeviceManagement.Infrustructure.Connectivity.Models.Security
{
    public interface ICredentialProvider
    {
        ICredentials Provide();
    }
}