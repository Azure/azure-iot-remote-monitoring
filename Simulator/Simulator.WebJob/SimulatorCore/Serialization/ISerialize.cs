namespace Microsoft.Azure.Devices.Applications.RemoteMonitoring.Simulator.WebJob.SimulatorCore.Serialization
{
    /// <summary>
    /// Interface to serialize & deserialize through the ITransport interface
    /// </summary>
    public interface ISerialize
    {
        byte[] SerializeObject(object Object);
        T DeserializeObject<T>(byte[] bytes) where T : class;
    }
}
