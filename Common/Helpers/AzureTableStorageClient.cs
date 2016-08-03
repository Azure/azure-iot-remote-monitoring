using Microsoft.Azure.Devices.Applications.RemoteMonitoring.Common.Models;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Table;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace Microsoft.Azure.Devices.Applications.RemoteMonitoring.Common.Helpers
{
    public class AzureTableStorageClient : IAzureTableStorageClient
    {
        private CloudTable _table;
        private readonly CloudTableClient _tableClient;
        private readonly string _tableName;

        public AzureTableStorageClient(string storageConnectionString, string tableName)
        {
            CloudStorageAccount storageAccount = CloudStorageAccount.Parse(storageConnectionString);
            _tableClient = storageAccount.CreateCloudTableClient();
            _tableName = tableName;
        }
        private async Task<CloudTable> GetCloudTableAsync()
        {
            if (_table == null && _tableName != null)
            {
                _table = _tableClient.GetTableReference(_tableName);
                await _table.CreateIfNotExistsAsync();
            }
            return _table;
        }
        private CloudTable GetCloudTable()
        {
            if (_table != null)
            {
                return _table;
            }
            _table = _tableClient.GetTableReference(_tableName);
            _table.CreateIfNotExists();
            return _table;
        }

        public TableResult Execute(TableOperation tableOperation)
        {
            CloudTable table = this.GetCloudTable();
            return table.Execute(tableOperation);
        }
        public async Task<TableResult> ExecuteAsync(TableOperation operation)
        {
            CloudTable table = await this.GetCloudTableAsync();
            return await table.ExecuteAsync(operation);
        }
        public IEnumerable<T> ExecuteQuery<T>(TableQuery<T> tableQuery) where T : TableEntity, new()
        {
            CloudTable table = this.GetCloudTable();
            return table.ExecuteQuery(tableQuery);
        }
        public async Task<IEnumerable<T>> ExecuteQueryAsync<T>(TableQuery<T> tableQuery) where T : TableEntity, new()
        {
            CloudTable table = await this.GetCloudTableAsync();
            return table.ExecuteQuery(tableQuery);
        }

        public async Task<TableStorageResponse<TResult>> DoTableInsertOrReplaceAsync<TResult, TInput>(TInput incomingEntity,
            Func<TInput, TResult> tableEntityToModelConverter) where TInput : TableEntity
        {
            CloudTable table = await this.GetCloudTableAsync();

            // Simply doing an InsertOrReplace will not do any concurrency checking, according to 
            // http://azure.microsoft.com/en-us/blog/managing-concurrency-in-microsoft-azure-storage-2/
            // So we will not use InsertOrReplace. Instead we will look to see if we have a rule like this
            // If so, then we'll do a concurrency-safe update, otherwise simply insert
            TableOperation retrieveOperation =
                TableOperation.Retrieve<TInput>(incomingEntity.PartitionKey, incomingEntity.RowKey);
            TableResult retrievedEntity = await table.ExecuteAsync(retrieveOperation);

            TableOperation operation = null;
            if (retrievedEntity.Result != null)
            {
                operation = TableOperation.Replace(incomingEntity);
            }
            else
            {
                operation = TableOperation.Insert(incomingEntity);
            }

            return await PerformTableOperation<TResult, TInput>(operation, incomingEntity, tableEntityToModelConverter);
        }

        public async Task<TableStorageResponse<TResult>> DoDeleteAsync<TResult, TInput>(TInput incomingEntity,
            Func<TInput, TResult> tableEntityToModelConverter) where TInput : TableEntity
        {
            TableOperation operation = TableOperation.Delete(incomingEntity);
            return await PerformTableOperation<TResult, TInput>(operation, incomingEntity, tableEntityToModelConverter);
        }

        private async Task<TableStorageResponse<TResult>> PerformTableOperation<TResult, TInput>(TableOperation operation, TInput incomingEntity, Func<TInput, TResult> tableEntityToModelConverter) where TInput : TableEntity
        {
            CloudTable table = await this.GetCloudTableAsync();
            var result = new TableStorageResponse<TResult>();

            try
            {
                await table.ExecuteAsync(operation);

                var nullModel = tableEntityToModelConverter((TInput)null);
                result.Entity = nullModel;
                result.Status = TableStorageResponseStatus.Successful;
            }
            catch (Exception ex)
            {
                TableOperation retrieveOperation = TableOperation.Retrieve<TInput>(incomingEntity.PartitionKey, incomingEntity.RowKey);
                TableResult retrievedEntity = table.Execute(retrieveOperation);

                if (retrievedEntity != null)
                {
                    // Return the found version of this rule in case it had been modified by someone else since our last read.
                    var retrievedModel = tableEntityToModelConverter((TInput)retrievedEntity.Result);
                    result.Entity = retrievedModel;
                }
                else
                {
                    // We didn't find an existing rule, probably creating new, so we'll just return what was sent in
                    result.Entity = tableEntityToModelConverter(incomingEntity);
                }

                if (ex.GetType() == typeof(StorageException)
                    && (((StorageException)ex).RequestInformation.HttpStatusCode == (int)HttpStatusCode.PreconditionFailed
                    || ((StorageException)ex).RequestInformation.HttpStatusCode == (int)HttpStatusCode.Conflict))
                {
                    result.Status = TableStorageResponseStatus.ConflictError;
                }
                else
                {
                    result.Status = TableStorageResponseStatus.UnknownError;
                }
            }

            return result;
        }
    }
}
