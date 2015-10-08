using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web.Mvc;
using Microsoft.Azure.Devices.Applications.RemoteMonitoring.Common.DeviceSchema;
using Microsoft.Azure.Devices.Applications.RemoteMonitoring.Common.Exceptions;
using Microsoft.Azure.Devices.Applications.RemoteMonitoring.DeviceAdmin.Infrastructure.BusinessLogic;
using Microsoft.Azure.Devices.Applications.RemoteMonitoring.DeviceAdmin.Infrastructure.Models;
using Microsoft.Azure.Devices.Applications.RemoteMonitoring.DeviceAdmin.Web.Models;
using Microsoft.Azure.Devices.Applications.RemoteMonitoring.DeviceAdmin.Web.Security;

namespace Microsoft.Azure.Devices.Applications.RemoteMonitoring.DeviceAdmin.Web.Controllers
{
    using StringPair = KeyValuePair<string, string>;
    using Microsoft.Azure.Devices.Applications.RemoteMonitoring.Common;
    using Microsoft.Azure.Devices.Applications.RemoteMonitoring.Common.Configurations;

    /// <summary>
    /// A Controller for Dashboard-related views.
    /// </summary>
    [Authorize]
    [OutputCache(CacheProfile = "NoCacheProfile")]
    public class DashboardController : Controller
    {
        #region Constants

        private const double MaxDeviceSummaryAgeMinutes = 10.0;
        private const int MaxDevicesToDisplayOnDashboard = 200;

        #endregion

        #region Instance Variables

        private readonly IDeviceLogic _deviceLogic;
        private readonly IDeviceTelemetryLogic _deviceTelemetryLogic;
        private readonly IConfigurationProvider _configProvider;

        #endregion

        #region Constructors

        /// <summary>
        /// Initializes a new instance of the DashboardController class.
        /// </summary>
        /// <param name="deviceLogic">
        /// The IDeviceLogic implementation that the new instance will use.
        /// </param>
        /// <param name="deviceTelemetryLogic">
        /// The IDeviceTelemetryLogic implementation that the new instance will 
        /// use.
        /// </param>
        public DashboardController(
            IDeviceLogic deviceLogic,
            IDeviceTelemetryLogic deviceTelemetryLogic,
            IConfigurationProvider configProvider)
        {
            if (deviceLogic == null)
            {
                throw new ArgumentNullException("deviceLogic");
            }

            if (deviceTelemetryLogic == null)
            {
                throw new ArgumentNullException("deviceTelemetryLogic");
            }

            if (configProvider == null)
            {
                throw new ArgumentNullException("configProvider");
            }

            _deviceLogic = deviceLogic;
            _deviceTelemetryLogic = deviceTelemetryLogic;
            _configProvider = configProvider;
        }

        #endregion

        #region Public Methods

        #region Instance Method: Index

        [RequirePermission(Permission.ViewTelemetry)]
        public async Task<ActionResult> Index()
        {
            DashboardModel model;
            DeviceListQuery query;
            DeviceListQueryResult queryResult;

            model = new DashboardModel();

            List<Infrastructure.Models.FilterInfo> filters = new List<Infrastructure.Models.FilterInfo>();
            filters.Add(new Infrastructure.Models.FilterInfo()
                {
                    ColumnName = "status", 
                    FilterType = FilterType.Status, 
                    FilterValue = "Running"
                });
            query = new DeviceListQuery()
            {
                Skip = 0,
                Take = MaxDevicesToDisplayOnDashboard,
                SortColumn = "DeviceID",
                Filters = filters
            };

            queryResult = await _deviceLogic.GetDevices(query);
            if ((queryResult != null) &&
                (queryResult.Results != null))
            {
                foreach (dynamic devInfo in queryResult.Results)
                {

                    string deviceId;
                    try
                    {
                        deviceId = DeviceSchemaHelper.GetDeviceID(devInfo);
                    }
                    catch (DeviceRequiredPropertyNotFoundException)
                    {
                        continue;
                    }

                    model.DeviceIdsForDropdown.Add(new StringPair(deviceId, deviceId));
                }
            }

            model.MapApiQueryKey = _configProvider.GetConfigurationSettingValue("MapApiQueryKey");

            return View(model);
        }

        #endregion

        #region Instance Method: LoadDashboardDevicePaneData

        [HttpGet]
        [WebApiRequirePermission(Permission.ViewTelemetry)]
        public async Task<DashboardDevicePaneDataModel> LoadDashboardDevicePaneData(
            string deviceId)
        {
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

            result = new DashboardDevicePaneDataModel();

            result.DeviceTelemetrySummaryModel =  summaryModel =
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
        }

        [HttpGet]
        [WebApiRequirePermission(Permission.ViewTelemetry)]
        public async Task<ActionResult> GetDeviceLocationData()
        {
            DeviceListQuery query;
            DeviceListQueryResult queryResult;

            query = new DeviceListQuery()
            {
                Skip = 0,
                Take = MaxDevicesToDisplayOnDashboard,
                SortColumn = "DeviceID"
            };
            queryResult = await _deviceLogic.GetDevices(query);
            DeviceListLocationsModel dataModel = _deviceLogic.ExtractLocationsData(queryResult.Results);

            return Json(dataModel, JsonRequestBehavior.AllowGet);
        }

        [HttpGet]
        [WebApiRequirePermission(Permission.ViewTelemetry)]
        public async Task<ActionResult> GetBingMapsApiKey()
        {
            String keySetting = await Task.Run(() => 
                {
                    return _configProvider.GetConfigurationSettingValue("MapApiQueryKey");
                });
            return Json(new { mapApiKey = keySetting }, JsonRequestBehavior.AllowGet);
        }

        #endregion

        #endregion
    }
}