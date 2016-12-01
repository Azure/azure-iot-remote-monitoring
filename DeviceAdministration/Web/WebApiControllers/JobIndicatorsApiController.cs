using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Web.Http;
using System.Xml;
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
                catch (Exception ex)
                {
                    Trace.TraceError($"Exception raised while retrieving value of job indicator {indicator}: {ex}");
                    results.Add(Strings.NotAvailable);
                }
            }

            return results;
        }

        [HttpGet]
        [Route("api/v1/jobIndicators/definitions")]
        [WebApiRequirePermission(Permission.ViewJobs)]
        public IEnumerable<object> GetDefinitions()
        {
            return new[]
            {
                new
                {
                    title = Strings.ActiveJobs,
                    id = "Running"
                },
                new
                {
                    title = Strings.ScheduledJobs,
                    id = "Scheduled"
                },
                new
                {
                    title = Strings.FailedJobsInLast24Hours,
                    id = "Failed(P1D)"
                },
                new
                {
                    title = Strings.CompletedJobsInLast24Hours,
                    id = "Completed(P1D)"
                }
            };
        }

        private async Task<int> GetIndicatorValue(string indicator)
        {
            var regex = new Regex(@"(?<status>\w+)(\((?<timespan>\w+)\))?");

            var match = regex.Match(indicator);
            if (!match.Success)
            {
                throw new ArgumentOutOfRangeException();
            }

            var status = (JobStatus)Enum.Parse(typeof(JobStatus), match.Groups["status"].Value);

            if (string.IsNullOrWhiteSpace(match.Groups["timespan"].Value))
            {
                return await GetJobCountByStatusAsync(status);
            }
            else
            {
                var timespan = XmlConvert.ToTimeSpan(match.Groups["timespan"].Value);
                return await GetJobCountByStatusAndTimespanAsync(status, timespan);
            }
        }

        private async Task<int> GetJobCountByStatusAsync(JobStatus status)
        {
            var jobs = await _iotHubDeviceManager.GetJobResponsesByStatus(status);

            return jobs.Count();
        }

        private async Task<int> GetJobCountByStatusAndTimespanAsync(JobStatus status, TimeSpan timespan)
        {
            var jobs = await _iotHubDeviceManager.GetJobResponsesByStatus(status);

            var timeLimit = DateTime.UtcNow - timespan;
            return jobs.Count(j => j.CreatedTimeUtc >= timeLimit);
        }
    }
}