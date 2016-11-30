using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web.Http;
using GlobalResources;
using Microsoft.Azure.Devices.Applications.RemoteMonitoring.DeviceAdmin.Infrastructure.Repository;
using Microsoft.Azure.Devices.Applications.RemoteMonitoring.DeviceAdmin.Web.Security;

namespace Microsoft.Azure.Devices.Applications.RemoteMonitoring.DeviceAdmin.Web.WebApiControllers
{
    public class JobIndicatorsApiController : WebApiControllerBase
    {
        private readonly IIoTHubDeviceManager _iotHubDeviceManager;

        public JobIndicatorsApiController(IIoTHubDeviceManager iotHubDeviceManager)
        {
            _iotHubDeviceManager = iotHubDeviceManager;
        }

        [HttpGet]
        [Route("api/v1/jobIndicators/values")]
        [WebApiRequirePermission(Permission.ViewJobs)]
        public async Task<IEnumerable<string>> GetValues([FromUri] IEnumerable<string> indicators)
        {
            var results = new List<string>();

            foreach (var indicator in indicators)
            {
                try
                {
                    results.Add((await GetIndicatorValue(indicator)).ToString());
                }
                catch
                {
                    results.Add(Strings.NotAvailable);
                }
            }

            return results;
        }

        [HttpGet]
        [Route("api/v1/jobIndicators/definitions")]
        [WebApiRequirePermission(Permission.ViewJobs)]
        public async Task<IEnumerable<object>> GetDefinitions()
        {
            return new[]
            {
                new
                {
                    title = Strings.ActiveJobs,
                    id = "activeJobs"
                },
                new
                {
                    title = Strings.ScheduledJobs,
                    id = "scheduledJobs"
                },
                new
                {
                    title = Strings.FailedJobsInLast24Hours,
                    id = "failedJobsInLast24Hours"
                }
            };
        }

        private async Task<int> GetIndicatorValue(string indicator)
        {
            // ToDo: use a more flexiable indicator naming, such as "<status>[(timespan)]", e.g. "active", "failed(PT24H)"
            switch (indicator)
            {
                case "activeJobs":
                    return await GetActiveJobs();

                case "scheduledJobs":
                    return await GetScheduledJobs();

                case "failedJobsInLast24Hours":
                    return await GetFailedJobsInLast24Hours();

                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        private async Task<int> GetActiveJobs()
        {
            var jobs = await _iotHubDeviceManager.GetJobResponsesByStatus(JobStatus.Running);
            return jobs.Count();
        }

        private async Task<int> GetScheduledJobs()
        {
            var jobs = await _iotHubDeviceManager.GetJobResponsesByStatus(JobStatus.Scheduled);
            return jobs.Count();
        }

        private async Task<int> GetFailedJobsInLast24Hours()
        {
            var jobs = await _iotHubDeviceManager.GetJobResponsesByStatus(JobStatus.Failed);

            var oneDayAgoUtc = DateTime.UtcNow - TimeSpan.FromDays(1);
            return jobs.Count(j => j.CreatedTimeUtc < oneDayAgoUtc);
        }
    }
}