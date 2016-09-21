namespace Microsoft.Azure.Devices.Applications.RemoteMonitoring.Common.Helpers
{
    public interface IAzureTableStorageClientFactory
    {
        IAzureTableStorageClient CreateClient(string storageConnectionString, string tableName);
    }
}