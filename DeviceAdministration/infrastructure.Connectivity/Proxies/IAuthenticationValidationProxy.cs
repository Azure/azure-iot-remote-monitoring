using DeviceManagement.Infrustructure.Connectivity.com.jasperwireless.spark.echo;

namespace DeviceManagement.Infrustructure.Connectivity.Proxies
{
    public interface IAuthenticationValidationProxy
    {
        EchoResponse ValidateCredentials();
    }
}