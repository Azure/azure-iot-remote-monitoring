namespace Microsoft.Azure.Devices.Applications.RemoteMonitoring.DeviceAdmin.Infrastructure.Models
{
    public class JobRepositoryModel
    {
        public string JobId { get; private set; }
        public string QueryName { get; private set; }
        public string JobName { get; private set; }

        public JobRepositoryModel(JobTableEntity e)
        {
            JobId = e.JobId;
            QueryName = e.QueryName;
            JobName = e.JobName;
        }

        public JobRepositoryModel(string jobId, string queryName, string jobName)
        {
            JobId = jobId;
            QueryName = queryName;
            JobName = jobName;
        }
    }
}
