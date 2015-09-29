namespace Microsoft.Azure.Devices.Applications.RemoteMonitoring.DeviceAdmin.Infrastructure.Exceptions
{
    public class DeviceAlreadyRegisteredException : DeviceAdministrationExceptionBase
    {
        public DeviceAlreadyRegisteredException(string deviceId) : base(deviceId) { }

        public override string Message
        {
            get
            {
                return string.Format(Strings.DeviceAlreadyRegisteredExceptionMessage, DeviceId);
            }
        }
    }
}
