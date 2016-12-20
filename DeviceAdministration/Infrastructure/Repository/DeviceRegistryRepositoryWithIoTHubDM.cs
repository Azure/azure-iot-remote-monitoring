using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Azure.Devices.Applications.RemoteMonitoring.Common.Extensions;
using Microsoft.Azure.Devices.Applications.RemoteMonitoring.Common.Helpers;
using Microsoft.Azure.Devices.Applications.RemoteMonitoring.Common.Models;
using Microsoft.Azure.Devices.Applications.RemoteMonitoring.DeviceAdmin.Infrastructure.Models;
using Microsoft.Azure.Devices.Shared;

namespace Microsoft.Azure.Devices.Applications.RemoteMonitoring.DeviceAdmin.Infrastructure.Repository
{
    public class DeviceRegistryRepositoryWithIoTHubDM : DeviceRegistryRepository
    {
        private readonly IIoTHubDeviceManager _deviceManager;

        public DeviceRegistryRepositoryWithIoTHubDM(IDocumentDBClient<DeviceModel> documentClient, IIoTHubDeviceManager deviceManager) :
            base(documentClient)
        {
            _deviceManager = deviceManager;
        }

        public async override Task<DeviceModel> GetDeviceAsync(string deviceId)
        {
            var baseTask = base.GetDeviceAsync(deviceId);
            var selfTask = this._deviceManager.GetTwinAsync(deviceId);
            await Task.WhenAll(baseTask, selfTask);

            var device = baseTask.Result;

            // Add the twin from IoT Hub to the model
            if (device != null)
            {
                device.Twin = selfTask.Result;
            }

            return device;
        }

        public override async Task<DeviceModel> AddDeviceAsync(DeviceModel device)
        {
            var result = await base.AddDeviceAsync(device);

            // Update the twin: set status as running
            await SetHubEnabledStateTag(device.DeviceProperties.DeviceID, true);

            return result;
        }

        public override async Task<DeviceModel> UpdateDeviceAsync(DeviceModel device)
        {
            var result = await base.UpdateDeviceAsync(device);

            // Update the twin if it was changed comparing to the one just retrieved from IoT Hub
            if (device.Twin != null)
            {
                var existing = await this._deviceManager.GetTwinAsync(device.Twin.DeviceId);
                if (device.Twin.UpdateRequired(existing))
                {
                    await this._deviceManager.UpdateTwinAsync(device.Twin.DeviceId, device.Twin);
                }
            }

            return result;
        }

        public override async Task<DeviceModel> UpdateDeviceEnabledStatusAsync(string deviceId, bool isEnabled)
        {
            var result = await base.UpdateDeviceEnabledStatusAsync(deviceId, isEnabled);

            // Update the twin: set status
            await SetHubEnabledStateTag(deviceId, isEnabled);

            return result;
        }

        public override async Task<DeviceListFilterResult> GetDeviceList(DeviceListFilter filter)
        {
            // Kick-off DocDB query initializing
            var queryTask = this._documentClient.QueryAsync();

            // Considering all the device properties was copied to IoT Hub twin as tag or
            // reported property, we will only query on the IoT Hub twins. The DocumentDB
            // will not be touched.
            var filteredDevices = await this._deviceManager.QueryDevicesAsync(filter);

            var sortedDevices = this.SortDeviceList(filteredDevices.AsQueryable(), filter.SortColumn, filter.SortOrder);

            var pagedDeviceList = sortedDevices.Skip(filter.Skip).Take(filter.Take).ToList();

            // Query on DocDB for traditional device properties, commands and so on
            var deviceIds = pagedDeviceList.Select(twin => twin.DeviceId);
            var devicesFromDocDB = (await queryTask).Where(x => deviceIds.Contains(x.DeviceProperties.DeviceID))
                .ToDictionary(d => d.DeviceProperties.DeviceID);

            return new DeviceListFilterResult
            {
                Results = pagedDeviceList.Select(twin =>
                {
                    DeviceModel deviceModel;
                    if (devicesFromDocDB.TryGetValue(twin.DeviceId, out deviceModel))
                    {
                        deviceModel.Twin = twin;
                        return deviceModel;
                    }
                    else
                    {
                        return null;
                    }
                }).Where(model => model != null).ToList(),
                TotalDeviceCount = (int)await this._deviceManager.GetDeviceCountAsync(),
                TotalFilteredCount = filteredDevices.Count()
            };
        }

        // The status was implemented as a both service and device side writable variable
        // in DocumentDB in the pre-DM version. Now it was implemented as a tag, which could
        // be changd by service side only
        private async Task SetHubEnabledStateTag(string deviceId, bool isEnabled)
        {
            var twin = new Twin(deviceId) { ETag = "*" };
            twin.Tags["HubEnabledState"] = isEnabled ? "Running" : "Disabled";
            await this._deviceManager.UpdateTwinAsync(deviceId, twin);
        }

        private IQueryable<Twin> SortDeviceList(IQueryable<Twin> deviceList, string sortColumn, QuerySortOrder sortOrder)
        {
            // if a sort column was not provided then return the full device list in its original sort
            if (string.IsNullOrWhiteSpace(sortColumn))
            {
                return deviceList;
            }

            Func<Twin, dynamic> keySelector = twin => twin.Get(sortColumn);

            if (sortOrder == QuerySortOrder.Ascending)
            {
                return deviceList.OrderBy(keySelector).AsQueryable();
            }
            else
            {
                return deviceList.OrderByDescending(keySelector).AsQueryable();
            }
        }
    }
}
