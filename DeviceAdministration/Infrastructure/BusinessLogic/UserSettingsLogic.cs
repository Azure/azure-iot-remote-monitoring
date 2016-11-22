using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Azure.Devices.Applications.RemoteMonitoring.DeviceAdmin.Infrastructure.Models;
using Microsoft.Azure.Devices.Applications.RemoteMonitoring.DeviceAdmin.Infrastructure.Repository;
using Newtonsoft.Json;

namespace Microsoft.Azure.Devices.Applications.RemoteMonitoring.DeviceAdmin.Infrastructure.BusinessLogic
{
    /// <summary>
    /// Business logic around different types of devices
    /// </summary>
    public class UserSettingsLogic : IUserSettingsLogic
    {
        private readonly IUserSettingsRepository _userSettingsRepository;
        private const string _deviceListColumnsKey = "deviceListColumns";

        public UserSettingsLogic(IUserSettingsRepository userSettingsRepository)
        {
            _userSettingsRepository = userSettingsRepository;
        }

        public async Task<IEnumerable<DeviceListColumns>> GetDeviceListColumnsAsync(string userId)
        {
            var setting = await _userSettingsRepository.GetUserSettingAsync(userId, _deviceListColumnsKey);

            return setting != null ? JsonConvert.DeserializeObject<IEnumerable<DeviceListColumns>>(setting.Value) : null;
        }

        public async Task<IEnumerable<DeviceListColumns>> GetGlobalDeviceListColumnsAsync()
        {
            var setting = await _userSettingsRepository.GetGlobalUserSettingAsync(_deviceListColumnsKey);

            return setting != null ? JsonConvert.DeserializeObject<IEnumerable<DeviceListColumns>>(setting.Value) : null;
        }

        public async Task<bool> SetDeviceListColumnsAsync(string userId, IEnumerable<DeviceListColumns> columns, bool saveAsGlobal = false)
        {
            var setting = new UserSetting(_deviceListColumnsKey, JsonConvert.SerializeObject(columns));
            var result = await _userSettingsRepository.SetUserSettingAsync(userId, setting, saveAsGlobal);

            return (result != null);
        }
    }
}
