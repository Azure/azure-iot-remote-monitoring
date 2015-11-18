using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Azure.Devices.Applications.RemoteMonitoring.Common.Configurations;
using Microsoft.Azure.Devices.Applications.RemoteMonitoring.Common.DeviceSchema;
using Microsoft.Azure.Devices.Applications.RemoteMonitoring.Common.Exceptions;
using Microsoft.Azure.Devices.Applications.RemoteMonitoring.Common.Helpers;
using Microsoft.Azure.Devices.Applications.RemoteMonitoring.Common.Utility;
using Microsoft.Azure.Devices.Applications.RemoteMonitoring.DeviceAdmin.Infrastructure.Exceptions;
using Microsoft.Azure.Devices.Applications.RemoteMonitoring.DeviceAdmin.Infrastructure.Models;
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
        private async Task<List<dynamic>> GetAllDevicesAsync()
        {
            IEnumerable docs;
            List<dynamic> deviceList = new List<dynamic>();

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

                if (docs != null)
                {
                    foreach (object doc in docs)
                    {
                        if (doc != null)
                        {
                            deviceList.Add(doc);
                        }
                    }
                }

                continuationToken = result.ContinuationToken;

            } while (!String.IsNullOrEmpty(continuationToken));

            return deviceList;
        }

        /// <summary>
        /// Queries the DocumentDB and retrieves the device based on its deviceId
        /// </summary>
        /// <param name="deviceId">DeviceID of the device to retrieve</param>
        /// <returns>Device instance if present, null if a device was not found with the provided deviceId</returns>
        public async Task<dynamic> GetDeviceAsync(string deviceId)
        {
            dynamic result = null;

            Dictionary<string, Object> queryParams = new Dictionary<string, Object>();
            queryParams.Add("@id", deviceId);
            DocDbRestQueryResult response = await _docDbRestUtil.QueryCollectionAsync("SELECT VALUE root FROM root WHERE (root.DeviceProperties.DeviceID = @id)", queryParams);
            JArray foundDevices = response.ResultSet;

            if (foundDevices != null && foundDevices.Count > 0)
            {
                result = foundDevices.Children().ElementAt(0);
            }

            return result;
        }

        /// <summary>
        /// Adds a device to the DocumentDB.
        /// Throws a DeviceAlreadyRegisteredException if a device already exists in the database with the provided deviceId
        /// </summary>
        /// <param name="device"></param>
        /// <returns></returns>
        public async Task<dynamic> AddDeviceAsync(dynamic device)
        {
            string deviceId = DeviceSchemaHelper.GetDeviceID(device);
            dynamic existingDevice = await GetDeviceAsync(deviceId);

            if (existingDevice != null)
            {
                throw new DeviceAlreadyRegisteredException(deviceId);
            }

            device = await _docDbRestUtil.SaveNewDocumentAsync(device);

            return device;
        }

        public async Task RemoveDeviceAsync(string deviceId)
        {
            dynamic existingDevice = await GetDeviceAsync(deviceId);

            if (existingDevice == null)
            {
                throw new DeviceNotRegisteredException(deviceId);
            }

            await _docDbRestUtil.DeleteDocumentAsync(existingDevice);
        }

        /// <summary>
        /// Updates an existing device in the DocumentDB
        /// Throws a DeviceNotRegisteredException is the device does not already exist in the DocumentDB
        /// </summary>
        /// <param name="device"></param>
        /// <returns></returns>
        public async Task<dynamic> UpdateDeviceAsync(dynamic device)
        {
            string deviceId = DeviceSchemaHelper.GetDeviceID(device);

            dynamic existingDevice = await GetDeviceAsync(deviceId);

            if (existingDevice == null)
            {
                throw new DeviceNotRegisteredException(deviceId);
            }

            string incomingRid = DeviceSchemaHelper.GetDocDbRid(device);

            if (string.IsNullOrWhiteSpace(incomingRid))
            {
                // copy the existing _rid onto the incoming data if needed
                var existingRid = DeviceSchemaHelper.GetDocDbRid(existingDevice);
                if (string.IsNullOrWhiteSpace(existingRid))
                {
                    throw new InvalidOperationException("Could not find _rid property on existing device");
                }
                device._rid = existingRid;
            }

            string incomingId = DeviceSchemaHelper.GetDocDbId(device);

            if (string.IsNullOrWhiteSpace(incomingId))
            {
                // copy the existing id onto the incoming data if needed
                var existingId = DeviceSchemaHelper.GetDocDbId(existingDevice);
                if (string.IsNullOrWhiteSpace(existingId))
                {
                    throw new InvalidOperationException("Could not find id property on existing device");
                }
                device.id = existingId;
            }

            DeviceSchemaHelper.UpdateUpdatedTime(device);

            device = await _docDbRestUtil.UpdateDocumentAsync(device);

            return device;
        }

        public async Task<dynamic> UpdateDeviceEnabledStatusAsync(string deviceId, bool isEnabled)
        {
            dynamic existingDevice = await GetDeviceAsync(deviceId);

            if (existingDevice == null)
            {
                throw new DeviceNotRegisteredException(deviceId);
            }

            dynamic deviceProps = DeviceSchemaHelper.GetDeviceProperties(existingDevice);
            deviceProps.HubEnabledState = isEnabled;
            DeviceSchemaHelper.UpdateUpdatedTime(existingDevice);

            existingDevice = await _docDbRestUtil.UpdateDocumentAsync(existingDevice);

            return existingDevice;
        }

        /// <summary>
        /// Searches the DeviceProperties of all devices in the DocumentDB, sorts them and pages based on the provided values
        /// </summary>
        /// <param name="query">Object containing search, filtering, paging, and other info</param>
        /// <returns></returns>
        public async Task<DeviceListQueryResult> GetDeviceList(DeviceListQuery query)
        {
            List<dynamic> deviceList = await GetAllDevicesAsync();

            IQueryable<dynamic> filteredDevices = FilterHelper.FilterDeviceList(deviceList.AsQueryable<dynamic>(), query.Filters);

            IQueryable<dynamic> filteredAndSearchedDevices = SearchDeviceList(filteredDevices, query.SearchQuery);

            IQueryable<dynamic> sortedDevices = SortDeviceList(filteredAndSearchedDevices, query.SortColumn, query.SortOrder);

            List<dynamic> pagedDeviceList = sortedDevices.Skip(query.Skip).Take(query.Take).ToList();

            int filteredCount = filteredAndSearchedDevices.Count();

            return new DeviceListQueryResult()
            {
                Results = pagedDeviceList,
                TotalDeviceCount = deviceList.Count,
                TotalFilteredCount = filteredCount
            };
        }

        /// <summary>
        /// Searches the DeviceProperties of the provided device list for the given search term
        /// (case insensitive)
        /// </summary>
        /// <param name="deviceList">List to searcn</param>
        /// <param name="search">Term to search for</param>
        /// <returns>List of devices that match the given search</returns>
        private IQueryable<dynamic> SearchDeviceList(IQueryable<dynamic> deviceList, string search)
        {
            if (string.IsNullOrWhiteSpace(search))
            {
                return deviceList;
            }

            Func<dynamic, bool> filter =
                (d) =>
                {
                    return SearchTypePropertiesForValue(d, search);
                };

            // look for all devices that contain the search value in one of the DeviceProperties Properties
            return deviceList.Where(filter).AsQueryable();
        }

        /// <summary>
        /// Looks in all the Properties of the DeviceProperties instance on a device for the given search term
        /// </summary>
        /// <param name="device">Device to search</param>
        /// <param name="search">Value to search for</param>
        /// <returns>true - if at least one of the properties in DeviceProperties contains the value, false - no match was found</returns>
        private bool SearchTypePropertiesForValue(dynamic device, string search)
        {
            object devProps = null;

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

        /// <summary>
        /// Sorts the device list on the given column in the given order
        /// </summary>
        /// <param name="deviceList">List of devices to sort</param>
        /// <param name="sortColumn">Column to sort on</param>
        /// <param name="sortOrder">Order to sort (asc/desc)</param>
        /// <returns>Sorted device list</returns>
        private IQueryable<dynamic> SortDeviceList(IQueryable<dynamic> deviceList, string sortColumn, QuerySortOrder sortOrder)
        {
            Func<dynamic, dynamic> getPropVal;
            Func<dynamic, dynamic> keySelector;

            // if a sort column was not provided then return the full device list in its original sort
            if (string.IsNullOrWhiteSpace(sortColumn))
            {
                return deviceList;
            }

            getPropVal =
                ReflectionHelper.ProducePropertyValueExtractor(
                    sortColumn,
                    false,
                    false);

            keySelector =
                (item) =>
                {
                    dynamic deviceProperties;

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
