using System;
using System.Collections.Generic;
using System.Linq;
using DeviceManagement.Infrustructure.Connectivity.Models.TerminalDevice;
using Microsoft.Azure.Devices.Applications.RemoteMonitoring.Common.Configurations;
using Microsoft.Azure.Devices.Applications.RemoteMonitoring.Common.Helpers;
using Microsoft.Azure.Devices.Applications.RemoteMonitoring.Common.Models;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Table;

namespace Microsoft.Azure.Devices.Applications.RemoteMonitoring.DeviceAdmin.Infrastructure.Repository
{
    public class IccidRepository : IIccidRepository
    {
        private const string ICCID_TABLE_NAME = "IccidTable";
        private readonly IAzureTableStorageClient _azureTableStorageClient;

        public IccidRepository(IConfigurationProvider configProvider, IAzureTableStorageClientFactory tableStorageClientFactory)
        {
            _azureTableStorageClient = tableStorageClientFactory.CreateClient(configProvider.GetConfigurationSettingValue("device.StorageConnectionString"), ICCID_TABLE_NAME);
        }

        public bool AddIccid(Iccid iccid, string providerName)
        {
            try
            {
                var incomingEntity = new IccidTableEntity()
                {
                    Iccid = iccid.Id,
                    ProviderName = providerName,
                    RowKey = iccid.Id
                };
                _azureTableStorageClient.Execute(TableOperation.InsertOrMerge(incomingEntity));
            }
            catch (StorageException)
            {
                return false;
            }
            return true;
        }

        public bool AddIccids(List<Iccid> iccids, string providerName)
        {
            var failedUpdates = (from iccid in iccids let success = AddIccid(iccid, providerName) where success == false select iccid).ToList();
            return !failedUpdates.Any();
        }

        public bool DeleteIccidTableEntity(IccidTableEntity iccidTableEntity)
        {
            try
            {
                _azureTableStorageClient.Execute(TableOperation.Delete(iccidTableEntity));
            }
            catch (Exception)
            {
                return false;
            }

            return true;
        }

        public bool DeleteAllIccids()
        {
            var query = new TableQuery<IccidTableEntity>().
                Where(TableQuery.GenerateFilterCondition("PartitionKey", QueryComparisons.Equal, IccidRegistrationKey.Default.ToString()));
            var queryResponse = _azureTableStorageClient.ExecuteQuery(query).ToList();
            if (!queryResponse.Any()) return true;
            var failedDelete = (from iccid in queryResponse let deleted = DeleteIccidTableEntity(iccid) where !deleted select iccid).ToList();
            return !failedDelete.Any();
        }

        public IList<Iccid> GetIccids()
        {
            var query = new TableQuery<IccidTableEntity>()
                .Where(TableQuery.GenerateFilterCondition("PartitionKey", QueryComparisons.Equal, IccidRegistrationKey.Default.ToString()));
            var queryResponse = _azureTableStorageClient.ExecuteQuery(query);
            return (from iccid in queryResponse select new Iccid(iccid.Iccid)).ToList();
        }

        public string GetLastSetLocaleServiceRequestId(string iccid)
        {
            return Find(iccid)?.LastSetLocaleServiceRequestId;
        }

        public void SetLastSetLocaleServiceRequestId(string iccid, string serviceRequestId)
        {
            var entity = Find(iccid);
            if (entity != null)
            {
                entity.LastSetLocaleServiceRequestId = serviceRequestId;
                _azureTableStorageClient.Execute(TableOperation.InsertOrReplace(entity));
            }
        }

        private IccidTableEntity Find(string iccid)
        {
            var query = new TableQuery<IccidTableEntity>().Where(TableQuery.CombineFilters(
                TableQuery.GenerateFilterCondition("PartitionKey", QueryComparisons.Equal, IccidRegistrationKey.Default.ToString()),
                TableOperators.And,
                TableQuery.GenerateFilterCondition("RowKey", QueryComparisons.Equal, iccid)));

            return _azureTableStorageClient.ExecuteQuery(query).SingleOrDefault();
        }
    }
}
