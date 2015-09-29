using System;

namespace Microsoft.Azure.Devices.Applications.RemoteMonitoring.DeviceAdmin.Infrastructure.Exceptions
{
    public class DeviceRegistrationException : DeviceAdministrationExceptionBase
    {
        public DeviceRegistrationException(string deviceId, Exception innerException) : base(deviceId, innerException) { }

        public override string Message
        {
            get
            {
                return string.Format(Strings.DeviceRegistrationExceptionMessage, DeviceId);
            }
        }
    }
}
