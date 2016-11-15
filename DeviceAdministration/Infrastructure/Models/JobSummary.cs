namespace Microsoft.Azure.Devices.Applications.RemoteMonitoring.DeviceAdmin.Infrastructure.Models
{
    public class JobSummary
    {
        public JobSummaryType SummaryType { get; set; }

        public int Total { get; set; }

        public enum JobSummaryType
        {
            ActiveJobs,
            DeviceWithScheduledJobs,
            FailedJobsInLast24Hours
        }
    }
}
