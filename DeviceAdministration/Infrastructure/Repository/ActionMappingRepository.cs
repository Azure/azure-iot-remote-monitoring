using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Azure.Devices.Applications.RemoteMonitoring.Common.Configurations;
using Microsoft.Azure.Devices.Applications.RemoteMonitoring.Common.Helpers;
using Microsoft.Azure.Devices.Applications.RemoteMonitoring.DeviceAdmin.Infrastructure.Models;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Blob;
using Newtonsoft.Json;

namespace Microsoft.Azure.Devices.Applications.RemoteMonitoring.DeviceAdmin.Infrastructure.Repository
{
    public class ActionMappingRepository : IActionMappingRepository
    {
        private readonly string _connectionString;
        private readonly string _containerName;  // must be lower case!
        private readonly string _blobName;

        public ActionMappingRepository(IConfigurationProvider configurationProvider)
        {
            _connectionString = configurationProvider.GetConfigurationSettingValue("device.StorageConnectionString");
            _containerName = configurationProvider.GetConfigurationSettingValue("ActionMappingStoreContainerName");
            _blobName = configurationProvider.GetConfigurationSettingValue("ActionMappingStoreBlobName");
        }

        public async Task<List<ActionMapping>> GetAllMappingsAsync()
        {
            ActionMappingBlobResults results = await GetActionsAndEtagAsync();
            return results.ActionMappings;
        }

        public async Task SaveMappingAsync(ActionMapping m)
        {
            ActionMappingBlobResults existingResults = await GetActionsAndEtagAsync();

            List<ActionMapping> existingMappings = existingResults.ActionMappings;

            // look for the new mapping
            var ruleoutput = m.RuleOutput;
            ActionMapping found = existingMappings.FirstOrDefault(a => a.RuleOutput.ToLower() == m.RuleOutput.ToLower());

            if (found == null)
            {
                // add the new mapping
                existingMappings.Add(m);
            }
            else
            {
                // update the ActionId for the found mapping
                found.ActionId = m.ActionId;
            }

            // now save back to the blob
            string newJsonData = JsonConvert.SerializeObject(existingMappings);
            byte[] newBytes = Encoding.UTF8.GetBytes(newJsonData);

            CloudBlobContainer container = await BlobStorageHelper.BuildBlobContainerAsync(_connectionString, _containerName);
            CloudBlockBlob blob = container.GetBlockBlobReference(_blobName);

            await blob.UploadFromByteArrayAsync(
                newBytes,
                0,
                newBytes.Length,
                AccessCondition.GenerateIfMatchCondition(existingResults.ETag),
                null,
                null);
        }

        private async Task<ActionMappingBlobResults> GetActionsAndEtagAsync()
        {
            CloudBlobContainer container = await BlobStorageHelper.BuildBlobContainerAsync(_connectionString, _containerName);
            CloudBlockBlob blob = container.GetBlockBlobReference(_blobName);
            bool exists = await blob.ExistsAsync();

            var mappings = new List<ActionMapping>();

            if (exists)
            {
                await blob.FetchAttributesAsync();
                long blobLength = blob.Properties.Length;

                if (blobLength > 0)
                {
                    byte[] existingBytes = new byte[blobLength];
                    await blob.DownloadToByteArrayAsync(existingBytes, 0);

                    // get the existing mappings in object form
                    string existingJsonData = Encoding.UTF8.GetString(existingBytes);
                    mappings = JsonConvert.DeserializeObject<List<ActionMapping>>(existingJsonData);
                }

                return new ActionMappingBlobResults(mappings, blob.Properties.ETag);
            }

            // if it doesn't exist, return the empty list and an empty string for the ETag
            return new ActionMappingBlobResults(mappings, "");
        }

        private class ActionMappingBlobResults
        {
            public ActionMappingBlobResults(List<ActionMapping> actionMappings, string eTag)
            {
                ActionMappings = actionMappings;
                ETag = eTag;
            }

            public List<ActionMapping> ActionMappings { get; private set; }
            public string ETag { get; private set; }
        }
    }
}