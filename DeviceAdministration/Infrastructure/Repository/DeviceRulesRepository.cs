using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using Microsoft.Azure.Devices.Applications.RemoteMonitoring.Common.Configurations;
using Microsoft.Azure.Devices.Applications.RemoteMonitoring.Common.Helpers;
using Microsoft.Azure.Devices.Applications.RemoteMonitoring.Common.Models;
using Microsoft.Azure.Devices.Applications.RemoteMonitoring.DeviceAdmin.Infrastructure.Models;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Blob;
using Microsoft.WindowsAzure.Storage.Table;
using Newtonsoft.Json;

namespace Microsoft.Azure.Devices.Applications.RemoteMonitoring.DeviceAdmin.Infrastructure.Repository
{
    /// <summary>
    /// Class for working with persistence of Device Rules data.
    /// Note that we store device rules in an Azure table, but we also need
    /// to save a different format as a blob for the rules ASA job to pickup.
    /// The ASA rules job uses that blob as ref data and joins the most 
    /// recent version to the incoming data stream from the IoT Hub.
    /// (The ASA job checks for new rules blobs every minute at a well-known path)
    /// </summary>
    public class DeviceRulesRepository : IDeviceRulesRepository
    {
        private readonly string _blobName;
        private readonly string _storageAccountConnectionString;
        private readonly string _deviceRulesBlobStoreContainerName;
        private readonly string _deviceRulesNormalizedTableName;
        private readonly IAzureTableStorageClient _azureTableStorageClient;
        private readonly IBlobStorageClient _blobStorageClient;

        private DateTimeFormatInfo _formatInfo;

        public DeviceRulesRepository(IConfigurationProvider configurationProvider, IAzureTableStorageClientFactory tableStorageClientFactory, IBlobStorageClientFactory blobStorageClientFactory)
        {
            _storageAccountConnectionString = configurationProvider.GetConfigurationSettingValue("device.StorageConnectionString");
            _deviceRulesBlobStoreContainerName = configurationProvider.GetConfigurationSettingValue("DeviceRulesStoreContainerName");
            _deviceRulesNormalizedTableName = configurationProvider.GetConfigurationSettingValue("DeviceRulesTableName");
            _azureTableStorageClient = tableStorageClientFactory.CreateClient(_storageAccountConnectionString, _deviceRulesNormalizedTableName);
            _blobName = configurationProvider.GetConfigurationSettingValue("AsaRefDataRulesBlobName");
            _blobStorageClient = blobStorageClientFactory.CreateClient(_storageAccountConnectionString, _deviceRulesBlobStoreContainerName);

            // note: InvariantCulture is read-only, so use en-US and hardcode all relevant aspects
            CultureInfo culture = CultureInfo.CreateSpecificCulture("en-US");
            _formatInfo = culture.DateTimeFormat;
            _formatInfo.ShortDatePattern = @"yyyy-MM-dd";
            _formatInfo.ShortTimePattern = @"HH-mm";
        }

        /// <summary>
        /// Get all Device Rules from AzureTableStorage. If none are found it will return an empty list.
        /// </summary>
        /// <returns>All DeviceRules or an empty list</returns>
        public async Task<List<DeviceRule>> GetAllRulesAsync()
        {
            List<DeviceRule> result = new List<DeviceRule>();

            IEnumerable<DeviceRuleTableEntity> queryResults = await GetAllRulesFromTable();
            foreach (DeviceRuleTableEntity rule in queryResults)
            {
                var deviceRule = BuildRuleFromTableEntity(rule);
                result.Add(deviceRule);
            }

            return result;
        }

        /// <summary>
        /// Retrieve a single rule from AzureTableStorage or default if none exists. 
        /// A distinct rule is defined by the combination key deviceID/DataField
        /// </summary>
        /// <param name="deviceId"></param>
        /// <param name="dataField"></param>
        /// <returns></returns>
        public async Task<DeviceRule> GetDeviceRuleAsync(string deviceId, string ruleId)
        {
            TableOperation query = TableOperation.Retrieve<DeviceRuleTableEntity>(deviceId, ruleId);

            TableResult response = await Task.Run(() =>
                _azureTableStorageClient.Execute(query)
            );

            DeviceRule result = BuildRuleFromTableEntity((DeviceRuleTableEntity)response.Result);
            return result;
        }

        /// <summary>
        /// Retrieve all rules from the database that have been defined for a single device.
        /// If none exist an empty list will be returned. This method guarantees a non-null
        /// result.
        /// </summary>
        /// <param name="deviceId"></param>
        /// <returns></returns>
        public async Task<List<DeviceRule>> GetAllRulesForDeviceAsync(string deviceId)
        {
            TableQuery<DeviceRuleTableEntity> query = new TableQuery<DeviceRuleTableEntity>().
                Where(TableQuery.GenerateFilterCondition("PartitionKey", QueryComparisons.Equal, deviceId));
            var devicesResult = await _azureTableStorageClient.ExecuteQueryAsync(query);
            List<DeviceRule> result = new List<DeviceRule>();
            foreach (DeviceRuleTableEntity entity in devicesResult)
            {
                result.Add(BuildRuleFromTableEntity(entity));
            }
            return result;
        }

        /// <summary>
        /// Save a Device Rule to the server. This may be either a new rule or an update to an existing rule. 
        /// </summary>
        /// <param name="updateContainer"></param>
        /// <returns></returns>
        public async Task<TableStorageResponse<DeviceRule>> SaveDeviceRuleAsync(DeviceRule updatedRule)
        {
            DeviceRuleTableEntity incomingEntity = BuildTableEntityFromRule(updatedRule);

            TableStorageResponse<DeviceRule> result =
                await _azureTableStorageClient.DoTableInsertOrReplaceAsync<DeviceRule, DeviceRuleTableEntity>(incomingEntity, BuildRuleFromTableEntity);

            if (result.Status == TableStorageResponseStatus.Successful)
            {
                // Build up a new blob to push up for ASA job ref data
                List<DeviceRuleBlobEntity> blobList = await BuildBlobEntityListFromTableRows();
                await PersistRulesToBlobStorageAsync(blobList);
            }

            return result;
        }

        public async Task<TableStorageResponse<DeviceRule>> DeleteDeviceRuleAsync(DeviceRule ruleToDelete)
        {
            DeviceRuleTableEntity incomingEntity = BuildTableEntityFromRule(ruleToDelete);

            TableStorageResponse<DeviceRule> result =
                await _azureTableStorageClient.DoDeleteAsync<DeviceRule, DeviceRuleTableEntity>(incomingEntity, BuildRuleFromTableEntity);

            if (result.Status == TableStorageResponseStatus.Successful)
            {
                // Build up a new blob to push up for ASA job ref data
                List<DeviceRuleBlobEntity> blobList = await BuildBlobEntityListFromTableRows();
                await PersistRulesToBlobStorageAsync(blobList);
            }

            return result;
        }

        private async Task<IEnumerable<DeviceRuleTableEntity>> GetAllRulesFromTable()
        {
            TableQuery<DeviceRuleTableEntity> query = new TableQuery<DeviceRuleTableEntity>();

            return await _azureTableStorageClient.ExecuteQueryAsync(query);
        }

        private DeviceRuleTableEntity BuildTableEntityFromRule(DeviceRule incomingRule)
        {
            DeviceRuleTableEntity tableEntity =
                new DeviceRuleTableEntity(incomingRule.DeviceID, incomingRule.RuleId)
                {
                    DataField = incomingRule.DataField,
                    Threshold = (double)incomingRule.Threshold,
                    Enabled = incomingRule.EnabledState,
                    RuleOutput = incomingRule.RuleOutput
                };

            if (!string.IsNullOrWhiteSpace(incomingRule.Etag))
            {
                tableEntity.ETag = incomingRule.Etag;
            }

            return tableEntity;
        }

        private DeviceRule BuildRuleFromTableEntity(DeviceRuleTableEntity tableEntity)
        {
            if (tableEntity == null)
            {
                return null;
            }

            var updatedRule = new DeviceRule(tableEntity.RuleId)
            {
                DeviceID = tableEntity.DeviceId,
                DataField = tableEntity.DataField,
                Threshold = tableEntity.Threshold,
                EnabledState = tableEntity.Enabled,
                Operator = ">",
                RuleOutput = tableEntity.RuleOutput,
                Etag = tableEntity.ETag
            };

            return updatedRule;
        }

        /// <summary>
        /// Compile all rows from the table storage into the format used in the blob storage for
        /// ASA job reference data.
        /// </summary>
        /// <returns></returns>
        private async Task<List<DeviceRuleBlobEntity>> BuildBlobEntityListFromTableRows()
        {
            IEnumerable<DeviceRuleTableEntity> queryResults = await GetAllRulesFromTable();
            Dictionary<string, DeviceRuleBlobEntity> blobEntityDictionary = new Dictionary<string, DeviceRuleBlobEntity>();
            foreach (DeviceRuleTableEntity rule in queryResults)
            {
                if (rule.Enabled)
                {
                    DeviceRuleBlobEntity entity = null;
                    if (!blobEntityDictionary.ContainsKey(rule.PartitionKey))
                    {
                        entity = new DeviceRuleBlobEntity(rule.PartitionKey);
                        blobEntityDictionary.Add(rule.PartitionKey, entity);
                    }
                    else
                    {
                        entity = blobEntityDictionary[rule.PartitionKey];
                    }

                    if (rule.DataField == DeviceRuleDataFields.Temperature)
                    {
                        entity.Temperature = rule.Threshold;
                        entity.TemperatureRuleOutput = rule.RuleOutput;
                    }
                    else if (rule.DataField == DeviceRuleDataFields.Humidity)
                    {
                        entity.Humidity = rule.Threshold;
                        entity.HumidityRuleOutput = rule.RuleOutput;
                    }
                }
            }

            return blobEntityDictionary.Values.ToList();
        }

        //When we save data to the blob storage for use as ref data on an ASA job, ASA picks that
        //data up based on the current time, and the data must be finished uploading before that time.
        //
        //From the Azure Team: "What this means is your blob in the path 
        //<...>/devicerules/2015-09-23/15-24/devicerules.json needs to be uploaded before the clock 
        //strikes 2015-09-23 15:25:00 UTC, preferably before 2015-09-23 15:24:00 UTC to be used when 
        //the clock strikes 2015-09-23 15:24:00 UTC"
        //
        //If we have many devices, an upload could take a measurable amount of time.
        //
        //Also, it is possible that the ASA clock is not precisely in sync with the
        //server clock. We want to store our update on a path slightly ahead of the current time so
        //that by the time ASA reads it we will no longer be making any updates to that blob -- i.e.
        //all current changes go into a future blob. We will choose two minutes into the future. In the
        //worst case, if we make a change at 12:03:59 and our write is delayed by ten seconds (until 
        //12:04:09) it will still be saved on the path {date}\12-05 and will be waiting for ASA to 
        //find in one minute.
        private const int blobSaveMinutesInTheFuture = 2;
        private async Task PersistRulesToBlobStorageAsync(List<DeviceRuleBlobEntity> blobList)
        {
            string updatedJson = JsonConvert.SerializeObject(blobList);
            DateTime saveDate = DateTime.UtcNow.AddMinutes(blobSaveMinutesInTheFuture);
            string dateString = saveDate.ToString("d", _formatInfo);
            string timeString = saveDate.ToString("t", _formatInfo);
            string blobName = string.Format(@"{0}\{1}\{2}", dateString, timeString, _blobName);

            await _blobStorageClient.UploadTextAsync(blobName, updatedJson);
        }
    }
}
