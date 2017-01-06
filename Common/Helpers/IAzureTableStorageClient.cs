using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Azure.Devices.Applications.RemoteMonitoring.Common.Models;
using Microsoft.WindowsAzure.Storage.Table;

namespace Microsoft.Azure.Devices.Applications.RemoteMonitoring.Common.Helpers
{
    public interface IAzureTableStorageClient
    {
        Task<TableStorageResponse<TResult>> DoTableInsertOrReplaceAsync<TResult, TInput>(TInput incomingEntity,
            Func<TInput, TResult> tableEntityToModelConverter) where TInput : TableEntity;

        Task<TableStorageResponse<TResult>> DoDeleteAsync<TResult, TInput>(TInput incomingEntity,
            Func<TInput, TResult> tableEntityToModelConverter) where TInput : TableEntity;

        Task<TableStorageResponse<TResult>> DoTouchAsync<TResult, TInput>(TInput incomingEntity,
            Func<TInput, TResult> tableEntityToModelConverter)
            where TInput : TableEntity;

        TableResult Execute(TableOperation tableOperation);
        Task<TableResult> ExecuteAsync(TableOperation operation);
        Task<IList<TableResult>> ExecuteBatchAsync(TableBatchOperation operation);
        IEnumerable<T> ExecuteQuery<T>(TableQuery<T> tableQuery) where T : TableEntity, new();
        Task<IEnumerable<T>> ExecuteQueryAsync<T>(TableQuery<T> tableQuery) where T : TableEntity, new();
    }
}