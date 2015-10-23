using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web.Http;
using Microsoft.Azure.Devices.Applications.RemoteMonitoring.Common.DeviceSchema;
using Microsoft.Azure.Devices.Applications.RemoteMonitoring.Common.Exceptions;
using Microsoft.Azure.Devices.Applications.RemoteMonitoring.DeviceAdmin.Infrastructure.BusinessLogic;
using Microsoft.Azure.Devices.Applications.RemoteMonitoring.DeviceAdmin.Infrastructure.Models;
using Microsoft.Azure.Devices.Applications.RemoteMonitoring.DeviceAdmin.Web.Models;
using Microsoft.Azure.Devices.Applications.RemoteMonitoring.DeviceAdmin.Web.Security;
using Microsoft.Azure.Devices.Applications.RemoteMonitoring.Common.Configurations;

namespace Microsoft.Azure.Devices.Applications.RemoteMonitoring.DeviceAdmin.Web.WebApiControllers
{
    using StringPair = KeyValuePair<string, string>;

    /// <summary>
    /// A WebApiControllerBase-derived class for telemetry-related end points.
    /// </summary>
    [RoutePrefix("api/v1/telemetry")]
    public class TelemetryApiController : WebApiControllerBase
    {
        private const double MAX_DEVICE_SUMMARY_AGE_MINUTES = 10.0;
        private const int DISPLAYED_HISTORY_ITEMS = 18;
        private const int MAX_DEVICES_TO_DISPLAY_ON_DASHBOARD = 200;

        private static readonly TimeSpan CautionAlertMaxDelta = TimeSpan.FromMinutes(91.0);
        private static readonly TimeSpan CriticalAlertMaxDelta = TimeSpan.FromMinutes(11.0);

        private readonly IAlertsLogic _alertsLogic;
        private readonly IDeviceLogic _deviceLogic;
        private readonly IDeviceTelemetryLogic _deviceTelemetryLogic;
        private readonly IConfigurationProvider _configProvider;

        /// <summary>
        /// Initializes a new instance of the TelemetryApiController class.
        /// </summary>
        /// <param name="deviceTelemetryLogic">
        /// The IDeviceRegistryListLogic implementation that the new instance
        /// will use.
        /// </param>
        /// <param name="alertsLogic">
        /// The IAlertsLogic implementation that the new instance will use.
        /// </param>
        /// <param name="deviceLogic">
        /// The IDeviceLogic implementation that the new instance will use.
        /// </param>
        public TelemetryApiController(
            IDeviceTelemetryLogic deviceTelemetryLogic,
            IAlertsLogic alertsLogic,
            IDeviceLogic deviceLogic,
            IConfigurationProvider configProvider)
        {
            if (deviceTelemetryLogic == null)
            {
                throw new ArgumentNullException("deviceTelemetryLogic");
            }

            if (alertsLogic == null)
            {
                throw new ArgumentNullException("alertsLogic");
            }

            if(deviceLogic == null)
            {
                throw new ArgumentNullException("deviceLogic");
            }

            if (configProvider == null)
            {
                throw new ArgumentNullException("configProvider");
            }

            _deviceTelemetryLogic = deviceTelemetryLogic;
            _alertsLogic = alertsLogic;
            _deviceLogic = deviceLogic;
            _configProvider = configProvider;
        }

        [HttpGet]
        [Route("dashboardDevicePane")]
        [WebApiRequirePermission(Permission.ViewTelemetry)]
        public async Task<HttpResponseMessage> GetDashboardDevicePaneDataAsync(string deviceId)
        {
            ValidateArgumentNotNullOrWhitespace("deviceId", deviceId);

            DashboardDevicePaneDataModel result = new DashboardDevicePaneDataModel()
            {
                DeviceId = deviceId
            };

            Func<Task<DashboardDevicePaneDataModel>> getTelemetry =
                async () =>
                {
                    DeviceTelemetrySummaryModel summaryModel;

                    result.DeviceTelemetrySummaryModel = summaryModel =
                        await _deviceTelemetryLogic.LoadLatestDeviceTelemetrySummaryAsync(
                            deviceId, DateTime.Now.AddMinutes(-MAX_DEVICE_SUMMARY_AGE_MINUTES));

                    IEnumerable<DeviceTelemetryModel> telemetryModels;
                    if ((summaryModel != null) && summaryModel.Timestamp.HasValue && summaryModel.TimeFrameMinutes.HasValue)
                    {
                        DateTime minTime = summaryModel.Timestamp.Value.AddMinutes(-summaryModel.TimeFrameMinutes.Value);

                        telemetryModels = await _deviceTelemetryLogic.LoadLatestDeviceTelemetryAsync(deviceId, minTime);

                    }
                    else
                    {
                        telemetryModels = null;

                        result.DeviceTelemetrySummaryModel =
                            new DeviceTelemetrySummaryModel();
                    }

                    if (telemetryModels == null)
                    {
                        result.DeviceTelemetryModels = new DeviceTelemetryModel[0];
                    }
                    else
                    {
                        result.DeviceTelemetryModels =
                            telemetryModels.OrderBy(t => t.Timestamp).ToArray();
                    }

                    return result;
                };

            return await GetServiceResponseAsync<DashboardDevicePaneDataModel>(
                getTelemetry,
                false);
        }

        [HttpGet]
        [Route("list")]
        [WebApiRequirePermission(Permission.ViewTelemetry)]
        public async Task<HttpResponseMessage> GetDeviceTelemetryAsync(
            string deviceId,
            DateTime minTime)
        {
            ValidateArgumentNotNullOrWhitespace("deviceId", deviceId);

            Func<Task<DeviceTelemetryModel[]>> getTelemetry =
                async () =>
                {
                    IEnumerable<DeviceTelemetryModel> telemetryModels =
                        await _deviceTelemetryLogic.LoadLatestDeviceTelemetryAsync(
                            deviceId, 
                            minTime);

                    if (telemetryModels == null)
                    {
                        return new DeviceTelemetryModel[0];
                    }

                    return telemetryModels.OrderBy(t => t.Timestamp).ToArray();
                };

            return await GetServiceResponseAsync<DeviceTelemetryModel[]>(
                getTelemetry,
                false);
        }

        [HttpGet]
        [Route("summary")]
        [WebApiRequirePermission(Permission.ViewTelemetry)]
        public async Task<HttpResponseMessage> GetDeviceTelemetrySummaryAsync(string deviceId)
        {
            ValidateArgumentNotNullOrWhitespace("deviceId", deviceId);

            Func<Task<DeviceTelemetrySummaryModel>> getTelemetrySummary =
                async () =>
                {
                    return await _deviceTelemetryLogic.LoadLatestDeviceTelemetrySummaryAsync(
                        deviceId,
                        null);
                };

            return await GetServiceResponseAsync<DeviceTelemetrySummaryModel>(
                getTelemetrySummary,
                false);
        }

        [HttpGet]
        [Route("alertHistory")]
        [WebApiRequirePermission(Permission.ViewTelemetry)]
        public async Task<HttpResponseMessage> GetLatestAlertHistoryAsync()
        {
            Func<Task<AlertHistoryResultsModel>> loadHistoryItems =
                async () =>
                {
                    // Dates are stored internally as UTC and marked as such.  
                    // When parsed, they'll be made relative to the server's 
                    // time zone.  This is only in an issue on servers machines, 
                    // not set to GMT.
                    DateTime currentTime = DateTime.Now;

                    var historyItems = new List<AlertHistoryItemModel>();
                    var deviceModels = new List<AlertHistoryDeviceModel>();
                    var resultsModel = new AlertHistoryResultsModel();

                    IEnumerable<AlertHistoryItemModel> data =
                        await _alertsLogic.LoadLatestAlertHistoryAsync(
                            currentTime.Subtract(CautionAlertMaxDelta), 
                            DISPLAYED_HISTORY_ITEMS);

                    if (data != null)
                    {
                        historyItems.AddRange(data);

                        List<dynamic> devices = await LoadAllDevicesAsync();

                        if (devices != null)
                        {
                            DeviceListLocationsModel locationsModel = _deviceLogic.ExtractLocationsData(devices);
                            if (locationsModel != null)
                            {
                                resultsModel.MaxLatitude = locationsModel.MaximumLatitude;
                                resultsModel.MaxLongitude = locationsModel.MaximumLongitude;
                                resultsModel.MinLatitude = locationsModel.MinimumLatitude;
                                resultsModel.MinLongitude = locationsModel.MinimumLongitude;

                                if (locationsModel.DeviceLocationList != null)
                                {
                                    Func<string, DateTime?> getStatusTime =
                                        _deviceTelemetryLogic.ProduceGetLatestDeviceAlertTime(historyItems);

                                    foreach (DeviceLocationModel locationModel in locationsModel.DeviceLocationList)
                                    {
                                        if ((locationModel == null) || string.IsNullOrWhiteSpace(locationModel.DeviceId))
                                        {
                                            continue;
                                        }

                                        var deviceModel = new AlertHistoryDeviceModel()
                                        {
                                            DeviceId = locationModel.DeviceId,
                                            Latitude = locationModel.Latitude,
                                            Longitude = locationModel.Longitude
                                        };

                                        DateTime? lastStatusTime = getStatusTime(locationModel.DeviceId);
                                        if (lastStatusTime.HasValue)
                                        {
                                            TimeSpan deltaTime = currentTime - lastStatusTime.Value;

                                            if (deltaTime < CriticalAlertMaxDelta)
                                            {
                                                deviceModel.Status = AlertHistoryDeviceStatus.Critical;
                                            }
                                            else if (deltaTime < CautionAlertMaxDelta)
                                            {
                                                deviceModel.Status = AlertHistoryDeviceStatus.Caution;
                                            }
                                        }

                                        deviceModels.Add(deviceModel);
                                    }
                                }
                            }
                        }
                    }

                    resultsModel.Data = historyItems.Take(DISPLAYED_HISTORY_ITEMS).ToList();
                    resultsModel.Devices = deviceModels;
                    resultsModel.TotalAlertCount = historyItems.Count;
                    resultsModel.TotalFilteredCount = historyItems.Count;

                    return resultsModel;
                };

            return await GetServiceResponseAsync<AlertHistoryResultsModel>(loadHistoryItems, false);
        }

        private async Task<List<dynamic>> LoadAllDevicesAsync()
        {
            var query = new DeviceListQuery()
                    {
                Skip = 0,
                Take = MAX_DEVICES_TO_DISPLAY_ON_DASHBOARD,
                SortColumn = "DeviceID"
                    };

            string deviceId;
            var devices = new List<dynamic>();
            DeviceListQueryResult queryResult = await _deviceLogic.GetDevices(query);
            if ((queryResult != null) &&  (queryResult.Results != null))
            {
                string enabledState = "";
                dynamic props = null;
                foreach (dynamic devInfo in queryResult.Results)
                {
                    try
                    {
                        deviceId = DeviceSchemaHelper.GetDeviceID(devInfo);
                        props = DeviceSchemaHelper.GetDeviceProperties(devInfo);
                        enabledState = props.HubEnabledState;
                    }
                    catch (DeviceRequiredPropertyNotFoundException)
                    {
                        continue;
                    }

                    if (!string.IsNullOrWhiteSpace(deviceId))
                    {
                        devices.Add(devInfo);
                    }
                }
            }

            return devices;
        }

        [HttpGet]
        [Route("deviceLocationData")]
        [WebApiRequirePermission(Permission.ViewTelemetry)]
        public async Task<HttpResponseMessage> GetDeviceLocationData()
        {
            return await GetServiceResponseAsync<DeviceListLocationsModel>(async () =>
            {
                var query = new DeviceListQuery()
                {
                    Skip = 0,
                    Take = MAX_DEVICES_TO_DISPLAY_ON_DASHBOARD,
                    SortColumn = "DeviceID"
                };

                DeviceListQueryResult queryResult = await _deviceLogic.GetDevices(query);
                DeviceListLocationsModel dataModel = _deviceLogic.ExtractLocationsData(queryResult.Results);
 
                return dataModel;
            }, false);
        }

        [HttpGet]
        [Route("mapApiKey")]
        [WebApiRequirePermission(Permission.ViewTelemetry)]
        public async Task<HttpResponseMessage> GetMapApiKey()
        {
            return await GetServiceResponseAsync<string>(async () =>
            {
                String keySetting = await Task.Run(() =>
                {
                    return _configProvider.GetConfigurationSettingValue("MapApiQueryKey");
                });
                return keySetting;
            }, false);
        }
    }
}