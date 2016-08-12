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
        private readonly IBlobStorageClient _blobStorageManager;
        private readonly string _blobName;

        public ActionMappingRepository(IConfigurationProvider configurationProvider, IBlobStorageClientFactory blobStorageClientFactory)
        {
            string blobName = configurationProvider.GetConfigurationSettingValue("ActionMappingStoreBlobName");
            string connectionString = configurationProvider.GetConfigurationSettingValue("device.StorageConnectionString");
            string containerName = configurationProvider.GetConfigurationSettingValue("ActionMappingStoreContainerName");
            _blobName = blobName;
            _blobStorageManager = blobStorageClientFactory.CreateClient(connectionString, containerName);
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

            await _blobStorageManager.UploadFromByteArrayAsync(
                _blobName,
                newBytes,
                0,
                newBytes.Length,
                AccessCondition.GenerateIfMatchCondition(existingResults.ETag),
                null,
                null);
        }

        private async Task<ActionMappingBlobResults> GetActionsAndEtagAsync()
        {
            var mappings = new List<ActionMapping>();
            byte[] blobData = await _blobStorageManager.GetBlobData(_blobName);

            if (blobData != null && blobData.Length > 0)
            {
                // get the existing mappings in object form
                string existingJsonData = Encoding.UTF8.GetString(blobData);
                mappings = JsonConvert.DeserializeObject<List<ActionMapping>>(existingJsonData);
                string etag = await _blobStorageManager.GetBlobEtag(_blobName);
                return new ActionMappingBlobResults(mappings, etag);
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