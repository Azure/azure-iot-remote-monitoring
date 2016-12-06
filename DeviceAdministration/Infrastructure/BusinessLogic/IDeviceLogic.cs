using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Azure.Devices.Applications.RemoteMonitoring.Common.Models;
using Microsoft.Azure.Devices.Applications.RemoteMonitoring.Common.Models.Commands;
using Microsoft.Azure.Devices.Applications.RemoteMonitoring.DeviceAdmin.Infrastructure.Models;

namespace Microsoft.Azure.Devices.Applications.RemoteMonitoring.DeviceAdmin.Infrastructure.BusinessLogic
{
    public interface IDeviceLogic
    {
        void ApplyDevicePropertyValueModels(DeviceModel device, IEnumerable<DevicePropertyValueModel> devicePropertyValueModels);
        Task<DeviceListFilterResult> GetDevices(DeviceListFilter filter);
        Task<DeviceModel> GetDeviceAsync(string deviceId);
        Task<DeviceWithKeys> AddDeviceAsync(DeviceModel device);
        IEnumerable<DevicePropertyValueModel> ExtractDevicePropertyValuesModels(DeviceModel device);
        Task RemoveDeviceAsync(string deviceId);
        Task<DeviceModel> UpdateDeviceAsync(DeviceModel device);
        Task<DeviceModel> UpdateDeviceFromDeviceInfoPacketAsync(DeviceModel device);
        Task<DeviceModel> UpdateDeviceEnabledStatusAsync(string deviceId, bool isEnabled);
        Task<SecurityKeys> GetIoTHubKeysAsync(string id);
        Task GenerateNDevices(int deviceCount);
        Task SendCommandAsync(string deviceId, string commandName, DeliveryType deliveryType, dynamic parameters);
        Task<List<string>> BootstrapDefaultDevices();
        DeviceListLocationsModel ExtractLocationsData(List<DeviceModel> devices);
        IList<DeviceTelemetryFieldModel> ExtractTelemetry(DeviceModel device);
        Task AddToNameCache(string deviceId);
    }
}