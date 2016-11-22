using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Microsoft.Azure.Devices.Applications.RemoteMonitoring.DeviceAdmin.Infrastructure.Models
{
    public class UserSetting
    {
        public UserSetting()
        {

        }

        public UserSetting(UserSettingTableEntity entity)
        {
            Key = entity.RowKey;
            Value = entity.SettingValue;
            Etag = entity.ETag;
        }

        public UserSetting(string key, string value)
        {
            Key = key;
            Value = value;
        }

        public string Etag { get; set; }
        public string Key { get; set; }
        public string Value { get; set; }
    }
}
