using System;
using System.Runtime.Serialization;

namespace Microsoft.Azure.Devices.Applications.RemoteMonitoring.DeviceAdmin.Infrastructure.Exceptions
{
    [Serializable]
    public class JobNotFoundException : Exception
    {
        public string JobID { get; set; }

        public JobNotFoundException(string jobID)
            : base(FormattableString.Invariant($"Job with ID = '{jobID}' could not be found in the table"))
        {
            JobID = jobID;
        }

        public override void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            base.GetObjectData(info, context);
        }
    }
}
