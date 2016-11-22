using System;

namespace Microsoft.Azure.Devices.Applications.RemoteMonitoring.DeviceAdmin.Infrastructure.Exceptions
{
    [Serializable]
    public class DuplicatedJobFoundException : Exception
    {
        public string JobID { get; set; }

        public DuplicatedJobFoundException(string jobID)
            : base($"Multiplte jobs with the same ID ({jobID}) were found in the table")
        {
            JobID = jobID;
        }
    }
}
