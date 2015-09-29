using System;

namespace Microsoft.Azure.IoT.Samples.DeviceManagement.Infrastructure.Exceptions
{
    public class DeviceKeyTypeNotFoundException : Exception
    {
        public string DeviceId { get; set; }
        public string KeyType { get; set; }

        public override string Message
        {
            get
            {
                return string.Format(Strings.DeviceKeysTypeNotFoundException, KeyType, DeviceId);
            }
        }

        public DeviceKeyTypeNotFoundException(string deviceId, string keyType)
        {
            DeviceId = deviceId;
            KeyType = keyType;
        }
    }
}
