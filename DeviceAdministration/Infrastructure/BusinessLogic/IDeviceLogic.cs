using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Azure.Devices.Applications.RemoteMonitoring.Common.Models;
using Microsoft.Azure.Devices.Applications.RemoteMonitoring.Common.Models.Commands;
using Microsoft.Azure.Devices.Applications.RemoteMonitoring.DeviceAdmin.Infrastructure.Models;

namespace Microsoft.Azure.Devices.Applications.RemoteMonitoring.DeviceAdmin.Infrastructure.BusinessLogic
{
    public interface IDeviceLogic
    {
        void ApplyDevicePropertyValueModels(
            dynamic device,
            IEnumerable<DevicePropertyValueModel> devicePropertyValueModels);
        Task<DeviceListQueryResult> GetDevices(DeviceListQuery q);
        Task<dynamic> GetDeviceAsync(string deviceId);
        Task<DeviceWithKeys> AddDeviceAsync(dynamic device);
        IEnumerable<DevicePropertyValueModel> ExtractDevicePropertyValuesModels(dynamic device);
        Task RemoveDeviceAsync(string deviceId);
        Task<dynamic> UpdateDeviceAsync(dynamic device);
        Task<dynamic> UpdateDeviceFromDeviceInfoPacketAsync(dynamic device);
        Task<dynamic> UpdateDeviceEnabledStatusAsync(string deviceId, bool isEnabled);
        Task<SecurityKeys> GetIoTHubKeysAsync(string id);
        Task GenerateNDevices(int deviceCount);
        Task SendCommandAsync(string deviceId, string commandName, dynamic parameters);
        Task<List<string>> BootstrapDefaultDevices();
        DeviceListLocationsModel ExtractLocationsData(List<dynamic> devices);
        IList<DeviceTelemetryFieldModel> ExtractTelemetry(dynamic device);
    }
}