namespace Microsoft.Azure.Devices.Applications.RemoteMonitoring.Common.Helpers
{
    public class AzureTableStorageClientFactory : IAzureTableStorageClientFactory
    {
        private IAzureTableStorageClient _tableStorageClient;

        public AzureTableStorageClientFactory() : this(null)
        {
        }

        public AzureTableStorageClientFactory(IAzureTableStorageClient customClient)
        {
            _tableStorageClient = customClient;
        }

        public IAzureTableStorageClient CreateClient(string storageConnectionString, string tableName)
        {
            if (_tableStorageClient == null)
            {
                _tableStorageClient = new AzureTableStorageClient(storageConnectionString, tableName);
            }
            return _tableStorageClient;
        }
    }
}