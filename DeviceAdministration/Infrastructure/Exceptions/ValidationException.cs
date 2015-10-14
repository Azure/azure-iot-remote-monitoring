using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using System.Security.Permissions;

namespace Microsoft.Azure.Devices.Applications.RemoteMonitoring.DeviceAdmin.Infrastructure.Exceptions
{
    [Serializable]
    public class ValidationException : DeviceAdministrationExceptionBase
    {
        public ValidationException(string deviceId) : base(deviceId)
        {
            Errors = new List<string>();
        }

        public ValidationException(string deviceId, Exception innerException) : base(deviceId, innerException)
        {
            Errors = new List<string>();
        }

        [SecurityPermission(SecurityAction.Demand, SerializationFormatter = true)]
        protected ValidationException(SerializationInfo info, StreamingContext context) : base(info, context)
        {
            if (info == null)
            {
                throw new ArgumentNullException("info");
            }

            Errors = (IList<string>)info.GetValue("Errors", typeof(IList<string>));
        }

        public IList<string> Errors { get; set; }

        [SecurityPermission(SecurityAction.Demand, SerializationFormatter = true)]
        public override void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            if (info == null)
            {
                throw new ArgumentNullException("info");
            }

            info.AddValue("Errors", Errors, typeof(IList<string>));
            base.GetObjectData(info, context);
        }
    }
}
