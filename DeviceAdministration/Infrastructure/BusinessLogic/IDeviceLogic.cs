using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Azure.Devices.Applications.RemoteMonitoring.Common.Models;
using Microsoft.Azure.Devices.Applications.RemoteMonitoring.Common.Models.Commands;
using Microsoft.Azure.Devices.Applications.RemoteMonitoring.DeviceAdmin.Infrastructure.Models;

namespace Microsoft.Azure.Devices.Applications.RemoteMonitoring.DeviceAdmin.Infrastructure.BusinessLogic
{
    public interface IDeviceLogic
    {
        void ApplyDevicePropertyValueModels(Common.Models.Device device, IEnumerable<DevicePropertyValueModel> devicePropertyValueModels);
        Task<DeviceListQueryResult> GetDevices(DeviceListQuery q);
        Task<Common.Models.Device> GetDeviceAsync(string deviceId);
        Task<DeviceWithKeys> AddDeviceAsync(Common.Models.Device device);
        IEnumerable<DevicePropertyValueModel> ExtractDevicePropertyValuesModels(Common.Models.Device device);
        Task RemoveDeviceAsync(string deviceId);
        Task<Common.Models.Device> UpdateDeviceAsync(Common.Models.Device device);
        Task<Common.Models.Device> UpdateDeviceFromDeviceInfoPacketAsync(Common.Models.Device device);
        Task<Common.Models.Device> UpdateDeviceEnabledStatusAsync(string deviceId, bool isEnabled);
        Task<SecurityKeys> GetIoTHubKeysAsync(string id);
        Task GenerateNDevices(int deviceCount);
        Task SendCommandAsync(string deviceId, string commandName, dynamic parameters);
        Task<List<string>> BootstrapDefaultDevices();
        DeviceListLocationsModel ExtractLocationsData(List<Common.Models.Device> devices);
        IList<DeviceTelemetryFieldModel> ExtractTelemetry(Common.Models.Device device);
    }
}