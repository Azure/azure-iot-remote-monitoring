using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Web.Mvc;
using DeviceManagement.Infrustructure.Connectivity.Exceptions;
using DeviceManagement.Infrustructure.Connectivity.Services;
using GlobalResources;
using Microsoft.Azure.Devices.Applications.RemoteMonitoring.Common.Models;
using Microsoft.Azure.Devices.Applications.RemoteMonitoring.DeviceAdmin.Infrastructure.BusinessLogic;
using Microsoft.Azure.Devices.Applications.RemoteMonitoring.DeviceAdmin.Infrastructure.Models;
using Microsoft.Azure.Devices.Applications.RemoteMonitoring.DeviceAdmin.Infrastructure.Repository;
using Microsoft.Azure.Devices.Applications.RemoteMonitoring.DeviceAdmin.Web.Helpers;
using Microsoft.Azure.Devices.Applications.RemoteMonitoring.DeviceAdmin.Web.Security;
using Microsoft.Azure.Devices.Applications.RemoteMonitoring.DeviceAdmin.Web.Models;

namespace Microsoft.Azure.Devices.Applications.RemoteMonitoring.DeviceAdmin.Web.Controllers
{
    public class AdvancedController : Controller
    {
        private readonly IApiRegistrationRepository _apiRegistrationRepository;
        private readonly IExternalCellularService _cellularService;
        private readonly IDeviceLogic _deviceLogic;
        private const string CellularInvalidCreds = "400200";
        private const string CellularInvalidLicense = "400100";

        public AdvancedController(IDeviceLogic deviceLogic,
            IExternalCellularService cellularService,
            IApiRegistrationRepository apiRegistrationRepository)
        {
            _deviceLogic = deviceLogic;
            _cellularService = cellularService;
            _apiRegistrationRepository = apiRegistrationRepository;
        }

        [RequirePermission(Permission.CellularConn)]
        public ActionResult CellularConn()
        {
            return View();
        }

        public PartialViewResult ApiRegistrationJasper()
        {
            var registrationModel = _apiRegistrationRepository.RecieveDetails();
            registrationModel.CellularProvider = CellularProviderEnum.Jasper;
            return PartialView("_ApiRegistrationJasper", registrationModel);
        }

        public PartialViewResult ApiRegistrationEricsson()
        {
            var registrationModel = _apiRegistrationRepository.RecieveDetails();
            registrationModel.CellularProvider = CellularProviderEnum.Ericsson;
            return PartialView("_ApiRegistrationEricsson", registrationModel);
        }

        public async Task<PartialViewResult> DeviceAssociation()
        {
            var devices = await GetDevices();

            try
            {
                if (_apiRegistrationRepository.IsApiRegisteredInAzure())
                {
                    ViewBag.HasRegistration = true;
                    ViewBag.UnassignedIccidList = _cellularService.GetListOfAvailableIccids(devices);
                    ViewBag.UnassignedDeviceIds = _cellularService.GetListOfAvailableDeviceIDs(devices);
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

            var device = await _deviceLogic.GetDeviceAsync(deviceId);
            device.SystemProperties.ICCID = iccid;
            await _deviceLogic.UpdateDeviceAsync(device);
        }

        public bool SaveRegistration(ApiRegistrationModel apiModel)
        {
            _apiRegistrationRepository.AmendRegistration(apiModel);

            var credentialsAreValid = _cellularService.ValidateCredentials(apiModel.CellularProvider);
            if (!credentialsAreValid)
            {
                _apiRegistrationRepository.DeleteApiDetails();
            }
            return true;
        }

        public PartialViewResult SelectAdvancedProcess()
        {
            return PartialView("_SelectAdvancedProcess");
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

        private async Task<List<dynamic>> GetDevices()
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