﻿using System;
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
        private async Task<List<DeviceND>> GetAllDevicesAsyncND()
        {
            IEnumerable docs;
            List<DeviceND> deviceList = new List<DeviceND>();

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
                            deviceList.Add(TypeMapper.Get().map<DeviceND>(doc));
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
        public async Task<DeviceND> GetDeviceAsyncND(string deviceId)
        {
            JToken result = null;

            Dictionary<string, Object> queryParams = new Dictionary<string, Object>();
            queryParams.Add("@id", deviceId);
            DocDbRestQueryResult response = await _docDbRestUtil.QueryCollectionAsync("SELECT VALUE root FROM root WHERE (root.DeviceProperties.DeviceID = @id)", queryParams);
            JArray foundDevices = response.ResultSet;

            if (foundDevices != null && foundDevices.Count > 0)
            {
                result = foundDevices.Children().ElementAt(0);
            }

            DeviceND d = TypeMapper.Get().map<DeviceND>(result);
            return d;
        }

        /// <summary>
        /// Adds a device to the DocumentDB.
        /// Throws a DeviceAlreadyRegisteredException if a device already exists in the database with the provided deviceId
        /// </summary>
        /// <param name="device"></param>
        /// <returns></returns>
        public async Task<DeviceND> AddDeviceAsyncND(DeviceND device)
        {
            string deviceId = device.DeviceProperties.DeviceID;
            DeviceND existingDevice = await GetDeviceAsyncND(deviceId);

            if (existingDevice != null)
            {
                throw new DeviceAlreadyRegisteredException(deviceId);
            }

            JObject d = await _docDbRestUtil.SaveNewDocumentAsync<DeviceND>(device);
            device = TypeMapper.Get().map<DeviceND>(d);

            return device;
        }

        public async Task RemoveDeviceAsync(string deviceId)
        {
            DeviceND existingDevice = await GetDeviceAsyncND(deviceId);

            if (existingDevice == null)
            {
                throw new DeviceNotRegisteredException(deviceId);
            }

            await _docDbRestUtil.DeleteDocumentAsync<DeviceND>(existingDevice);
        }

        /// <summary>
        /// Updates an existing device in the DocumentDB
        /// Throws a DeviceNotRegisteredException is the device does not already exist in the DocumentDB
        /// </summary>
        /// <param name="device"></param>
        /// <returns></returns>
        public async Task<DeviceND> UpdateDeviceAsyncND(DeviceND device)
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

            DeviceND existingDevice = await GetDeviceAsyncND(deviceId);

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

            JObject d = await _docDbRestUtil.UpdateDocumentAsync<DeviceND>(device);
            return TypeMapper.Get().map<DeviceND>(d);
        }

        public async Task<DeviceND> UpdateDeviceEnabledStatusAsyncND(string deviceId, bool isEnabled)
        {
            DeviceND existingDevice = await GetDeviceAsyncND(deviceId);

            if (existingDevice == null)
            {
                throw new DeviceNotRegisteredException(deviceId);
            }

            DeviceProperties deviceProps = DeviceSchemaHelperND.GetDeviceProperties(existingDevice);
            deviceProps.HubEnabledState = isEnabled;
            DeviceSchemaHelperND.UpdateUpdatedTime(existingDevice);

            JObject updatedDevice = await _docDbRestUtil.UpdateDocumentAsync<DeviceND>(existingDevice);
            return TypeMapper.Get().map<DeviceND>(updatedDevice);
        }

        public async Task<DeviceListQueryResultND> GetDeviceListND(DeviceListQuery query)
        {
            List<DeviceND> deviceList = await GetAllDevicesAsyncND();

            IQueryable<DeviceND> filteredDevices = FilterHelperND.FilterDeviceList(deviceList.AsQueryable<DeviceND>(), query.Filters);

            IQueryable<DeviceND> filteredAndSearchedDevices = SearchDeviceListND(filteredDevices, query.SearchQuery);

            IQueryable<DeviceND> sortedDevices = SortDeviceListND(filteredAndSearchedDevices, query.SortColumn, query.SortOrder);

            List<DeviceND> pagedDeviceList = sortedDevices.Skip(query.Skip).Take(query.Take).ToList();

            int filteredCount = filteredAndSearchedDevices.Count();

            return new DeviceListQueryResultND()
            {
                Results = pagedDeviceList,
                TotalDeviceCount = deviceList.Count,
                TotalFilteredCount = filteredCount
            };
        }

        private IQueryable<DeviceND> SearchDeviceListND(IQueryable<DeviceND> deviceList, string search)
        {
            if (string.IsNullOrWhiteSpace(search))
            {
                return deviceList;
            }

            Func<DeviceND, bool> filter =
                (d) => SearchTypePropertiesForValueND(d, search);

            // look for all devices that contain the search value in one of the DeviceProperties Properties
            return deviceList.Where(filter).AsQueryable();
        }
       
        private bool SearchTypePropertiesForValueND(DeviceND device, string search)
        {
            DeviceProperties devProps = null;

            // if the device or its system properties are null then
            // there's nothing that can be searched on
            if ((device == null) ||
                ((devProps = DeviceSchemaHelperND.GetDeviceProperties(device)) == null))
            {
                return false;
            }

            try
            {
                devProps = DeviceSchemaHelperND.GetDeviceProperties(device);
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

        private IQueryable<DeviceND> SortDeviceListND(IQueryable<DeviceND> deviceList, string sortColumn, QuerySortOrder sortOrder)
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

            Func<DeviceND, dynamic> keySelector =
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
                            return DeviceSchemaHelperND.GetHubEnabledState(item);
                        }
                        catch (DeviceRequiredPropertyNotFoundException)
                        {
                            return null;
                        }
                    }

                    try
                    {
                        deviceProperties =
                            DeviceSchemaHelperND.GetDeviceProperties(item);
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