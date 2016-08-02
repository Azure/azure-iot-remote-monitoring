using System;
using System.Threading.Tasks;
using Microsoft.WindowsAzure.Storage.Table;

namespace Microsoft.Azure.Devices.Applications.RemoteMonitoring.Common.Helpers
{
    public class CloudTableProvider : ICloudTableProvider
    {
        private readonly CloudTable _table;

        public CloudTableProvider(CloudTableClient tableClient, string tableName)
        {
            _table = tableClient.GetTableReference(tableName);
        }

        public async Task<CloudTable> GetCloudTableAsync()
        {
            await _table.CreateIfNotExistsAsync();
            return _table;
        }
        public CloudTable GetCloudTable()
        {
            _table.CreateIfNotExists();
            return _table;
        }
    }
}