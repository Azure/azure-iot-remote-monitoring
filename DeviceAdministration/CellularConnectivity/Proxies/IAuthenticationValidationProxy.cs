using DeviceManagement.Infrustructure.Connectivity.com.jasperwireless.spark.echo;

namespace DeviceManagement.Infrustructure.Connectivity.Proxies
{
    internal interface IAuthenticationValidationProxy
    {
        EchoResponse ValidateCredentials();
    }
}