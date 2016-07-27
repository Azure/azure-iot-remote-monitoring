using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Azure.Devices.Applications.RemoteMonitoring.Common.Models;
using Microsoft.Azure.Devices.Applications.RemoteMonitoring.Common.Models.Commands;
using Microsoft.Azure.Devices.Applications.RemoteMonitoring.DeviceAdmin.Infrastructure.Models;

namespace Microsoft.Azure.Devices.Applications.RemoteMonitoring.DeviceAdmin.Infrastructure.BusinessLogic
{
    public interface IDeviceLogic
    {
        void ApplyDevicePropertyValueModels(DeviceND device, IEnumerable<DevicePropertyValueModel> devicePropertyValueModels);
        Task<DeviceListQueryResult> GetDevices(DeviceListQuery q);
        Task<DeviceND> GetDeviceAsync(string deviceId);
        Task<DeviceWithKeys> AddDeviceAsync(DeviceND device);
        IEnumerable<DevicePropertyValueModel> ExtractDevicePropertyValuesModels(DeviceND device);
        Task RemoveDeviceAsync(string deviceId);
        Task<DeviceND> UpdateDeviceAsync(DeviceND device);
        Task<DeviceND> UpdateDeviceFromDeviceInfoPacketAsync(DeviceND device);
        Task<DeviceND> UpdateDeviceEnabledStatusAsync(string deviceId, bool isEnabled);
        Task<SecurityKeys> GetIoTHubKeysAsync(string id);
        Task GenerateNDevices(int deviceCount);
        Task SendCommandAsync(string deviceId, string commandName, dynamic parameters);
        Task<List<string>> BootstrapDefaultDevices();
        DeviceListLocationsModel ExtractLocationsData(List<DeviceND> devices);
        IList<DeviceTelemetryFieldModel> ExtractTelemetry(DeviceND device);
    }
}