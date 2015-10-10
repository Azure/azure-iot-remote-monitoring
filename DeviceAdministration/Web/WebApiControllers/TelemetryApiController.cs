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
using Microsoft.Azure.Devices.Applications.RemoteMonitoring.Common.Configurations;

namespace Microsoft.Azure.Devices.Applications.RemoteMonitoring.DeviceAdmin.Web.WebApiControllers
{
    /// <summary>
    /// A WebApiControllerBase-derived class for telemetry-related end points.
    /// </summary>
    [RoutePrefix("api/v1/telemetry")]
    public class TelemetryApiController : WebApiControllerBase
    {
        private const double MAX_DEVICE_SUMMARY_AGE_MINUTES = 10.0;
        private const int MAX_HISTORY_ITEMS = 18;
        private const int MAX_DEVICES_TO_DISPLAY_ON_DASHBOARD = 200;

        private readonly IAlertsLogic _alertsLogic;
        private readonly IDeviceTelemetryLogic _deviceTelemetryLogic;

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
        public TelemetryApiController(IDeviceTelemetryLogic deviceTelemetryLogic, IAlertsLogic alertsLogic)
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

        [HttpGet]
        [Route("dashboardDevicePane")]
        [WebApiRequirePermission(Permission.ViewTelemetry)]
        public async Task<HttpResponseMessage> GetDashboardDevicePaneDataAsync(string deviceId)
        {
            if (string.IsNullOrEmpty(deviceId))
            {
                throw new ArgumentException("deviceId is a null reference or empty string.", "deviceId");
            }

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
        public async Task<HttpResponseMessage> GetDeviceTelemetrySummaryAsync(
            string deviceId)
        {
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
            IEnumerable<AlertHistoryItemModel> data;

            List<AlertHistoryItemModel> historyItems = new List<AlertHistoryItemModel>();

            Func<Task<AlertHistoryResultsModel>> loadHistoryItems =
                async () => {

                    historyItems = new List<AlertHistoryItemModel>();
                    data = await _alertsLogic.LoadLatestAlertHistoryAsync(MAX_HISTORY_ITEMS);
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
    }
}