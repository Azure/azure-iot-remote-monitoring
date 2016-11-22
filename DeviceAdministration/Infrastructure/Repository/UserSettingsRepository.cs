using System.Threading.Tasks;
using Microsoft.Azure.Devices.Applications.RemoteMonitoring.Common.Configurations;
using Microsoft.Azure.Devices.Applications.RemoteMonitoring.Common.Helpers;
using Microsoft.Azure.Devices.Applications.RemoteMonitoring.Common.Models;
using Microsoft.Azure.Devices.Applications.RemoteMonitoring.DeviceAdmin.Infrastructure.Models;
using Microsoft.WindowsAzure.Storage.Table;

namespace Microsoft.Azure.Devices.Applications.RemoteMonitoring.DeviceAdmin.Infrastructure.Repository
{
    public class UserSettingsRepository : IUserSettingsRepository
    {

        private readonly string _storageAccountConnectionString;
        private const string _settingsTableName = "UserSettings";
        private const string _globalUserId = "global";
        private readonly IAzureTableStorageClient _azureTableStorageClient;

        public UserSettingsRepository(IConfigurationProvider configProvider, IAzureTableStorageClientFactory tableStorageClientFactory)
        {
            _storageAccountConnectionString = configProvider.GetConfigurationSettingValue("device.StorageConnectionString");
            _azureTableStorageClient = tableStorageClientFactory.CreateClient(_storageAccountConnectionString, _settingsTableName);
        }

        public async Task<UserSetting> GetUserSettingAsync(string userId, string settingKey)
        {
            var setting = await GetUserSettingImplAsync(userId, settingKey);
            if (setting == null)
            {
                setting = await GetGlobalUserSettingAsync(settingKey);
            }

            return setting;
        }

        public async Task<UserSetting> GetGlobalUserSettingAsync(string settingKey)
        {
            return await GetUserSettingImplAsync(_globalUserId, settingKey);
        }

        public async Task<UserSetting> SetUserSettingAsync(string userId, UserSetting setting, bool saveAsGlobal)
        {
            if (saveAsGlobal)
            {
                await SetUserSettingImplAsync(_globalUserId, setting);
            }

            return await SetUserSettingImplAsync(userId, setting);
        }

        private async Task<UserSetting> GetUserSettingImplAsync(string userId, string settingKey)
        {
            TableOperation query = TableOperation.Retrieve<UserSettingTableEntity>(userId, settingKey);
            TableResult response = await _azureTableStorageClient.ExecuteAsync(query);

            UserSetting result = null;
            if(response.Result != null && response.Result.GetType() == typeof(UserSettingTableEntity))
            {
                result = new UserSetting((UserSettingTableEntity)response.Result);
            }

            return result;
        }

        private async Task<UserSetting> SetUserSettingImplAsync(string userId, UserSetting setting)
        {
            var incomingEntity = new UserSettingTableEntity(userId, setting);

            if (!string.IsNullOrWhiteSpace(setting.Etag))
            {
                incomingEntity.ETag = setting.Etag;
            }

            TableStorageResponse<UserSetting> result = await _azureTableStorageClient.DoTableInsertOrReplaceAsync<UserSetting, UserSettingTableEntity>(incomingEntity, (tableEntity) =>
                {
                    if (tableEntity == null)
                    {
                        return null;
                    }

                    return new UserSetting(tableEntity);
                });

            return result.Entity;
        }
    }
}
