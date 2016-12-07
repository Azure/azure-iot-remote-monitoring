namespace Microsoft.Azure.Devices.Applications.RemoteMonitoring.DeviceAdmin.Infrastructure.Models
{
    public class JobRepositoryModel
    {
        public string JobId { get; private set; }
        public string FilterId { get; private set; }
        public string JobName { get; private set; }
        public string FilterName { get; private set; }

        public JobRepositoryModel(JobTableEntity e)
        {
            JobId = e.JobId;
            FilterId = e.FilterId;
            JobName = e.JobName;
            FilterName = e.FilterName;
        }

        public JobRepositoryModel(string jobId, string filterId, string jobName, string filterName)
        {
            JobId = jobId;
            JobName = jobName;
            FilterId = filterId;
            FilterName = filterName;
        }
    }
}
