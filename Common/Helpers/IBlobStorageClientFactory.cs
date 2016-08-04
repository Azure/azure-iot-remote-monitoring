namespace Microsoft.Azure.Devices.Applications.RemoteMonitoring.Common.Helpers
{
    public interface IBlobStorageClientFactory
    {
        IBlobStorageClient CreateClient(string storageConnectionString, string containerName);
    }
}