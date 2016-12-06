using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Azure.Devices.Applications.RemoteMonitoring.Common.Exceptions;
using Microsoft.Azure.Devices.Applications.RemoteMonitoring.Common.Helpers;
using Microsoft.Azure.Devices.Applications.RemoteMonitoring.Common.Models;
using Microsoft.Azure.Devices.Applications.RemoteMonitoring.DeviceAdmin.Infrastructure.Exceptions;
using Microsoft.Azure.Devices.Applications.RemoteMonitoring.DeviceAdmin.Infrastructure.Models;

namespace Microsoft.Azure.Devices.Applications.RemoteMonitoring.DeviceAdmin.Infrastructure.Repository
{
    public class DeviceRegistryRepository : IDeviceRegistryCrudRepository, IDeviceRegistryListRepository
    {
        protected readonly IDocumentDBClient<DeviceModel> _documentClient;

        public DeviceRegistryRepository(IDocumentDBClient<DeviceModel> documentClient)
        {
            _documentClient = documentClient;
        }

       
        /// <summary>
        /// Queries the DocumentDB and retrieves the device based on its deviceId
        /// </summary>
        /// <param name="deviceId">DeviceID of the device to retrieve</param>
        /// <returns>Device instance if present, null if a device was not found with the provided deviceId</returns>
        public virtual async Task<DeviceModel> GetDeviceAsync(string deviceId)
        {
            if (string.IsNullOrEmpty(deviceId))
            {
                throw new ArgumentException(deviceId);
            }

            var query = await _documentClient.QueryAsync();
            var devices = query.Where(x => x.DeviceProperties.DeviceID == deviceId).ToList();
            return devices.FirstOrDefault();
        }

        /// <summary>
        /// Adds a device to the DocumentDB.
        /// Throws a DeviceAlreadyRegisteredException if a device already exists in the database with the provided deviceId
        /// </summary>
        /// <param name="device"></param>
        /// <returns></returns>
        public virtual async Task<DeviceModel> AddDeviceAsync(DeviceModel device)
        {
            if (device == null)
            {
                throw new ArgumentNullException("device");
            }

            if (string.IsNullOrEmpty(device.id))
            {
                device.id = Guid.NewGuid().ToString();
            }

            DeviceModel existingDevice = await GetDeviceAsync(device.DeviceProperties.DeviceID);
            if (existingDevice != null)
            {
                throw new DeviceAlreadyRegisteredException(device.DeviceProperties.DeviceID);
            }

            var savedDevice = await _documentClient.SaveAsync(device);
            return savedDevice;
        }

        public async Task RemoveDeviceAsync(string deviceId)
        {
            if (string.IsNullOrEmpty(deviceId))
            {
                throw new ArgumentNullException("deviceId");
            }

            DeviceModel existingDevice = await GetDeviceAsync(deviceId);
            if (existingDevice == null)
            {
                throw new DeviceNotRegisteredException(deviceId);
            }

            await _documentClient.DeleteAsync(existingDevice.id);
        }

        /// <summary>
        /// Updates an existing device in the DocumentDB
        /// Throws a DeviceNotRegisteredException is the device does not already exist in the DocumentDB
        /// </summary>
        /// <param name="device"></param>
        /// <returns></returns>
        public virtual async Task<DeviceModel> UpdateDeviceAsync(DeviceModel device)
        {
            if (device == null)
            {
                throw new ArgumentNullException("device");
            }

            if (device.DeviceProperties == null)
            {
                throw new DeviceRequiredPropertyNotFoundException("'DeviceProperties' property is missing");
            }

            if (string.IsNullOrEmpty(device.DeviceProperties.DeviceID))
            {
                throw new DeviceRequiredPropertyNotFoundException("'DeviceID' property is missing");
            }

            DeviceModel existingDevice = await GetDeviceAsync(device.DeviceProperties.DeviceID);
            if (existingDevice == null)
            {
                throw new DeviceNotRegisteredException(device.DeviceProperties.DeviceID);
            }

            string incomingRid = device._rid ?? "";

            if (string.IsNullOrWhiteSpace(incomingRid))
            {
                // copy the existing _rid onto the incoming data if needed
                var existingRid = existingDevice._rid ?? "";
                if (string.IsNullOrWhiteSpace(existingRid))
                {
                    throw new InvalidOperationException("Could not find _rid property on existing device");
                }
                device._rid = existingRid;
            }

            string incomingId = device.id ?? "";

            if (string.IsNullOrWhiteSpace(incomingId))
            {
                // copy the existing id onto the incoming data if needed
                if (existingDevice.DeviceProperties == null)
                {
                    throw new DeviceRequiredPropertyNotFoundException("'DeviceProperties' property is missing");
                }

                var existingId = existingDevice.id ?? "";
                if (string.IsNullOrWhiteSpace(existingId))
                {
                    throw new InvalidOperationException("Could not find id property on existing device");
                }
                device.id = existingId;
            }

            device.DeviceProperties.UpdatedTime = DateTime.UtcNow;
            var savedDevice = await this._documentClient.SaveAsync(device);
            return savedDevice;
        }

        public virtual async Task<DeviceModel> UpdateDeviceEnabledStatusAsync(string deviceId, bool isEnabled)
        {
            if (string.IsNullOrEmpty(deviceId))
            {
                throw new ArgumentNullException("deviceId");
            }

            DeviceModel existingDevice = await this.GetDeviceAsync(deviceId);

            if (existingDevice == null)
            {
                throw new DeviceNotRegisteredException(deviceId);
            }


            if (existingDevice.DeviceProperties == null)
            {
                throw new DeviceRequiredPropertyNotFoundException("Required DeviceProperties not found");
            }

            existingDevice.DeviceProperties.HubEnabledState = isEnabled;
            existingDevice.DeviceProperties.UpdatedTime = DateTime.UtcNow;
            var savedDevice =await this._documentClient.SaveAsync(existingDevice);
            return savedDevice;
        }

        public virtual async Task<DeviceListFilterResult> GetDeviceList(DeviceListFilter filter)
        {
            List<DeviceModel> deviceList = await this.GetAllDevicesAsync();

            IQueryable<DeviceModel> filteredDevices = FilterHelper.FilterDeviceList(deviceList.AsQueryable<DeviceModel>(), filter.Clauses);

            IQueryable<DeviceModel> filteredAndSearchedDevices = this.SearchDeviceList(filteredDevices, filter.SearchQuery);

            IQueryable<DeviceModel> sortedDevices = this.SortDeviceList(filteredAndSearchedDevices, filter.SortColumn, filter.SortOrder);

            List<DeviceModel> pagedDeviceList = sortedDevices.Skip(filter.Skip).Take(filter.Take).ToList();

            int filteredCount = filteredAndSearchedDevices.Count();

            return new DeviceListFilterResult()
            {
                Results = pagedDeviceList,
                TotalDeviceCount = deviceList.Count,
                TotalFilteredCount = filteredCount
            };
        }

        /// <summary>
        /// Queries the DocumentDB and retrieves all documents in the collection
        /// </summary>
        /// <returns>All documents in the collection</returns>
        private async Task<List<DeviceModel>> GetAllDevicesAsync()
        {
            var devices = await _documentClient.QueryAsync();
            return devices.ToList();
        }

        private IQueryable<DeviceModel> SearchDeviceList(IQueryable<DeviceModel> deviceList, string search)
        {
            if (string.IsNullOrWhiteSpace(search))
            {
                return deviceList;
            }

            Func<DeviceModel, bool> filter = (d) => this.SearchTypePropertiesForValue(d, search);

            // look for all devices that contain the search value in one of the DeviceProperties Properties
            return deviceList.Where(filter).AsQueryable();
        }

        private bool SearchTypePropertiesForValue(DeviceModel device, string search)
        {
            // if the device or its system properties are null then
            // there's nothing that can be searched on
            if (device?.DeviceProperties == null)
            {
                return false;
            }

            // iterate through the DeviceProperties Properties and look for the search value
            // case insensitive search
            var upperCaseSearch = search.ToUpperInvariant();
            return device.DeviceProperties.ToKeyValuePairs().Any(t =>
                    (t.Value != null) &&
                    t.Value.ToString().ToUpperInvariant().Contains(upperCaseSearch));
        }

        private IQueryable<DeviceModel> SortDeviceList(IQueryable<DeviceModel> deviceList, string sortColumn, QuerySortOrder sortOrder)
        {
            // if a sort column was not provided then return the full device list in its original sort
            if (string.IsNullOrWhiteSpace(sortColumn))
            {
                return deviceList;
            }

            Func<DeviceProperties, dynamic> getPropVal = ReflectionHelper.ProducePropertyValueExtractor(sortColumn, false, false);
            Func<DeviceModel, dynamic> keySelector = (item) =>
            {
                if (item?.DeviceProperties == null)
                {
                    return null;
                }

                if (string.Equals("hubEnabledState", sortColumn, StringComparison.CurrentCultureIgnoreCase))
                {
                    return item.DeviceProperties.GetHubEnabledState();
                }

                return getPropVal(item.DeviceProperties);
            };

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