// Until now, IoT Hub is not stable for running queries (internal server error)
// We will use application side filtering as workaround. Please uncomment flag below to enable the filtering on IoT Hub side
#define QUERY_DEVICES_IOTHUB
#define QUERY_JOBS_IOTHUB

using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using Microsoft.Azure.Devices.Applications.RemoteMonitoring.Common.Configurations;
using Microsoft.Azure.Devices.Applications.RemoteMonitoring.Common.Models;
using Microsoft.Azure.Devices.Applications.RemoteMonitoring.DeviceAdmin.Infrastructure.Models;
using Newtonsoft.Json;

namespace Microsoft.Azure.Devices.Applications.RemoteMonitoring.DeviceAdmin.Infrastructure.Repository
{
    /// <summary>
    ///     Wraps calls to the IoT hub identity store.
    ///     IDisposable is implemented in order to close out the connection to the IoT Hub when this object is no longer in use
    /// </summary>
    public class IoTHubDeviceManager : IIoTHubDeviceManager, IDisposable
    {
        private readonly RegistryManager _deviceManager;
        private readonly ServiceClient serviceClient;
        private bool _disposed;

        public IoTHubDeviceManager(IConfigurationProvider configProvider)
        {
            // Temporary code to bypass https cert validation till DNS on IotHub is configured
            ServicePointManager.ServerCertificateValidationCallback = (sender, certificate, chain, errors) => true;
            var _iotHubConnectionString = configProvider.GetConfigurationSettingValue("iotHub.ConnectionString");
            this._deviceManager = RegistryManager.CreateFromConnectionString(_iotHubConnectionString);
            this.serviceClient = ServiceClient.CreateFromConnectionString(_iotHubConnectionString);

#if !QUERY_JOBS_IOTHUB
            this._mockJobs = new Lazy<List<JobQueryResult>>(() => BuildMockJobs().Result);
#endif
        }

        public async Task<Device> AddDeviceAsync(Device device)
        {
            return await this._deviceManager.AddDeviceAsync(device);
        }

        public async Task<Device> GetDeviceAsync(string deviceId)
        {
            return await this._deviceManager.GetDeviceAsync(deviceId);
        }

        public async Task RemoveDeviceAsync(string deviceId)
        {
            await this._deviceManager.RemoveDeviceAsync(deviceId);
        }

        public async Task<Device> UpdateDeviceAsync(Device device)
        {
            return await this._deviceManager.UpdateDeviceAsync(device);
        }

        public async Task SendAsync(string deviceId, Message message)
        {
            await this.serviceClient.SendAsync(deviceId, message);
        }

        public async Task<CloudToDeviceMethodResult> InvokeDeviceMethodAsync(string deviceId, CloudToDeviceMethod method)
        {
            return await this.serviceClient.InvokeDeviceMethodAsync(deviceId, method);
        }

        public async Task CloseAsyncDevice()
        {
            await this.serviceClient.CloseAsync();
        }

        public async Task CloseAsyncService()
        {
            await this._deviceManager.CloseAsync();
        }

        public async Task<Twin> GetTwinAsync(string deviceId)
        {
            return await _deviceManager.GetTwinAsync(deviceId);
        }

        public async Task UpdateTwinAsync(string deviceId, Twin twin)
        {
            await this._deviceManager.UpdateTwinAsync(deviceId, twin, twin.ETag);
        }

        public async Task<IEnumerable<Twin>> QueryDevicesAsync(DeviceListQuery query)
        {
#if QUERY_DEVICES_IOTHUB
            var sqlQuery = query.GetSQLQuery();
            var deviceQuery = this._deviceManager.CreateQuery(sqlQuery);

            var twins = new List<Twin>();
            while (deviceQuery.HasMoreResults)
            {
                twins.AddRange(await deviceQuery.GetNextAsTwinAsync());
            }

            return twins;
#else
            // [WORKAROUND] Filtering devices at application side rather than IoT Hub side
            var devices = await this._deviceManager.GetDevicesAsync(1000);
            var tasks = devices.Select(device => this._deviceManager.GetTwinAsync(device.Id));
            var twins = await Task.WhenAll(tasks);

            return twins.Where(twin => query.Filters == null || query.Filters.All(filter =>
            {
                if (string.IsNullOrWhiteSpace(filter.ColumnName))
                {
                    return true;
                }

                string value = twin.Get(filter.ColumnName)?.ToString();
                int compare = string.Compare(value, filter.FilterValue);

                switch (filter.FilterType)
                {
                    case FilterType.EQ: return compare == 0;
                    case FilterType.NE: return compare != 0;
                    case FilterType.LT: return compare < 0;
                    case FilterType.GT: return compare > 0;
                    case FilterType.LE: return compare <= 0;
                    case FilterType.GE: return compare >= 0;
                    case FilterType.IN: throw new NotImplementedException();
                    default: throw new NotSupportedException();
                }
            }));
#endif
        }

        public async Task<long> GetDeviceCountAsync()
        {
            return (await this._deviceManager.GetRegistryStatisticsAsync()).TotalDeviceCount;
        }

        public async Task<IEnumerable<JobQueryResult>> GetJobsByDeviceIDAsync(string deviceId)
        {
#if QUERY_JOBS_IOTHUB
            return await QueryJobs($"SELECT * FROM devices.jobs WHERE devices.jobs.deviceId='{deviceId}'");
#else
            return _mockJobs.Value.Where(j => j.DeviceID == deviceId);
#endif

        }

        public async Task<IEnumerable<JobQueryResult>> GetJobsByJobIDAsync(string jobId)
        {
#if QUERY_JOBS_IOTHUB
            return await QueryJobs($"SELECT * FROM devices.jobs WHERE devices.jobs.jobId='{jobId}'");
#else
            return _mockJobs.Value.Where(j => j.JobID == jobId);
#endif
        }

        private async Task<IEnumerable<JobQueryResult>> QueryJobs(string sqlQueryString)
        {
            var jobQuery = this._deviceManager.CreateQuery(sqlQueryString);

            var results = new List<JobQueryResult>();
            while (jobQuery.HasMoreResults)
            {
                var jobsInJSON = await jobQuery.GetNextAsJsonAsync();
                results.AddRange(jobsInJSON.Select(j => JsonConvert.DeserializeObject<JobQueryResult>(j)));
            }

            return results;
        }

        #region IDispose
        /// <summary>
        ///     Implement the IDisposable interface in order to close the device manager
        /// </summary>
        public void Dispose()
        {
            this.Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (this._disposed)
            {
                return;
            }

            if (disposing)
            {
                if (this._deviceManager != null)
                {
                    this._deviceManager.CloseAsync().Wait();
                }
            }

            this._disposed = true;
        }

        ~IoTHubDeviceManager()
        {
            this.Dispose(false);
        }
        #endregion

#if !QUERY_JOBS_IOTHUB
        #region JobQueryMock
        private Lazy<List<JobQueryResult>> _mockJobs;

        private async Task<List<JobQueryResult>> BuildMockJobs()
        {
            var rand = new Random();
            var devices = (await _deviceManager.GetDevicesAsync(1000)).Select(d => d.Id).ToArray();

            var jobIDs = Enumerable.Range(1, 10).Select(i => Guid.NewGuid());

            var jobs = new List<JobQueryResult>();
            foreach (var jobID in jobIDs)
            {
                string jobType = rand.Next() % 2 == 0 ? "scheduleUpdateTwin" : "scheduleDeviceMethod";

                int deviceCount = rand.Next(1, devices.Count());
                var selectedDevices = new List<string>();
                var availableDevices = devices.ToList();

                while (selectedDevices.Count < deviceCount)
                {
                    int index = rand.Next(deviceCount);
                    selectedDevices.Add(availableDevices.ElementAt(index));
                    availableDevices.RemoveAt(index);
                }

                jobs.AddRange(selectedDevices.Select(id =>
                {
                    string status = rand.Next() % 5 > 0 ? "completed" : "failed";
                    DateTime endTime = DateTime.UtcNow - TimeSpan.FromMinutes(rand.Next(60));
                    DateTime startTime = endTime - TimeSpan.FromMinutes(10);

                    return new JobQueryResult
                    {
                        DeviceID = id,
                        JobID = jobID.ToString(),
                        JobType = jobType,
                        Status = status,
                        StartTimeUtc = startTime,
                        EndTimeUtc = endTime,
                        CreatedTimeUtc = startTime - TimeSpan.FromMinutes(rand.Next(5)),
                        LastUpdatedTimeUtc = DateTime.UtcNow - TimeSpan.FromMinutes(rand.Next(10)),
                        Outcome = null
                    };
                }));
            }

            return jobs;
        }
        #endregion
#endif
    }
}
