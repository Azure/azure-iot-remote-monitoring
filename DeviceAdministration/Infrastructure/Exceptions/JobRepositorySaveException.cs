using System;

namespace Microsoft.Azure.Devices.Applications.RemoteMonitoring.DeviceAdmin.Infrastructure.Exceptions
{
    [Serializable]
    public class JobRepositorySaveException : Exception
    {
        public string JobID { get; set; }

        public JobRepositorySaveException(string jobID)
            : base($"Failed to save Job with ID = '{jobID}' into the table")
        {
            JobID = jobID;
        }
    }
}
