using System;

namespace Microsoft.Azure.Devices.Applications.RemoteMonitoring.DeviceAdmin.Infrastructure.Exceptions
{
    [Serializable]
    public class JobNotFoundException : Exception
    {
        public string JobID { get; set; }

        public JobNotFoundException(string jobID)
            : base($"Job with ID = '{jobID}' could not be found in the table")
        {
            JobID = jobID;
        }
    }
}
