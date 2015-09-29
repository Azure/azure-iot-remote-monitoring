namespace Microsoft.Azure.Devices.Applications.RemoteMonitoring.DeviceAdmin.Infrastructure.BusinessLogic
{
    public interface ICommandParameterTypeLogic
    {
        bool IsValid(string typeName, object value);
        object Get(string typeName, object value);
    }
}