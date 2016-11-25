using System;
using System.Collections.Generic;
using Microsoft.Azure.Devices.Applications.RemoteMonitoring.Common.Helpers;
using Microsoft.Azure.Devices.Applications.RemoteMonitoring.DeviceAdmin.Web.Extensions;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace Microsoft.Azure.Devices.Applications.RemoteMonitoring.DeviceAdmin.Web.Models
{
    public class DeviceJobModel
    {
        public DeviceJobModel(JobResponse jobResponse)
        {
            Status = jobResponse.Status;
            JobId = jobResponse.JobId;
            DeviceCount = jobResponse.DeviceJobStatistics.DeviceCount;
            SucceededCount = jobResponse.DeviceJobStatistics.SucceededCount;
            FailedCount = jobResponse.DeviceJobStatistics.FailedCount;
            PendingCount = jobResponse.DeviceJobStatistics.PendingCount;
            RunningCount = jobResponse.DeviceJobStatistics.RunningCount;
            OperationType = jobResponse.Type.LocalizedString();
            StartTime = jobResponse.StartTimeUtc;
            EndTime = jobResponse.EndTimeUtc;
        }

        public string JobId { get; set; }
        public string JobName { get; set; }
        [JsonConverter(typeof(StringEnumConverter))]
        public JobStatus Status { get; set; }
        public string QueryName { get; set; }
        public string OperationType { get; set; }
        public DateTime? StartTime { get; set; }
        public DateTime? EndTime { get; set; }
        public int DeviceCount { get; set; }
        public int SucceededCount { get; set; }
        public int FailedCount { get; set; }
        public int PendingCount { get; set; }
        public int RunningCount { get; set; }

        /// <summary>
        /// This is a mock method to provide a list of job responses
        /// for Web UI. Will remove it later (ToDo).
        /// </summary>
        /// <returns></returns>
        public static List<DeviceJobModel> BuildMockJobs()
        {
            List<DeviceJobModel> jobs = new List<DeviceJobModel>();
            var job1 = new DeviceJobModel(new JobResponse
            {
                DeviceJobStatistics = new DeviceJobStatistics
                {
                    DeviceCount = 10,
                    SucceededCount = 9,
                    FailedCount = 1,
                    PendingCount = 0,
                    RunningCount = 0,
                }
            });
            job1.JobId = "job1";
            job1.JobName = "sample job1";
            job1.QueryName = "MyNewQuery1";
            job1.StartTime = DateTime.UtcNow.AddHours(-1.5);
            job1.EndTime = DateTime.UtcNow.AddHours(-0.5);
            job1.Status = JobStatus.Completed;
            job1.OperationType = JobType.ScheduleUpdateTwin.LocalizedString();

            var job2 = new DeviceJobModel(new JobResponse
            {
                DeviceJobStatistics = new DeviceJobStatistics
                {
                    DeviceCount = 100,
                    SucceededCount = 95,
                    FailedCount = 5,
                    PendingCount = 0,
                    RunningCount = 0,
                },
            });
            job2.JobId = "job2";
            job2.JobName = "sample job2";
            job2.QueryName = "MyNewQuery2";
            job2.StartTime = DateTime.UtcNow.AddHours(-0.5);
            job2.EndTime = DateTime.UtcNow.AddHours(-0.1);
            job2.Status = JobStatus.Scheduled;
            job2.OperationType = JobType.ScheduleDeviceMethod.LocalizedString();

            var job3 = new DeviceJobModel(new JobResponse
            {
                DeviceJobStatistics = new DeviceJobStatistics
                {
                    DeviceCount = 100,
                    SucceededCount = 10,
                    FailedCount = 90,
                    PendingCount = 0,
                    RunningCount = 0,
                },
            });
            job3.JobId = "job3";
            job3.JobName = "sample job3";
            job3.QueryName = "MyNewQuery3";
            job3.StartTime = DateTime.UtcNow.AddHours(-1.8);
            job3.EndTime = DateTime.UtcNow.AddHours(-0.2);
            job3.Status = JobStatus.Failed;
            job3.OperationType = JobType.ScheduleDeviceMethod.LocalizedString();

            jobs.Add(job1);
            jobs.Add(job2);
            jobs.Add(job3);
            return jobs;
        }
    }
}