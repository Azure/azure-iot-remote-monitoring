namespace Microsoft.Azure.Devices.Applications.RemoteMonitoring.DeviceAdmin.Infrastructure.Exceptions
{
    public class UnsupportedCommandException : DeviceAdministrationExceptionBase
    {
        public UnsupportedCommandException(string deviceId, string commandName)
            : base(deviceId)
        {
            CommandName = commandName;
        }

        public string CommandName { get; set; }

        public override string Message
        {
            get
            {
                return string.Format(Strings.UnsupportedCommandExceptionMessage, DeviceId, CommandName);
            }
        }
    }
}
