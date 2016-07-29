using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Azure.Devices.Applications.RemoteMonitoring.Common.Configurations;
using Microsoft.Azure.Devices.Applications.RemoteMonitoring.Common.DeviceSchema;
using Microsoft.Azure.Devices.Applications.RemoteMonitoring.Common.Exceptions;
using Microsoft.Azure.Devices.Applications.RemoteMonitoring.Common.Helpers;
using Microsoft.Azure.Devices.Applications.RemoteMonitoring.Common.Mapper;
using Microsoft.Azure.Devices.Applications.RemoteMonitoring.Common.Models;
using Microsoft.Azure.Devices.Applications.RemoteMonitoring.Common.Utility;
using Microsoft.Azure.Devices.Applications.RemoteMonitoring.DeviceAdmin.Infrastructure.Exceptions;
using Microsoft.Azure.Devices.Applications.RemoteMonitoring.DeviceAdmin.Infrastructure.Models;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Microsoft.Azure.Devices.Applications.RemoteMonitoring.DeviceAdmin.Infrastructure.Repository
{
    public class DeviceRegistryRepository : IDeviceRegistryCrudRepository, IDeviceRegistryListRepository
    {
        // Configuration strings for use in accessing the DocumentDB, Database and DocumentCollection
        readonly string _endpointUri;
        readonly string _authorizationKey;
        readonly string _databaseId;
        readonly string _documentCollectionName;

        IDocDbRestUtility _docDbRestUtil;

        public DeviceRegistryRepository(IConfigurationProvider configProvider, IDocDbRestUtility docDbRestUtil)
        {
            if (configProvider == null)
            {
                throw new ArgumentNullException("configProvider");
            }

            _endpointUri = configProvider.GetConfigurationSettingValue("docdb.EndpointUrl");
            _authorizationKey = configProvider.GetConfigurationSettingValue("docdb.PrimaryAuthorizationKey");
            _databaseId = configProvider.GetConfigurationSettingValue("docdb.DatabaseId");
            _documentCollectionName = configProvider.GetConfigurationSettingValue("docdb.DocumentCollectionId");


            _docDbRestUtil = docDbRestUtil;
            Task.Run(() => _docDbRestUtil.InitializeDatabase()).Wait();
            Task.Run(() => _docDbRestUtil.InitializeCollection()).Wait();
        }

        /// <summary>
        /// Queries the DocumentDB and retrieves all documents in the collection
        /// </summary>
        /// <returns>All documents in the collection</returns>
        private async Task<List<DeviceModel>> GetAllDevicesAsync()
        {
            IEnumerable docs;
            List<DeviceModel> deviceList = new List<DeviceModel>();
            List<DeviceModel> tmpDeviceList = new List<DeviceModel>();

            string query = "SELECT VALUE root FROM root";
            string continuationToken = null;
            int pageSize = 500;
            do
            {
                DocDbRestQueryResult result = await _docDbRestUtil.QueryCollectionAsync(query, null, pageSize, continuationToken);

                docs =
                    ReflectionHelper.GetNamedPropertyValue(
                        result,
                        "ResultSet",
                        true,
                        false) as IEnumerable;

                tmpDeviceList = JsonConvert.DeserializeObject<List<DeviceModel>>(docs.ToString());
                deviceList.AddRange(tmpDeviceList);

                continuationToken = result.ContinuationToken;

            } while (!String.IsNullOrEmpty(continuationToken));

            return (deviceList.Count != 0 ? deviceList : null);
        }

        /// <summary>
        /// Queries the DocumentDB and retrieves the device based on its deviceId
        /// </summary>
        /// <param name="deviceId">DeviceID of the device to retrieve</param>
        /// <returns>Device instance if present, null if a device was not found with the provided deviceId</returns>
        public async Task<DeviceModel> GetDeviceAsync(string deviceId)
        {
            JToken result = null;

            Dictionary<string, Object> queryParams = new Dictionary<string, Object>();
            queryParams.Add("@id", deviceId);
            DocDbRestQueryResult response = await _docDbRestUtil.QueryCollectionAsync("SELECT VALUE root FROM root WHERE (root.DeviceProperties.DeviceID = @id)", queryParams);
            JArray foundDevices = response.ResultSet;

            if (foundDevices != null && foundDevices.Count > 0)
            {
                result = foundDevices.Children().ElementAt(0);
                return result.ToObject<DeviceModel>();
            }
            return null;
        }

        /// <summary>
        /// Adds a device to the DocumentDB.
        /// Throws a DeviceAlreadyRegisteredException if a device already exists in the database with the provided deviceId
        /// </summary>
        /// <param name="device"></param>
        /// <returns></returns>
        public async Task<DeviceModel> AddDeviceAsync(DeviceModel device)
        {
            string deviceId = device.DeviceProperties.DeviceID;
            DeviceModel existingDevice = await this.GetDeviceAsync(deviceId);

            if (existingDevice != null)
            {
                throw new DeviceAlreadyRegisteredException(deviceId);
            }

            device = (await _docDbRestUtil.SaveNewDocumentAsync<DeviceModel>(device)).ToObject<DeviceModel>();

            return device;
        }

        public async Task RemoveDeviceAsync(string deviceId)
        {
            DeviceModel existingDevice = await this.GetDeviceAsync(deviceId);

            if (existingDevice == null)
            {
                throw new DeviceNotRegisteredException(deviceId);
            }

            await _docDbRestUtil.DeleteDocumentAsync<DeviceModel>(existingDevice);
        }

        /// <summary>
        /// Updates an existing device in the DocumentDB
        /// Throws a DeviceNotRegisteredException is the device does not already exist in the DocumentDB
        /// </summary>
        /// <param name="device"></param>
        /// <returns></returns>
        public async Task<DeviceModel> UpdateDeviceAsync(DeviceModel device)
        {
            if (device.DeviceProperties == null)
            {
                throw new DeviceRequiredPropertyNotFoundException("'DeviceProperties' property is missing");
            }

            if (device.DeviceProperties.DeviceID == null)
            {
                throw new DeviceRequiredPropertyNotFoundException("'DeviceID' property is missing");
            }

            string deviceId = device.DeviceProperties.DeviceID;

            DeviceModel existingDevice = await this.GetDeviceAsync(deviceId);

            if (existingDevice == null)
            {
                throw new DeviceNotRegisteredException(deviceId);
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

            return (await _docDbRestUtil.UpdateDocumentAsync<DeviceModel>(device)).ToObject<DeviceModel>();
        }

        public async Task<DeviceModel> UpdateDeviceEnabledStatusAsync(string deviceId, bool isEnabled)
        {
            DeviceModel existingDevice = await this.GetDeviceAsync(deviceId);

            if (existingDevice == null)
            {
                throw new DeviceNotRegisteredException(deviceId);
            }

            DeviceProperties deviceProps = DeviceSchemaHelper.GetDeviceProperties(existingDevice);
            deviceProps.HubEnabledState = isEnabled;
            DeviceSchemaHelper.UpdateUpdatedTime(existingDevice);

            return (await _docDbRestUtil.UpdateDocumentAsync<DeviceModel>(existingDevice)).ToObject<DeviceModel>();
        }

        public async Task<DeviceListQueryResult> GetDeviceList(DeviceListQuery query)
        {
            List<DeviceModel> deviceList = await this.GetAllDevicesAsync();

            IQueryable<DeviceModel> filteredDevices = FilterHelper.FilterDeviceList(deviceList.AsQueryable<DeviceModel>(), query.Filters);

            IQueryable<DeviceModel> filteredAndSearchedDevices = this.SearchDeviceList(filteredDevices, query.SearchQuery);

            IQueryable<DeviceModel> sortedDevices = this.SortDeviceList(filteredAndSearchedDevices, query.SortColumn, query.SortOrder);

            List<DeviceModel> pagedDeviceList = sortedDevices.Skip(query.Skip).Take(query.Take).ToList();

            int filteredCount = filteredAndSearchedDevices.Count();

            return new DeviceListQueryResult()
            {
                Results = pagedDeviceList,
                TotalDeviceCount = deviceList.Count,
                TotalFilteredCount = filteredCount
            };
        }

        private IQueryable<DeviceModel> SearchDeviceList(IQueryable<DeviceModel> deviceList, string search)
        {
            if (string.IsNullOrWhiteSpace(search))
            {
                return deviceList;
            }

            Func<DeviceModel, bool> filter =
                (d) => this.SearchTypePropertiesForValue(d, search);

            // look for all devices that contain the search value in one of the DeviceProperties Properties
            return deviceList.Where(filter).AsQueryable();
        }
       
        private bool SearchTypePropertiesForValue(DeviceModel device, string search)
        {
            DeviceProperties devProps = null;

            // if the device or its system properties are null then
            // there's nothing that can be searched on
            if ((device == null) ||
                ((devProps = DeviceSchemaHelper.GetDeviceProperties(device)) == null))
            {
                return false;
            }

            try
            {
                devProps = DeviceSchemaHelper.GetDeviceProperties(device);
            }
            catch (DeviceRequiredPropertyNotFoundException)
            {
                devProps = null;
            }

            if (devProps == null)
            {
                return false;
            }

            // iterate through the DeviceProperties Properties and look for the search value
            // case insensitive search
            var upperCaseSearch = search.ToUpperInvariant();
            return devProps.ToKeyValuePairs().Any(
                t =>
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

            Func<DeviceProperties, dynamic> getPropVal =
                ReflectionHelper.ProducePropertyValueExtractor(
                    sortColumn,
                    false,
                    false);

            Func<DeviceModel, dynamic> keySelector =
                (item) =>
                {
                    DeviceProperties deviceProperties;

                    if (item == null)
                    {
                        return null;
                    }

                    if (string.Equals(
                        "hubEnabledState",
                        sortColumn,
                        StringComparison.CurrentCultureIgnoreCase))
                    {
                        try
                        {
                            return DeviceSchemaHelper.GetHubEnabledState(item);
                        }
                        catch (DeviceRequiredPropertyNotFoundException)
                        {
                            return null;
                        }
                    }

                    try
                    {
                        deviceProperties =
                            DeviceSchemaHelper.GetDeviceProperties(item);
                    }
                    catch (DeviceRequiredPropertyNotFoundException)
                    {
                        return null;
                    }

                    return getPropVal(deviceProperties);
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