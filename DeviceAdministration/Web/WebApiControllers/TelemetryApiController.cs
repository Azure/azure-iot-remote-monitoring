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

namespace Microsoft.Azure.Devices.Applications.RemoteMonitoring.DeviceAdmin.Web.WebApiControllers
{
    using StringPair = KeyValuePair<string, string>;

    /// <summary>
    /// A WebApiControllerBase-derived class for telemetry-related end points.
    /// </summary>
    [RoutePrefix("api/v1/telemetry")]
    public class TelemetryApiController : WebApiControllerBase
    {
        #region Constants

        private const int MaxDevicesToDisplayOnDashboard = 200;

        private const double CautionAlertMaxMinutes = 91.0;
        private const double CriticalAlertMaxMinutes = 11.0;
        private const double MaxDeviceSummaryAgeMinutes = 10.0;
        private const int MaxHistoryItems = 18;

        #endregion

        #region Instance Variables

        private readonly IAlertsLogic _alertsLogic;
        private readonly IDeviceLogic _deviceLogic;
        private readonly IDeviceTelemetryLogic _deviceTelemetryLogic;

        #endregion

        #region Constructors

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
            IDeviceLogic deviceLogic)
        {
            if (deviceTelemetryLogic == null)
            {
                throw new ArgumentNullException("deviceTelemetryLogic");
            }

            if (alertsLogic == null)
            {
                throw new ArgumentNullException("alertsLogic");
            }

            _deviceTelemetryLogic = deviceTelemetryLogic;
            _alertsLogic = alertsLogic;
            _deviceLogic = deviceLogic;
        }

        #endregion

        #region Public Methods

        #region Instance Method: GetDashboardDevicePaneDataAsync

        [HttpGet]
        [Route("dashboardDevicePane")]
        [WebApiRequirePermission(Permission.ViewTelemetry)]
        public async Task<HttpResponseMessage> GetDashboardDevicePaneDataAsync(
            string deviceId)
        {
            Func<Task<DashboardDevicePaneDataModel>> getTelemetry;
            DateTime minTime;
            DashboardDevicePaneDataModel result;
            DeviceTelemetrySummaryModel summaryModel;
            IEnumerable<DeviceTelemetryModel> telemetryModels;

            if (string.IsNullOrEmpty(deviceId))
            {
                throw new ArgumentException(
                    "deviceId is a null reference or empty string.",
                    "deviceId");
            }

            result = new DashboardDevicePaneDataModel()
            {
                DeviceId = deviceId
            };

            getTelemetry =
                async () =>
                {
                    result.DeviceTelemetrySummaryModel = summaryModel =
                        await _deviceTelemetryLogic.LoadLatestDeviceTelemetrySummaryAsync(
                            deviceId,
                            DateTime.Now.AddMinutes(-MaxDeviceSummaryAgeMinutes));

                    if ((summaryModel != null) &&
                        summaryModel.Timestamp.HasValue &&
                        summaryModel.TimeFrameMinutes.HasValue)
                    {
                        minTime =
                            summaryModel.Timestamp.Value.AddMinutes(
                                -summaryModel.TimeFrameMinutes.Value);

                        telemetryModels =
                            await _deviceTelemetryLogic.LoadLatestDeviceTelemetryAsync(
                                deviceId,
                                minTime);

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

        #endregion

        #region Instance Method: GetDeviceTelemetryAsync

        [HttpGet]
        [Route("list")]
        [WebApiRequirePermission(Permission.ViewTelemetry)]
        public async Task<HttpResponseMessage> GetDeviceTelemetryAsync(
            string deviceId,
            DateTime minTime)
        {
            Func<Task<DeviceTelemetryModel[]>> getTelemetry;
            IEnumerable<DeviceTelemetryModel> telemetryModels;

            getTelemetry =
                async () =>
                {
                    telemetryModels =
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

        #endregion

        #region Instance Method: GetDeviceTelemetrySummaryAsync

        [HttpGet]
        [Route("summary")]
        [WebApiRequirePermission(Permission.ViewTelemetry)]
        public async Task<HttpResponseMessage> GetDeviceTelemetrySummaryAsync(
            string deviceId)
        {
            Func<Task<DeviceTelemetrySummaryModel>> getTelemetrySummary;

            getTelemetrySummary =
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

        #endregion

        #region Instance Method: GetLatestAlertHistoryAsync

        [HttpGet]
        [Route("alertHistory")]
        [WebApiRequirePermission(Permission.ViewTelemetry)]
        public async Task<HttpResponseMessage> GetLatestAlertHistoryAsync()
        {
            Func<Task<AlertHistoryResultsModel>> loadHistoryItems;

            loadHistoryItems =
                async () =>
                {
                    DateTime currentTime = DateTime.UtcNow;

                    List<AlertHistoryItemModel> historyItems = new List<AlertHistoryItemModel>();
                    List<AlertHistoryDeviceModel> deviceModels = new List<AlertHistoryDeviceModel>();

                    AlertHistoryResultsModel resultsModel = new AlertHistoryResultsModel();

                    IEnumerable<AlertHistoryItemModel> data =
                        await _alertsLogic.LoadLatestAlertHistoryAsync(currentTime.AddMinutes(-CautionAlertMaxMinutes));
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
                                        _deviceTelemetryLogic.ProducedGetLatestDeviceAlertTime(historyItems);

                                    foreach (DeviceLocationModel locationModel in locationsModel.DeviceLocationList)
                                    {
                                        if ((locationModel == null) ||
                                            string.IsNullOrWhiteSpace(locationModel.DeviceId))
                                        {
                                            continue;
                                        }

                                        AlertHistoryDeviceModel deviceModel = new AlertHistoryDeviceModel()
                                        {
                                            DeviceId = locationModel.DeviceId,
                                            Latitude = locationModel.Latitude,
                                            Longitude = locationModel.Longitude
                                        };

                                        DateTime? lastStatusTime = getStatusTime(locationModel.DeviceId);
                                        if (lastStatusTime.HasValue)
                                        {
                                            TimeSpan deltaTime = currentTime - lastStatusTime.Value.ToUniversalTime();

                                            if (deltaTime.TotalMinutes < CriticalAlertMaxMinutes)
                                            {
                                                deviceModel.Status = AlertHistoryDeviceStatus.Critical;
                                            }
                                            else if (deltaTime.TotalMinutes < CautionAlertMaxMinutes)
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

                    resultsModel.Data = historyItems.Take(MaxDevicesToDisplayOnDashboard).ToList();
                    resultsModel.Devices = deviceModels;
                    resultsModel.TotalAlertCount = historyItems.Count;
                    resultsModel.TotalFilteredCount = historyItems.Count;

                    return resultsModel;
                };

            return await GetServiceResponseAsync<AlertHistoryResultsModel>(
                loadHistoryItems,
                false);
        }

        #endregion

        #endregion

        #region Private Methods

        private async Task<List<dynamic>> LoadAllDevicesAsync()
        {
            string deviceId;
            DeviceListQuery query;
            DeviceListQueryResult queryResult;

            query = new DeviceListQuery()
            {
                Skip = 0,
                Take = MaxDevicesToDisplayOnDashboard,
                SortColumn = "DeviceID"
            };

            List<dynamic> devices = new List<dynamic>();
            queryResult = await _deviceLogic.GetDevices(query);
            if ((queryResult != null) &&
                (queryResult.Results != null))
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

        #endregion
    }
}