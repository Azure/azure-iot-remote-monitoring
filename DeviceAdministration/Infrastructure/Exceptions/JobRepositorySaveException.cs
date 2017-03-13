using System;
using System.Runtime.Serialization;

namespace Microsoft.Azure.Devices.Applications.RemoteMonitoring.DeviceAdmin.Infrastructure.Exceptions
{
    [Serializable]
    public class JobRepositorySaveException : Exception
    {
        public string JobID { get; set; }

        public JobRepositorySaveException(string jobID)
            : base(FormattableString.Invariant($"Failed to save Job with ID = '{jobID}' into the table"))
        {
            JobID = jobID;
        }

        public override void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            base.GetObjectData(info, context);
        }
    }
}
