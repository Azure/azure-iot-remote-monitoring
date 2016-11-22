using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Azure.Devices.Applications.RemoteMonitoring.DeviceAdmin.Infrastructure.Models;

namespace Microsoft.Azure.Devices.Applications.RemoteMonitoring.DeviceAdmin.Infrastructure.Repository
{
    /// <summary>
    /// Interface to expose methods that can be called against the underlying identity repository
    /// </summary>
    public interface IIoTHubDeviceManager
    {
        Task<Device> AddDeviceAsync(Device device);
        Task<Device> GetDeviceAsync(string deviceId);
        Task RemoveDeviceAsync(string deviceId);
        Task<Device> UpdateDeviceAsync(Device device);
        Task SendAsync(string deviceId, Message message);
        Task<CloudToDeviceMethodResult> InvokeDeviceMethodAsync(string deviceId, CloudToDeviceMethod method);
        Task CloseAsyncService();
        Task CloseAsyncDevice();
        Task<Twin> GetTwinAsync(string deviceId);
        Task UpdateTwinAsync(string deviceId, Twin twin);
        Task<IEnumerable<Twin>> QueryDevicesAsync(DeviceListQuery query);
        Task<long> GetDeviceCountAsync();
        Task<IEnumerable<DeviceJob>> GetDeviceJobsByDeviceIdAsync(string deviceId);
        Task<IEnumerable<DeviceJob>> GetDeviceJobsByJobIdAsync(string jobId);
        Task<JobResponse> GetJobResponseByJobIdAsync(string jobId);
    }
}
