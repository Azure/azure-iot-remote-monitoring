using Microsoft.Azure.Devices.Applications.RemoteMonitoring.Common.Configurations;
using Microsoft.Azure.Devices.Applications.RemoteMonitoring.Common.Helpers;
using Microsoft.Azure.Devices.Applications.RemoteMonitoring.Common.Models;
using Microsoft.Azure.Devices.Applications.RemoteMonitoring.DeviceAdmin.Infrastructure.Models;
using Microsoft.WindowsAzure.Storage.Table;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Microsoft.Azure.Devices.Applications.RemoteMonitoring.DeviceAdmin.Infrastructure.Repository
{
    public class UserSettingsRepository : IUserSettingsRepository
    {

        private readonly string _storageAccountConnectionString;
        private const string _settingsTableName = "UserSettings";
        private const string _settingsTablePartitionKey = "settings";

        public UserSettingsRepository(IConfigurationProvider configProvider)
        {
            _storageAccountConnectionString = configProvider.GetConfigurationSettingValue("device.StorageConnectionString");
        }

        public async Task<UserSetting> GetUserSettingValueAsync(string settingKey)
        {
            var settingsTable = await AzureTableStorageHelper.GetTableAsync(_storageAccountConnectionString, _settingsTableName);
            TableOperation query = TableOperation.Retrieve<UserSettingTableEntity>(_settingsTablePartitionKey, settingKey);

            TableResult response = await Task.Run(() =>
                settingsTable.Execute(query)
            );

            UserSetting result = null;
            if(response.Result != null && response.Result.GetType() == typeof(UserSettingTableEntity))
            {
                result = new UserSetting
                {
                    Etag = ((UserSettingTableEntity)response.Result).ETag,
                    Key = ((UserSettingTableEntity)response.Result).RowKey,
                    Value = ((UserSettingTableEntity)response.Result).SettingValue
                };
            }

            return result;
        }

        public async Task<TableStorageResponse<UserSetting>> SetUserSettingValueAsync(UserSetting setting)
        {
            var incomingEntity =
                new UserSettingTableEntity()
                {
                    PartitionKey = _settingsTablePartitionKey,
                    RowKey = setting.Key,
                    SettingValue = setting.Value
                };

            if (!string.IsNullOrWhiteSpace(setting.Etag))
            {
                incomingEntity.ETag = setting.Etag;
            }

            TableStorageResponse<UserSetting> result = await AzureTableStorageHelper.DoTableInsertOrReplaceAsync<UserSetting, UserSettingTableEntity>(incomingEntity, (tableEntity) =>
                {
                    if (tableEntity == null)
                    {
                        return null;
                    }

                    var updatedSetting = new UserSetting()
                    {
                        Key = tableEntity.RowKey,
                        Value = tableEntity.SettingValue,
                        Etag = tableEntity.ETag
                    };

                    return updatedSetting;
                }, _storageAccountConnectionString, _settingsTableName);

            return result;
        }
    }
}
