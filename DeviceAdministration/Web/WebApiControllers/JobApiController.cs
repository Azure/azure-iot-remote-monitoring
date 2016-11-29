using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web.Http;
using Microsoft.Azure.Devices.Applications.RemoteMonitoring.DeviceAdmin.Web.DataTables;
using Microsoft.Azure.Devices.Applications.RemoteMonitoring.DeviceAdmin.Web.Models;
using Microsoft.Azure.Devices.Applications.RemoteMonitoring.DeviceAdmin.Web.Security;
using Microsoft.Azure.Devices.Applications.RemoteMonitoring.DeviceAdmin.Infrastructure.Models;
using Microsoft.Azure.Devices.Applications.RemoteMonitoring.DeviceAdmin.Infrastructure.Repository;

namespace Microsoft.Azure.Devices.Applications.RemoteMonitoring.DeviceAdmin.Web.WebApiControllers
{
    [RoutePrefix("api/v1/jobs")]
    public class JobApiController : WebApiControllerBase
    {
        private readonly IJobRepository _jobRepository;
        private readonly IIoTHubDeviceManager _iotHubDeviceManager;

        public JobApiController(IJobRepository jobRepository, IIoTHubDeviceManager iotHubDeviceManager)
        {
            _jobRepository = jobRepository;
            _iotHubDeviceManager = iotHubDeviceManager;
        }

        [HttpGet]
        [Route("")]
        [WebApiRequirePermission(Permission.ViewJobs)]
        // GET: api/v1/jobs
        public async Task<HttpResponseMessage> GetJobs()
        {
            return await GetServiceResponseAsync<DataTablesResponse<DeviceJobModel>>(async () =>
            {
                var jobResponses = await _iotHubDeviceManager.GetJobResponsesAsync();

                var result = jobResponses.OrderByDescending(j => j.CreatedTimeUtc).Select(r => new DeviceJobModel(r)).ToList();
                var dataTablesResponse = new DataTablesResponse<DeviceJobModel>()
                {
                    RecordsTotal = result.Count,
                    Data = result.ToArray()
                };

                return await Task.FromResult(dataTablesResponse);

            }, false);
        }

        [HttpGet]
        [Route("summary")]
        [WebApiRequirePermission(Permission.ViewJobs)]
        // GET: api/v1/jobs/summary
        public async Task<HttpResponseMessage> GetJobSummary()
        {
            //TODO: mock code: query JobSummaryEntity
            var summaries = new List<JobSummary>();
            summaries.Add(new JobSummary() { SummaryType = JobSummary.JobSummaryType.ActiveJobs, Total = 3 });
            summaries.Add(new JobSummary() { SummaryType = JobSummary.JobSummaryType.DeviceWithScheduledJobs, Total = 35 });
            summaries.Add(new JobSummary() { SummaryType = JobSummary.JobSummaryType.FailedJobsInLast24Hours, Total = 1 });

            return await GetServiceResponseAsync<IEnumerable<JobSummary>>(async () =>
            {
                return await Task.FromResult(summaries);
            });
        }

        [HttpPut]
        [Route("{id}/cancel")]
        [WebApiRequirePermission(Permission.ManageJobs)]
        // PUT: api/v1/jobs/{id}/cancel
        public async Task<HttpResponseMessage> CancelJob(string id)
        {
            return await GetServiceResponseAsync<DeviceJobModel>(async () =>
            {
                var jobResponse = await _iotHubDeviceManager.CancelJobByJobIdAsync(id);
                return new DeviceJobModel(jobResponse);
            });
        }

        [HttpGet]
        [Route("{id}/twinjobDevices")]
        [WebApiRequirePermission(Permission.ViewJobs)]
        // GET: api/v1/jobs/{id}/twinjobDevices?deviceJobStatus={Failed}
        public async Task<HttpResponseMessage> GetTwinJobDeviceDetails(string jobId, [FromUri] DeviceJobStatus deviceJobStatus)
        {
            //TODO: mock code: cancel job      
            var devices = new List<string>() { "SampleDevice1", "SampleDevice2" };
            return await GetServiceResponseAsync<IEnumerable<string>>(async () =>
            {
                return await Task.FromResult(devices);
            });
        }


        [HttpGet]
        [Route("{id}/methodDevices")]
        [WebApiRequirePermission(Permission.ViewJobs)]
        // GET: api/v1/jobs/{id}/methodDevices?deviceJobStatus={Failed}
        public async Task<HttpResponseMessage> GetMethodJobDeviceDetails(string jobId, [FromUri] DeviceJobStatus deviceJobStatus)
        {
            //TODO: mock code: get devices within the job, and get the method return value    
            var devices = new Dictionary<string, string>();
            devices.Add("SampleDevice1", "10");
            devices.Add("SampleDevice2", "11");

            return await GetServiceResponseAsync<IDictionary<string, string>>(async () =>
             {
                 return await Task.FromResult(devices);
             });
        }
    }
}
