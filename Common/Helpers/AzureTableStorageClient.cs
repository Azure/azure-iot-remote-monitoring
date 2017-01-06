using System;
using System.Collections.Generic;
using System.Net;
using System.Threading.Tasks;
using Microsoft.Azure.Devices.Applications.RemoteMonitoring.Common.Models;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Table;

namespace Microsoft.Azure.Devices.Applications.RemoteMonitoring.Common.Helpers
{
    public class AzureTableStorageClient : IAzureTableStorageClient
    {
        private readonly CloudTableClient _tableClient;
        private readonly string _tableName;
        private CloudTable _table;

        public AzureTableStorageClient(string storageConnectionString, string tableName)
        {
            var storageAccount = CloudStorageAccount.Parse(storageConnectionString);
            _tableClient = storageAccount.CreateCloudTableClient();
            _tableName = tableName;
        }

        public TableResult Execute(TableOperation tableOperation)
        {
            var table = GetCloudTable();
            return table.Execute(tableOperation);
        }

        public async Task<TableResult> ExecuteAsync(TableOperation operation)
        {
            var table = await GetCloudTableAsync();
            return await table.ExecuteAsync(operation);
        }

        public async Task<IList<TableResult>> ExecuteBatchAsync(TableBatchOperation operation)
        {
            var table = await GetCloudTableAsync();
            return await table.ExecuteBatchAsync(operation);
        }

        public IEnumerable<T> ExecuteQuery<T>(TableQuery<T> tableQuery) where T : TableEntity, new()
        {
            var table = GetCloudTable();
            return table.ExecuteQuery(tableQuery);
        }

        public async Task<IEnumerable<T>> ExecuteQueryAsync<T>(TableQuery<T> tableQuery) where T : TableEntity, new()
        {
            var table = await GetCloudTableAsync();
            return table.ExecuteQuery(tableQuery);
        }

        public async Task<TableStorageResponse<TResult>> DoTableInsertOrReplaceAsync<TResult, TInput>(
            TInput incomingEntity,
            Func<TInput, TResult> tableEntityToModelConverter) where TInput : TableEntity
        {
            var table = await GetCloudTableAsync();

            // Simply doing an InsertOrReplace will not do any concurrency checking, according to 
            // http://azure.microsoft.com/en-us/blog/managing-concurrency-in-microsoft-azure-storage-2/
            // So we will not use InsertOrReplace. Instead we will look to see if we have a rule like this
            // If so, then we'll do a concurrency-safe update, otherwise simply insert
            var retrieveOperation =
                TableOperation.Retrieve<TInput>(incomingEntity.PartitionKey, incomingEntity.RowKey);
            var retrievedEntity = await table.ExecuteAsync(retrieveOperation);

            TableOperation operation = null;
            if (retrievedEntity.Result != null)
            {
                operation = TableOperation.Replace(incomingEntity);
            }
            else
            {
                operation = TableOperation.Insert(incomingEntity);
            }

            return await PerformTableOperation(operation, incomingEntity, tableEntityToModelConverter);
        }

        public async Task<TableStorageResponse<TResult>> DoDeleteAsync<TResult, TInput>(TInput incomingEntity,
            Func<TInput, TResult> tableEntityToModelConverter) where TInput : TableEntity
        {
            var operation = TableOperation.Delete(incomingEntity);
            return await PerformTableOperation(operation, incomingEntity, tableEntityToModelConverter);
        }

        public async Task<TableStorageResponse<TResult>> DoTouchAsync<TResult, TInput>(TInput incomingEntity,
            Func<TInput, TResult> tableEntityToModelConverter)
            where TInput : TableEntity
        {
            var table = await GetCloudTableAsync();

            var retrieveOperation =
                TableOperation.Retrieve<TInput>(incomingEntity.PartitionKey, incomingEntity.RowKey);
            var tableResult = await table.ExecuteAsync(retrieveOperation);
            var retrievedEntity = tableResult.Result as TInput;
            if (retrievedEntity != null)
            {
                var replaceOperation = TableOperation.Replace(retrievedEntity);
                return await PerformTableOperation(replaceOperation, retrievedEntity, tableEntityToModelConverter);
            }
            return new TableStorageResponse<TResult>
            {
                Entity = tableEntityToModelConverter(null),
                Status = TableStorageResponseStatus.NotFound,
            };
        }

        private async Task<CloudTable> GetCloudTableAsync()
        {
            if (_table != null)
            {
                return _table;
            }
            _table = _tableClient.GetTableReference(_tableName);
            await _table.CreateIfNotExistsAsync();
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

        private async Task<TableStorageResponse<TResult>> PerformTableOperation<TResult, TInput>(
            TableOperation operation, TInput incomingEntity, Func<TInput, TResult> tableEntityToModelConverter)
            where TInput : TableEntity
        {
            var table = await GetCloudTableAsync();
            var result = new TableStorageResponse<TResult>();

            try
            {
                await table.ExecuteAsync(operation);

                var nullModel = tableEntityToModelConverter(null);
                result.Entity = nullModel;
                result.Status = TableStorageResponseStatus.Successful;
            }
            catch (Exception ex)
            {
                var retrieveOperation = TableOperation.Retrieve<TInput>(incomingEntity.PartitionKey,
                    incomingEntity.RowKey);
                var retrievedEntity = table.Execute(retrieveOperation);

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
                    &&
                    (((StorageException)ex).RequestInformation.HttpStatusCode ==
                     (int)HttpStatusCode.PreconditionFailed
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