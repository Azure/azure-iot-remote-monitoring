using System;
using System.Runtime.Serialization;

namespace Microsoft.Azure.Devices.Applications.RemoteMonitoring.DeviceAdmin.Infrastructure.Exceptions
{
    [Serializable]
    public class JobRepositoryRemoveException : Exception
    {
        public string JobID { get; set; }

        public JobRepositoryRemoveException(string jobID)
            : base(FormattableString.Invariant($"Failed to delete Job with ID = '{jobID}' from the table"))
        {
            JobID = jobID;
        }

        public override void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            base.GetObjectData(info, context);
        }
    }
}
