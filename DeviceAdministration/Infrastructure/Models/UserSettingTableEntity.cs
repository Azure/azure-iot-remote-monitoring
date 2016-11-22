using Microsoft.WindowsAzure.Storage.Table;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Microsoft.Azure.Devices.Applications.RemoteMonitoring.DeviceAdmin.Infrastructure.Models
{
    public class UserSettingTableEntity : TableEntity
    {
        public UserSettingTableEntity()
        {

        }

        public UserSettingTableEntity(string userId, UserSetting setting)
        {
            PartitionKey = userId;
            RowKey = setting.Key;
            SettingValue = setting.Value;
            ETag = "*";
        }

        public string SettingValue { get; set; }
    }
}
