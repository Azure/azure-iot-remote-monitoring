namespace Microsoft.Azure.Devices.Applications.RemoteMonitoring.DeviceAdmin.Infrastructure.Models
{
    public class JobRepositoryModel
    {
        public string JobId { get; private set; }
        public string FilterId { get; private set; }
        public string JobName { get; private set; }
        public string FilterName { get; set; }
        public string MethodName { get; set; }

        public JobRepositoryModel(JobTableEntity e)
        {
            JobId = e.JobId;
            JobName = e.JobName;
            FilterName = e.FilterName;
            // Both FilterId and FilterName should be empty when the filter deleted
            FilterId = string.IsNullOrEmpty(e.FilterName) ? string.Empty : e.FilterId;
            MethodName = e.MethodName;
        }

        public JobRepositoryModel(string jobId, string filterId, string jobName, string filterName, string methodName = null)
        {
            JobId = jobId;
            JobName = jobName;
            FilterId = filterId;
            FilterName = filterName;
            MethodName = methodName;
        }
    }
}
