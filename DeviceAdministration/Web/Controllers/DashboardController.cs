using Microsoft.Azure.Devices.Applications.RemoteMonitoring.Common.DeviceSchema;
using Microsoft.Azure.Devices.Applications.RemoteMonitoring.Common.Exceptions;
using Microsoft.Azure.Devices.Applications.RemoteMonitoring.DeviceAdmin.Infrastructure.BusinessLogic;
using Microsoft.Azure.Devices.Applications.RemoteMonitoring.DeviceAdmin.Infrastructure.Models;
using Microsoft.Azure.Devices.Applications.RemoteMonitoring.DeviceAdmin.Web.Models;
using Microsoft.Azure.Devices.Applications.RemoteMonitoring.DeviceAdmin.Web.Security;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web.Mvc;
using Microsoft.Azure.Devices.Applications.RemoteMonitoring.Common.Configurations;
using StringPair = System.Collections.Generic.KeyValuePair<string, string>;

namespace Microsoft.Azure.Devices.Applications.RemoteMonitoring.DeviceAdmin.Web.Controllers
{
    using System.Web;

    /// <summary>
    /// A Controller for Dashboard-related views.
    /// </summary>
    [Authorize]
    [OutputCache(CacheProfile = "NoCacheProfile")]
    public class DashboardController : Controller
    {
        private const double MaxDeviceSummaryAgeMinutes = 10.0;
        private const int MaxDevicesToDisplayOnDashboard = 200;

        private readonly IDeviceLogic _deviceLogic;
        private readonly IDeviceTelemetryLogic _deviceTelemetryLogic;
        private readonly IConfigurationProvider _configProvider;

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

        [RequirePermission(Permission.ViewTelemetry)]
        public async Task<ActionResult> Index()
        {
            var model = new DashboardModel();

            List<Infrastructure.Models.FilterInfo> filters = new List<Infrastructure.Models.FilterInfo>();
            filters.Add(new Infrastructure.Models.FilterInfo()
                {
                    ColumnName = "status", 
                    FilterType = FilterType.Status, 
                    FilterValue = "Running"
                });
            var query = new DeviceListQuery()
            {
                Skip = 0,
                Take = MaxDevicesToDisplayOnDashboard,
                SortColumn = "DeviceID",
                Filters = filters
            };

            DeviceListQueryResult queryResult = await _deviceLogic.GetDevices(query);
            if ((queryResult != null) && (queryResult.Results != null))
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

            // Set key to empty if passed value 0 from arm template
            string key = _configProvider.GetConfigurationSettingValue("MapApiQueryKey");
            model.MapApiQueryKey = key.Equals("0") ? string.Empty : key;

            return View(model);
        }


        [HttpGet]
        [Route("culture/{cultureName}")]
        public ActionResult SetCulture(string cultureName)
        {
            // Save culture in a cookie
            HttpCookie cookie = this.Request.Cookies[Constants.CultureCookieName];

            if (cookie != null)
            {
                cookie.Value = cultureName; // update cookie value
            }
            else
            {
                cookie = new HttpCookie(Constants.CultureCookieName);
                cookie.Value = cultureName;
                cookie.Expires = DateTime.Now.AddYears(1);
            }

            Response.Cookies.Add(cookie);

            return RedirectToAction("Index");
        }
    }
}