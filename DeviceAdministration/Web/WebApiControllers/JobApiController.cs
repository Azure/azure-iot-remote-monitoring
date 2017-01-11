using GlobalResources;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web.Http;
using Microsoft.Azure.Devices.Applications.RemoteMonitoring.DeviceAdmin.Web.DataTables;
using Microsoft.Azure.Devices.Applications.RemoteMonitoring.DeviceAdmin.Web.Helpers;
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

                var result = jobResponses.Select(r => new DeviceJobModel(r)).OrderByDescending(j => j.StartTime).ToList();
                foreach(var job in result)
                {
                    var tuple = await GetJobDetailsAsync(job);
                    job.JobName = tuple.Item1;
                    job.FilterId = tuple.Item2;
                    job.FilterName = tuple.Item3;
                    job.OperationType = tuple.Item4;
                };

                var dataTablesResponse = new DataTablesResponse<DeviceJobModel>()
                {
                    RecordsTotal = result.Count,
                    Data = result.ToArray()
                };

                return await Task.FromResult(dataTablesResponse);

            }, false);
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
        [Route("{id}/results")]
        [WebApiRequirePermission(Permission.ViewJobs)]
        // GET: api/v1/jobs/{id}/results
        public async Task<HttpResponseMessage> GetJobResults(string id)
        {
            return await GetServiceResponseAsync<IEnumerable<DeviceJob>>(async () =>
            {
                var jobResponses = await _iotHubDeviceManager.GetDeviceJobsByJobIdAsync(id);
                return jobResponses.OrderByDescending(j => j.LastUpdatedDateTimeUtc).ToList();
            });
        }

        private async Task<Tuple<string, string, string, string>> GetJobDetailsAsync(DeviceJobModel job)
        {
            Task<JobRepositoryModel> queryJobTask = _jobRepository.QueryByJobIDAsync(job.JobId);
            return await DeviceJobHelper.GetJobDetailsAsync(job, queryJobTask);
        }
    }
}
