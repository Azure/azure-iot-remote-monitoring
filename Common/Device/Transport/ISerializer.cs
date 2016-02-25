namespace Microsoft.Azure.Devices.Applications.RemoteMonitoring.Common.Device.Transport
{
    /// <summary>
    /// Interface to serialize & deserialize through the ITransport interface
    /// </summary>
    public interface ISerializer
    {
        byte[] SerializeObject(object @object);

        T DeserializeObject<T>(byte[] bytes) where T : class;
    }
}
