using Microsoft.Azure.Devices.Applications.RemoteMonitoring.Common.Models;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Table;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace Microsoft.Azure.Devices.Applications.RemoteMonitoring.Common.Helpers
{
    public static class AzureTableStorageHelper
    {
        public static async Task<CloudTable> GetTableAsync(string storageConnectionString, string tableName)
        {
            CloudStorageAccount storageAccount = CloudStorageAccount.Parse(storageConnectionString);
            CloudTableClient tableClient = storageAccount.CreateCloudTableClient();
            CloudTable devicesTable = tableClient.GetTableReference(tableName);
            await devicesTable.CreateIfNotExistsAsync();
            return devicesTable;
        }

        public static async Task<TableStorageResponse<TResult>> DoTableInsertOrReplace<TResult, TInput>(TInput incomingEntity, 
            Func<TInput, TResult> tableEntityToModelConverter, string storageAccountConnectionString, string tableName) where TInput : TableEntity
        {
            var result = new TableStorageResponse<TResult>();

            var deviceRulesTable = await AzureTableStorageHelper.GetTableAsync(storageAccountConnectionString, tableName);

            // Simply doing an InsertOrReplace will not do any concurrency checking, according to 
            // http://azure.microsoft.com/en-us/blog/managing-concurrency-in-microsoft-azure-storage-2/
            // So we will not use InsertOrReplace. Instead we will look to see if we have a rule like this
            // If so, then we'll do a concurrency-safe update, otherwise simply insert
            TableOperation retrieveOperation =
                TableOperation.Retrieve<TInput>(incomingEntity.PartitionKey, incomingEntity.RowKey);
            TableResult retrievedEntity = await deviceRulesTable.ExecuteAsync(retrieveOperation);
            try
            {
                TableOperation operation = null;
                if (retrievedEntity.Result != null)
                {
                    operation = TableOperation.Replace(incomingEntity);
                }
                else
                {
                    operation = TableOperation.Insert(incomingEntity);
                }
                await deviceRulesTable.ExecuteAsync(operation);

                //Get the new version of the entity out of the database
                // And set up the result to return to the caller
                retrievedEntity = await deviceRulesTable.ExecuteAsync(retrieveOperation);
                var updatedModel = tableEntityToModelConverter((TInput)retrievedEntity.Result);
                result.Entity = updatedModel;
                result.Status = TableStorageResponseStatus.Successful;
            }
            catch (Exception ex)
            {
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
