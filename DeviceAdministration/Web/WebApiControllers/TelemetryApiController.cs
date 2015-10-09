using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web.Http;
using Microsoft.Azure.Devices.Applications.RemoteMonitoring.DeviceAdmin.Infrastructure.BusinessLogic;
using Microsoft.Azure.Devices.Applications.RemoteMonitoring.DeviceAdmin.Infrastructure.Models;
using Microsoft.Azure.Devices.Applications.RemoteMonitoring.DeviceAdmin.Web.Models;
using Microsoft.Azure.Devices.Applications.RemoteMonitoring.DeviceAdmin.Web.Security;

namespace Microsoft.Azure.Devices.Applications.RemoteMonitoring.DeviceAdmin.Web.WebApiControllers
{
    /// <summary>
    /// A WebApiControllerBase-derived class for telemetry-related end points.
    /// </summary>
    [RoutePrefix("api/v1/telemetry")]
    public class TelemetryApiController : WebApiControllerBase
    {
        #region Constants

        private const double MaxDeviceSummaryAgeMinutes = 10.0;
        private const int MaxHistoryItems = 18;

        #endregion

        #region Instance Variables

        private readonly IAlertsLogic _alertsLogic;
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
        public TelemetryApiController(
            IDeviceTelemetryLogic deviceTelemetryLogic,
            IAlertsLogic alertsLogic)
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

            ValidateArgumentPopulation("deviceId", deviceId);

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

            ValidateArgumentPopulation("deviceId", deviceId);

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

            ValidateArgumentPopulation("deviceId", deviceId);

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
            IEnumerable<AlertHistoryItemModel> data;
            List<AlertHistoryItemModel> historyItems;
            Func<Task<AlertHistoryResultsModel>> loadHistoryItems;

            historyItems = new List<AlertHistoryItemModel>();

            loadHistoryItems =
                async () => {

                    historyItems = new List<AlertHistoryItemModel>();
                    data = 
                        await _alertsLogic.LoadLatestAlertHistoryAsync(
                            MaxHistoryItems);
                    if (data != null)
                    {
                        historyItems.AddRange(data);
                    }

                    return new AlertHistoryResultsModel
                    {
                        Data = historyItems,
                        TotalAlertCount = historyItems.Count,
                        TotalFilteredCount = historyItems.Count
                    };
                };

            return await GetServiceResponseAsync<AlertHistoryResultsModel>(
                loadHistoryItems,
                false);
        }

        #endregion

        #endregion
    }
}