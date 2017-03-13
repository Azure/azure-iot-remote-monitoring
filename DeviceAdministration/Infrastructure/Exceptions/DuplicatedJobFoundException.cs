using System;
using System.Runtime.Serialization;

namespace Microsoft.Azure.Devices.Applications.RemoteMonitoring.DeviceAdmin.Infrastructure.Exceptions
{
    [Serializable]
    public class DuplicatedJobFoundException : Exception
    {
        public string JobID { get; set; }

        public DuplicatedJobFoundException(string jobID)
            : base(FormattableString.Invariant($"Multiplte jobs with the same ID ({jobID}) were found in the table"))
        {
            JobID = jobID;
        }

        public override void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            base.GetObjectData(info, context);
        }
    }
}
