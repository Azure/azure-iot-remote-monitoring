using Microsoft.WindowsAzure.Storage.Table;

namespace Microsoft.Azure.Devices.Applications.RemoteMonitoring.DeviceAdmin.Infrastructure.Models
{
    public class JobTableEntity : TableEntity
    {
        public string JobId { get; set; }

        public string FilterId { get; set; }

        public string FilterName { get; set; }

        public string JobName { get; set; }

        public string MethodName { get; set; }

        public string JobType { get; set; }

        public JobTableEntity()
        {
        }

        public JobTableEntity(JobRepositoryModel job)
        {
            PartitionKey = JobId = job.JobId;
            RowKey = FilterId = job.FilterId;
            JobName = job.JobName;
            FilterName = job.FilterName;
            MethodName = job.MethodName;
            JobType = job.JobType.ToString();
        }
    }
}