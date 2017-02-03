using System;

namespace Microsoft.Azure.Devices.Applications.RemoteMonitoring.DeviceAdmin.Infrastructure.Models
{
    public class JobRepositoryModel
    {
        public string JobId { get; private set; }
        public string FilterId { get; private set; }
        public string JobName { get; private set; }
        public string FilterName { get; set; }
        public string MethodName { get; set; }
        public ExtendJobType JobType { get; set; }
        public string LocalizedJobTypeString { get; internal set; }

        public JobRepositoryModel(JobTableEntity e)
        {
            JobId = e.JobId;
            JobName = e.JobName;
            FilterName = e.FilterName;
            // Both FilterId and FilterName should be empty when the filter deleted
            FilterId = string.IsNullOrEmpty(e.FilterName) ? string.Empty : e.FilterId;
            MethodName = e.MethodName;
            ExtendJobType value;
            // Use default value if ExtendJobType is not stored in the table or stored but not recognized.
            if (!Enum.TryParse(e.JobType, out value))
            {
                JobType = ExtendJobType.Unknown;
            }
            else
            {
                JobType = value;
            }
        }

        public JobRepositoryModel(string jobId, string filterId, string jobName, string filterName, ExtendJobType jobType, string methodName = null)
        {
            JobId = jobId;
            JobName = jobName;
            FilterId = filterId;
            FilterName = filterName;
            MethodName = methodName;
            JobType = jobType;
        }

        public JobRepositoryModel(string jobId, string filterId, string jobName, string filterName, string localizedJobTypeString)
        {
            JobId = jobId;
            JobName = jobName;
            FilterId = filterId;
            FilterName = filterName;
            LocalizedJobTypeString = localizedJobTypeString;
        }
    }

    public enum ExtendJobType
    {
        Unknown = JobType.Unknown,
        ExportDevices = JobType.ExportDevices,
        ImportDevices = JobType.ImportDevices,
        ScheduleDeviceMethod = JobType.ScheduleDeviceMethod,
        ScheduleUpdateTwin = JobType.ScheduleUpdateTwin,
        ScheduleUpdateIcon = JobType.ScheduleUpdateTwin + 1,
        ScheduleRemoveIcon = JobType.ScheduleUpdateTwin + 2
    }
}
