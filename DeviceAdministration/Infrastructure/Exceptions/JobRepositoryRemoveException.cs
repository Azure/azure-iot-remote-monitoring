using System;

namespace Microsoft.Azure.Devices.Applications.RemoteMonitoring.DeviceAdmin.Infrastructure.Exceptions
{
    [Serializable]
    public class JobRepositoryRemoveException : Exception
    {
        public string JobID { get; set; }

        public JobRepositoryRemoveException(string jobID)
            : base($"Failed to delete Job with ID = '{jobID}' from the table")
        {
            JobID = jobID;
        }
    }
}
