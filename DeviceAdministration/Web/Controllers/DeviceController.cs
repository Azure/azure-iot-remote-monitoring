using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using System.Web.Mvc;
using DeviceManagement.Infrustructure.Connectivity.Exceptions;
using DeviceManagement.Infrustructure.Connectivity.Models.TerminalDevice;
using DeviceManagement.Infrustructure.Connectivity.Services;
using GlobalResources;
using Microsoft.Azure.Devices.Applications.RemoteMonitoring.Common.Configurations;
using Microsoft.Azure.Devices.Applications.RemoteMonitoring.Common.DeviceSchema;
using Microsoft.Azure.Devices.Applications.RemoteMonitoring.Common.Models;
using Microsoft.Azure.Devices.Applications.RemoteMonitoring.DeviceAdmin.Infrastructure.BusinessLogic;
using Microsoft.Azure.Devices.Applications.RemoteMonitoring.DeviceAdmin.Infrastructure.Exceptions;
using Microsoft.Azure.Devices.Applications.RemoteMonitoring.DeviceAdmin.Infrastructure.Models;
using Microsoft.Azure.Devices.Applications.RemoteMonitoring.DeviceAdmin.Infrastructure.Repository;
using Microsoft.Azure.Devices.Applications.RemoteMonitoring.DeviceAdmin.Web.Helpers;
using Microsoft.Azure.Devices.Applications.RemoteMonitoring.DeviceAdmin.Web.Models;
using Microsoft.Azure.Devices.Applications.RemoteMonitoring.DeviceAdmin.Web.Security;

namespace Microsoft.Azure.Devices.Applications.RemoteMonitoring.DeviceAdmin.Web.Controllers
{
    [Authorize]
    [OutputCache(CacheProfile = "NoCacheProfile")]
    public class DeviceController : Controller
    {
        private readonly IApiRegistrationRepository _apiRegistrationRepository;
        private readonly IExternalCellularService _cellularService;
        private readonly IDeviceLogic _deviceLogic;
        private readonly IDeviceTypeLogic _deviceTypeLogic;

        private readonly string _iotHubName = string.Empty;

        public DeviceController(IDeviceLogic deviceLogic, IDeviceTypeLogic deviceTypeLogic,
            IConfigurationProvider configProvider,
            IExternalCellularService cellularService,
            IApiRegistrationRepository apiRegistrationRepository)
        {
            _deviceLogic = deviceLogic;
            _deviceTypeLogic = deviceTypeLogic;
            _cellularService = cellularService;
            _apiRegistrationRepository = apiRegistrationRepository;

            _iotHubName = configProvider.GetConfigurationSettingValue("iotHub.HostName");
        }

        [RequirePermission(Permission.ViewDevices)]
        public ActionResult Index()
        {
            return View();
        }

        [RequirePermission(Permission.AddDevices)]
        public async Task<ActionResult> AddDevice()
        {
            var deviceTypes = await _deviceTypeLogic.GetAllDeviceTypesAsync();
            return View(deviceTypes);
        }

        [RequirePermission(Permission.AddDevices)]
        public async Task<ActionResult> SelectType(DeviceType deviceType)
        {
            if (_apiRegistrationRepository.IsApiRegisteredInAzure())
            {
                try
                {
                    var devices = await GetDevices();
                    ViewBag.AvailableIccids = _cellularService.GetListOfAvailableIccids(devices);
                    ViewBag.CanHaveIccid = true;
                }
                catch (CellularConnectivityException)
                {
                    ViewBag.CanHaveIccid = false;
                }
            }
            else
            {
                ViewBag.CanHaveIccid = false;
            }

            // device type logic getdevicetypeasync
            var device = new UnregisteredDeviceModel
            {
                DeviceType = deviceType,
                IsDeviceIdSystemGenerated = true
            };
            return PartialView("_AddDeviceCreate", device);
        }

        [HttpPost]
        [RequirePermission(Permission.AddDevices)]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> AddDeviceCreate(string button, UnregisteredDeviceModel model)
        {
            bool isModelValid = ModelState.IsValid;
            bool onlyValidating = (button != null && button.ToLower().Trim() == "check");

            if (ReferenceEquals(null, model) ||
                (model.GetType() == typeof (object)))
            {
                model = new UnregisteredDeviceModel();
            }

            if (_apiRegistrationRepository.IsApiRegisteredInAzure())
            {
                try
                {
                    var devices = await GetDevices();
                    ViewBag.AvailableIccids = _cellularService.GetListOfAvailableIccids(devices);
                    ViewBag.CanHaveIccid = true;
                }
                catch (CellularConnectivityException)
                {
                    ViewBag.CanHaveIccid = false;
                }
            }
            else
            {
                ViewBag.CanHaveIccid = false;
            }

            //reset flag
            model.IsDeviceIdUnique = false;

            if (model.IsDeviceIdSystemGenerated)
            {
                //clear the model state of errors prior to modifying the model
                ModelState.Clear();

                //assign a system generated device Id
                model.DeviceId = Guid.NewGuid().ToString();

                //validate the model
                isModelValid = TryValidateModel(model);
            }

            if (isModelValid)
            {
                bool deviceExists = await GetDeviceExistsAsync(model.DeviceId);

                model.IsDeviceIdUnique = !deviceExists;

                if (model.IsDeviceIdUnique)
                {
                    if (!onlyValidating)
                    {
                        return await Add(model);
                    }
                }
                else
                {
                    ModelState.AddModelError("DeviceId", Strings.DeviceIdInUse);
                }
            }

            return PartialView("_AddDeviceCreate", model);
        }

        [HttpPost]
        [RequirePermission(Permission.AddDevices)]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> EditDeviceProperties(EditDevicePropertiesModel model)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    return await Edit(model);
                }
                catch (ValidationException exception)
                {
                    if (exception.Errors != null && exception.Errors.Any())
                    {
                        exception.Errors.ToList<string>().ForEach(error => ModelState.AddModelError(string.Empty, error));
                    }
                }
                catch (Exception)
                {
                    ModelState.AddModelError(string.Empty, Strings.DeviceUpdateError);
                }
            }

            return View("EditDeviceProperties", model);
        }

        private async Task<ActionResult> Add(UnregisteredDeviceModel model)
        {
            Debug.Assert(model != null, "model is a null reference.");
            Debug.Assert(
                model.DeviceType != null,
                "model.DeviceType is a null reference.");

            dynamic deviceWithKeys = await AddDeviceAsync(model);

            var newDevice = new RegisteredDeviceModel
            {
                HostName = _iotHubName,
                DeviceType = model.DeviceType,
                DeviceId = DeviceSchemaHelper.GetDeviceID(deviceWithKeys.Device),
                PrimaryKey = deviceWithKeys.SecurityKeys.PrimaryKey,
                SecondaryKey = deviceWithKeys.SecurityKeys.SecondaryKey,
                InstructionsUrl = model.DeviceType.InstructionsUrl
            };

            return PartialView("_AddDeviceCopy", newDevice);
        }

        [RequirePermission(Permission.EditDeviceMetadata)]
        public async Task<ActionResult> EditDeviceProperties(string deviceId)
        {
            EditDevicePropertiesModel model;
            IEnumerable<DevicePropertyValueModel> propValModels;

            model = new EditDevicePropertiesModel
            {
                DevicePropertyValueModels = new List<DevicePropertyValueModel>()
            };

            var device = await _deviceLogic.GetDeviceAsync(deviceId);
            if (!object.ReferenceEquals(device, null))
            {
                model.DeviceId = DeviceSchemaHelper.GetDeviceID(device);

                propValModels = _deviceLogic.ExtractDevicePropertyValuesModels(device);
                propValModels = ApplyDevicePropertyOrdering(propValModels);

                model.DevicePropertyValueModels.AddRange(propValModels);
            }

            return View("EditDeviceProperties", model);
        }

        private async Task<ActionResult> Edit(EditDevicePropertiesModel model)
        {
            if (model != null)
            {
                dynamic device = await _deviceLogic.GetDeviceAsync(model.DeviceId);
                if (!object.ReferenceEquals(device, null))
                {
                    _deviceLogic.ApplyDevicePropertyValueModels(
                        device,
                        model.DevicePropertyValueModels);

                    await _deviceLogic.UpdateDeviceAsync(device);
                }
            }

            return RedirectToAction("Index");
        }

        [RequirePermission(Permission.ViewDevices)]
        public async Task<ActionResult> GetDeviceDetails(string deviceId)
        {
            IEnumerable<DevicePropertyValueModel> propModels;

            dynamic device = await _deviceLogic.GetDeviceAsync(deviceId);
            if (object.ReferenceEquals(device, null))
            {
                throw new InvalidOperationException("Unable to load device with deviceId " + deviceId);
            }

            DeviceDetailModel deviceModel = new DeviceDetailModel
            {
                DeviceID = deviceId,
                HubEnabledState = DeviceSchemaHelper.GetHubEnabledState(device),
                DevicePropertyValueModels = new List<DevicePropertyValueModel>()
            };

            propModels = _deviceLogic.ExtractDevicePropertyValuesModels(device);
            propModels = ApplyDevicePropertyOrdering(propModels);

            deviceModel.DevicePropertyValueModels.AddRange(propModels);

            // check if value is cellular by checking iccid property
            deviceModel.IsCellular = device.SystemProperties.ICCID != null;
            deviceModel.Iccid = device.SystemProperties.ICCID; // todo: try get rid of null checks

            return PartialView("_DeviceDetails", deviceModel);
        }

        [RequirePermission(Permission.ViewDevices)]
        public ActionResult GetDeviceCellularDetails(string iccid)
        {
            var viewModel = new SimInformationViewModel();
            viewModel.TerminalDevice = _cellularService.GetSingleTerminalDetails(new Iccid(iccid));
            viewModel.SessionInfo = _cellularService.GetSingleSessionInfo(new Iccid(iccid)).LastOrDefault() ??
                                    new SessionInfo();

            return PartialView("_CellularInformation", viewModel);
        }

        [RequirePermission(Permission.ViewDeviceSecurityKeys)]
        public async Task<ActionResult> GetDeviceKeys(string deviceId)
        {
            var keys = await _deviceLogic.GetIoTHubKeysAsync(deviceId);

            var keysModel = new SecurityKeysModel
            {
                PrimaryKey = keys != null ? keys.PrimaryKey : Strings.DeviceNotRegisteredInIoTHub,
                SecondaryKey = keys != null ? keys.SecondaryKey : Strings.DeviceNotRegisteredInIoTHub
            };

            return PartialView("_DeviceDetailsKeys", keysModel);
        }


        [RequirePermission(Permission.RemoveDevices)]
        public ActionResult RemoveDevice(string deviceId)
        {
            var device = new RegisteredDeviceModel
            {
                HostName = _iotHubName,
                DeviceId = deviceId
            };

            return View("RemoveDevice", device);
        }

        [HttpPost]
        [RequirePermission(Permission.RemoveDevices)]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> DeleteDevice(string deviceId)
        {
            await _deviceLogic.RemoveDeviceAsync(deviceId);
            return View("Index");
        }

        private static IEnumerable<DevicePropertyValueModel> ApplyDevicePropertyOrdering(
            IEnumerable<DevicePropertyValueModel> devicePropertyModels)
        {
            Debug.Assert(
                devicePropertyModels != null,
                "devicePropertyModels is a null reference.");

            return devicePropertyModels.OrderByDescending(
                t => DeviceDisplayHelper.GetIsCopyControlPropertyName(
                    t.Name)).ThenBy(u => u.DisplayOrder).ThenBy(
                        v => v.Name);
        }

        private async Task<dynamic> AddDeviceAsync(UnregisteredDeviceModel unregisteredDeviceModel)
        {
            dynamic device;

            Debug.Assert(
                unregisteredDeviceModel != null,
                "unregisteredDeviceModel is a null reference.");

            Debug.Assert(
                unregisteredDeviceModel.DeviceType != null,
                "unregisteredDeviceModel.DeviceType is a null reference.");

	        device = DeviceSchemaHelper.BuildDeviceStructure(unregisteredDeviceModel.DeviceId,
                unregisteredDeviceModel.DeviceType.IsSimulatedDevice, unregisteredDeviceModel.Iccid);

            return await this._deviceLogic.AddDeviceAsync(device);
        }

        private async Task<bool> GetDeviceExistsAsync(string deviceId)
        {
            dynamic existingDevice;

            existingDevice = await _deviceLogic.GetDeviceAsync(deviceId);

            return !object.ReferenceEquals(existingDevice, null);
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
