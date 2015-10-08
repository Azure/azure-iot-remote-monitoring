using System;
using System.Globalization;
using System.Runtime.Serialization;
using System.Security.Permissions;

namespace Microsoft.Azure.Devices.Applications.RemoteMonitoring.DeviceAdmin.Infrastructure.Exceptions
{
    [Serializable]
    public class DeviceRegistrationException : DeviceAdministrationExceptionBase
    {
        public DeviceRegistrationException(string deviceId, Exception innerException) : base(deviceId, innerException)
        {
        }

        // protected constructor for deserialization
        [SecurityPermission(SecurityAction.Demand, SerializationFormatter = true)]
        protected DeviceRegistrationException(SerializationInfo info, StreamingContext context) : base(info, context)
        {
        }

        public override string Message
        {
            get
            {
                return string.Format(
                    CultureInfo.CurrentCulture,
                    Strings.DeviceRegistrationExceptionMessage, 
                    DeviceId);
            }
        }
    }
}
