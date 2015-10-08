using System;
using System.Globalization;
using System.Runtime.Serialization;
using System.Security.Permissions;

namespace Microsoft.Azure.Devices.Applications.RemoteMonitoring.DeviceAdmin.Infrastructure.Exceptions
{
    [Serializable]
    public class UnsupportedCommandException : DeviceAdministrationExceptionBase
    {
        public UnsupportedCommandException(string deviceId, string commandName) : base(deviceId)
        {
            CommandName = commandName;
        }

        // protected constructor for deserialization
        [SecurityPermission(SecurityAction.Demand, SerializationFormatter = true)]
        protected UnsupportedCommandException(SerializationInfo info, StreamingContext context) : base(info, context)
        {
            if (info == null)
            {
                throw new ArgumentNullException("info");
            }

            this.CommandName = info.GetString("CommandName");
        }

        public string CommandName { get; set; }

        public override string Message
        {
            get
            {
                return string.Format(
                    CultureInfo.CurrentCulture,
                    Strings.UnsupportedCommandExceptionMessage, 
                    DeviceId, 
                    CommandName);
            }
        }

        [SecurityPermission(SecurityAction.Demand, SerializationFormatter = true)]
        public override void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            if (info == null)
            {
                throw new ArgumentNullException("info");
            }

            info.AddValue("CommandName", CommandName);
            base.GetObjectData(info, context);
        }
    }
}
