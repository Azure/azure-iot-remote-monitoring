using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Web.Mvc;
using Microsoft.Azure.Devices.Applications.RemoteMonitoring.Common.Configurations;
using Microsoft.Azure.Devices.Applications.RemoteMonitoring.Common.Exceptions;
using Microsoft.Azure.Devices.Applications.RemoteMonitoring.Common.Models;
using Microsoft.Azure.Devices.Applications.RemoteMonitoring.DeviceAdmin.Infrastructure.BusinessLogic;
using Microsoft.Azure.Devices.Applications.RemoteMonitoring.DeviceAdmin.Infrastructure.Models;
using Microsoft.Azure.Devices.Applications.RemoteMonitoring.DeviceAdmin.Web.Models;
using Microsoft.Azure.Devices.Applications.RemoteMonitoring.DeviceAdmin.Web.Security;
using StringPair = System.Collections.Generic.KeyValuePair<string, string>;

namespace Microsoft.Azure.Devices.Applications.RemoteMonitoring.DeviceAdmin.Web.Controllers
{
    using Helpers;
    using System.Threading;
    using System.Web;

    /// <summary>
    /// A Controller for Dashboard-related views.
    /// </summary>
    [Authorize]
    [OutputCache(CacheProfile = "NoCacheProfile")]
    public class DashboardController : Controller
    {
        private const int MaxDevicesToDisplayOnDashboard = 200;

        private readonly IDeviceLogic _deviceLogic;
        private readonly IConfigurationProvider _configProvider;

        /// <summary>
        /// Initializes a new instance of the DashboardController class.
        /// </summary>
        /// <param name="deviceLogic">The IDeviceLogic implementation that the new instance will use.</param>
        /// <param name="deviceTelemetryLogic"> The IDeviceTelemetryLogic implementation that the new instance will  use.</param>
        /// <param name="configProvider">The IConfigurationProvider implementation that the new intance will use.</param>
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
            _configProvider = configProvider;
        }

        [RequirePermission(Permission.ViewTelemetry)]
        public async Task<ActionResult> Index()
        {
            var model = new DashboardModel();
            var clauses = new List<Infrastructure.Models.Clause>
            {
                new Clause()
                {
                    ColumnName = "tags.HubEnabledState",
                    ClauseType = ClauseType.EQ,
                    ClauseValue = "Running"
                }
            };


            var query = new DeviceListFilter()
            {
                Skip = 0,
                Take = MaxDevicesToDisplayOnDashboard,
                SortColumn = "twin.deviceId",
                Clauses = clauses
            };

            DeviceListFilterResult filterResult = await _deviceLogic.GetDevices(query);

            if ((filterResult != null) && (filterResult.Results != null))
            {
                foreach (DeviceModel devInfo in filterResult.Results)
                {
                    string deviceId;
                    try
                    {
                        deviceId = devInfo.DeviceProperties.DeviceID;
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

            AddDefaultCultureIntoCookie();

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

        private void AddDefaultCultureIntoCookie()
        {
            if (this.Request == null) return;
            // Set default culture
            HttpCookie cookie = this.Request.Cookies[Constants.CultureCookieName];

            if (cookie == null)
            {
                cookie = new HttpCookie(Constants.CultureCookieName);
                cookie.Value = CultureHelper.GetClosestCulture(Thread.CurrentThread.CurrentCulture.Name).Name;
                cookie.Expires = DateTime.Now.AddYears(1);
                Response.Cookies.Add(cookie);
            }
        }
    }
}