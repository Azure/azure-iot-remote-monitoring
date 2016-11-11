using Microsoft.Azure.Devices.Applications.RemoteMonitoring.DeviceAdmin.Infrastructure.Models;
using Microsoft.Azure.Devices.Applications.RemoteMonitoring.DeviceAdmin.Web.Security;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web.Http;

namespace Microsoft.Azure.Devices.Applications.RemoteMonitoring.DeviceAdmin.Web.WebApiControllers
{
    [RoutePrefix("api/v1/jobs")]
    public class JobApiController : WebApiControllerBase
    {
        public JobApiController()
        {
        }

        [HttpGet]
        [Route("")]
        [WebApiRequirePermission(Permission.ViewJobs)]
        // GET: api/v1/jobs
        public async Task<HttpResponseMessage> GetJobs()
        {
            //TODO: mock code: query Job
            var jobs = new List<Job>();
            jobs.Add(new Job() { Name = "sample job1", StatusMessage = "Need to mock some of this using reflection." });

            return await GetServiceResponseAsync<IEnumerable<Job>>(async () =>
            {
                return await Task.FromResult(jobs);
            });
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

        [HttpPost]
        [Route("")]
        [WebApiRequirePermission(Permission.ManageJobs)]
        // Post: api/v1/jobs
        public async Task<HttpResponseMessage> ScheduleJob()
        {
            //TODO: mock code: Add Job
            var job = new Job() { Id = "jobid1", Name = "frist job", OperationType = Job.JobOperationType.EditPropertyOrTag, QueryName = "sample query 1" };
            return await GetServiceResponseAsync<Job>(async () =>
            {
                return await Task.FromResult(job);
            });
        }

        [HttpGet]
        [Route("{id}")]
        [WebApiRequirePermission(Permission.ViewJobs)]
        // GET: api/v1/jobs/{id}
        public async Task<HttpResponseMessage> GetJobById(string jobId)
        {
            //TODO: mock code: query Job
            var jobs = new List<Job>();
            jobs.Add(new Job() { Id = "jobid1", Name = "frist job", OperationType = Job.JobOperationType.EditPropertyOrTag, QueryName = "sample query 1" });
            jobs.Add(new Job() { Id = "jobid2", Name = "second job", OperationType = Job.JobOperationType.InvokeMethod, QueryName = "sample query 2" });

            return await GetServiceResponseAsync<IEnumerable<Job>>(async () =>
            {
                return await Task.FromResult(jobs);
            });
        }

        [HttpPut]
        [Route("{id}/cancel")]
        [WebApiRequirePermission(Permission.ManageJobs)]
        // GET: api/v1/jobs/{id}/cancel
        public async Task<HttpResponseMessage> CancelJob(string jobId)
        {
            //TODO: mock code: cancel job            
            return await GetServiceResponseAsync<bool>(async () =>
            {
                return await Task.FromResult(true);
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
