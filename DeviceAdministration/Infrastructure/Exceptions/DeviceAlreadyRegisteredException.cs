using System;
using System.Globalization;
using System.Runtime.Serialization;
using System.Security.Permissions;

namespace Microsoft.Azure.Devices.Applications.RemoteMonitoring.DeviceAdmin.Infrastructure.Exceptions
{
    [Serializable]
    public class DeviceAlreadyRegisteredException : DeviceAdministrationExceptionBase
    {
        public DeviceAlreadyRegisteredException(string deviceId) : base(deviceId)
        {
        }

        // protected constructor for deserialization
        [SecurityPermission(SecurityAction.Demand, SerializationFormatter = true)]
        protected DeviceAlreadyRegisteredException(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
        }

        public override string Message
        {
            get
            {
                return string.Format(
                    CultureInfo.CurrentCulture,
                    Strings.DeviceAlreadyRegisteredExceptionMessage, 
                    DeviceId);
            }
        }
    }
}
