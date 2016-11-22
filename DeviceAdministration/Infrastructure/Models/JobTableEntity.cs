using Microsoft.WindowsAzure.Storage.Table;

namespace Microsoft.Azure.Devices.Applications.RemoteMonitoring.DeviceAdmin.Infrastructure.Models
{
    public class JobTableEntity : TableEntity
    {
        public string JobId { get; set; }

        public string QueryName { get; set; }

        public string JobName { get; set; }

        public JobTableEntity()
        {
        }

        public JobTableEntity(JobRepositoryModel job)
        {
            PartitionKey = JobId = job.JobId;
            RowKey = QueryName = job.QueryName;
            JobName = job.JobName;
        }
    }
}