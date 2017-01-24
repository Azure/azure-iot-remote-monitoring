using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Web.Mvc;
using DeviceManagement.Infrustructure.Connectivity.Exceptions;
using Microsoft.Azure.Devices.Applications.RemoteMonitoring.Common.Extensions;
using Microsoft.Azure.Devices.Applications.RemoteMonitoring.Common.Models;
using Microsoft.Azure.Devices.Applications.RemoteMonitoring.DeviceAdmin.Infrastructure.BusinessLogic;
using Microsoft.Azure.Devices.Applications.RemoteMonitoring.DeviceAdmin.Infrastructure.Models;
using Microsoft.Azure.Devices.Applications.RemoteMonitoring.DeviceAdmin.Infrastructure.Repository;
using Microsoft.Azure.Devices.Applications.RemoteMonitoring.DeviceAdmin.Web.Helpers;
using Microsoft.Azure.Devices.Applications.RemoteMonitoring.DeviceAdmin.Web.Security;

namespace Microsoft.Azure.Devices.Applications.RemoteMonitoring.DeviceAdmin.Web.Controllers
{
    public class AdvancedController : Controller
    {
        private const string CellularInvalidCreds = "400200";
        private const string CellularInvalidLicense = "400100";

        private readonly IApiRegistrationRepository _apiRegistrationRepository;
        private readonly ICellularExtensions _cellularExtensions;
        private readonly IDeviceLogic _deviceLogic;

        public AdvancedController(IDeviceLogic deviceLogic,
            IApiRegistrationRepository apiRegistrationRepository,
            ICellularExtensions cellularExtensions)
        {
            _deviceLogic = deviceLogic;
            _apiRegistrationRepository = apiRegistrationRepository;
            _cellularExtensions = cellularExtensions;
        }

        [RequirePermission(Permission.CellularConn)]
        public ActionResult CellularConn()
        {
            return View();
        }

        public PartialViewResult SelectAdvancedProcess()
        {
            var registrationModel = _apiRegistrationRepository.RecieveDetails();
            return PartialView("_SelectAdvancedProcess", registrationModel);
        }

        public PartialViewResult ApiRegistration()
        {
            var registrationModel = _apiRegistrationRepository.RecieveDetails();
            return PartialView("_ApiRegistration", registrationModel);
        }

        public async Task<PartialViewResult> DeviceAssociation()
        {
            IList<DeviceModel> devices = await GetDevices();

            try
            {
                if (_apiRegistrationRepository.IsApiRegisteredInAzure())
                {
                    ViewBag.HasRegistration = true;
                    ViewBag.UnassignedIccidList = _cellularExtensions.GetListOfAvailableIccids(devices);
                    ViewBag.UnassignedDeviceIds = _cellularExtensions.GetListOfAvailableDeviceIDs(devices);
                }
                else
                {
                    ViewBag.HasRegistration = false;
                }
            }
            catch (CellularConnectivityException)
            {
                ViewBag.HasRegistration = false;
            }

            return PartialView("_DeviceAssociation");
        }

        public async Task AssociateIccidWithDevice(string deviceId, string iccid)
        {
            if (string.IsNullOrEmpty(iccid))
            {
                throw new ArgumentNullException();
            }

            await UpdateDeviceAssociation(deviceId, iccid);
        }

        public async Task RemoveIccidFromDevice(string deviceId)
        {
            await UpdateDeviceAssociation(deviceId, null);
        }

        private async Task UpdateDeviceAssociation(string deviceId, string iccid)
        {
            if (string.IsNullOrEmpty(deviceId))
            {
                throw new ArgumentNullException();
            }

            DeviceModel device = await _deviceLogic.GetDeviceAsync(deviceId);
            device.SystemProperties.ICCID = iccid;
            await _deviceLogic.UpdateDeviceAsync(device);
        }

        public bool SaveRegistration(ApiRegistrationModel apiModel)
        {
            try
            {
                var registrationModel = _apiRegistrationRepository.RecieveDetails();

                if (registrationModel.ApiRegistrationProvider != apiModel.ApiRegistrationProvider)
                {
                    // TODO
                    // unregister the API and any connected devices
                }

                _apiRegistrationRepository.AmendRegistration(apiModel);

                var credentialsAreValid = _cellularExtensions.ValidateCredentials(apiModel.ApiRegistrationProvider);
                if (!credentialsAreValid)
                {
                    _apiRegistrationRepository.DeleteApiDetails();
                }
            }
            catch (Exception ex)
            {
                _apiRegistrationRepository.DeleteApiDetails();
                return false;
            }

            return true;
        }

        public bool DeleteRegistration()
        {
            return _apiRegistrationRepository.DeleteApiDetails();
        }

        [RequirePermission(Permission.HealthBeat)]
        public ActionResult HealthBeat()
        {
            return View();
        }

        [RequirePermission(Permission.LogicApps)]
        public ActionResult LogicApps()
        {
            return View();
        }

        private async Task<List<DeviceModel>> GetDevices()
        {
            var query = new DeviceListQuery
            {
                Take = 1000
            };

            var devices = await _deviceLogic.GetDevices(query);
            return devices.Results;
        }
    }
}