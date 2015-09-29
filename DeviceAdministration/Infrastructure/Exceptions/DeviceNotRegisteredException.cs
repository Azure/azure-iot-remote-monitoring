namespace Microsoft.Azure.Devices.Applications.RemoteMonitoring.DeviceAdmin.Infrastructure.Exceptions
{
    public class DeviceNotRegisteredException : DeviceAdministrationExceptionBase
    {
        public DeviceNotRegisteredException(string deviceId) : base(deviceId) { }

        public override string Message
        {
            get
            {
                return string.Format(Strings.DeviceNotRegisteredExceptionMessage, DeviceId);
            }
        }
    }
}
