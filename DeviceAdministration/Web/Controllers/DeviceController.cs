using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using System.Web.Mvc;
using DeviceManagement.Infrustructure.Connectivity.Exceptions;
using DeviceManagement.Infrustructure.Connectivity.Models.TerminalDevice;
using GlobalResources;
using Microsoft.Azure.Devices.Applications.RemoteMonitoring.Common.Configurations;
using Microsoft.Azure.Devices.Applications.RemoteMonitoring.Common.Exceptions;
using Microsoft.Azure.Devices.Applications.RemoteMonitoring.Common.Factory;
using Microsoft.Azure.Devices.Applications.RemoteMonitoring.Common.Helpers;
using Microsoft.Azure.Devices.Applications.RemoteMonitoring.Common.Models;
using Microsoft.Azure.Devices.Applications.RemoteMonitoring.DeviceAdmin.Infrastructure.BusinessLogic;
using Microsoft.Azure.Devices.Applications.RemoteMonitoring.DeviceAdmin.Infrastructure.Exceptions;
using Microsoft.Azure.Devices.Applications.RemoteMonitoring.DeviceAdmin.Infrastructure.Models;
using Microsoft.Azure.Devices.Applications.RemoteMonitoring.DeviceAdmin.Infrastructure.Repository;
using Microsoft.Azure.Devices.Applications.RemoteMonitoring.DeviceAdmin.Web.Helpers;
using Microsoft.Azure.Devices.Applications.RemoteMonitoring.DeviceAdmin.Web.Models;
using Microsoft.Azure.Devices.Applications.RemoteMonitoring.DeviceAdmin.Web.Security;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;

namespace Microsoft.Azure.Devices.Applications.RemoteMonitoring.DeviceAdmin.Web.Controllers
{
    [Authorize]
    [OutputCache(CacheProfile = "NoCacheProfile")]
    public class DeviceController : Controller
    {
        private readonly IDeviceLogic _deviceLogic;
        private readonly IDeviceTypeLogic _deviceTypeLogic;
        private readonly IApiRegistrationRepository _apiRegistrationRepository;
        private readonly ICellularExtensions _cellularExtensions;
        private readonly IJobRepository _jobRepository;
        private readonly IUserSettingsLogic _userSettingsLogic;
        private readonly IIoTHubDeviceManager _deviceManager;
        private readonly IDeviceIconRepository _iconRepository;
        private readonly string _iotHubName;

        public DeviceController(IDeviceLogic deviceLogic,
            IDeviceTypeLogic deviceTypeLogic,
            IConfigurationProvider configProvider,
            IApiRegistrationRepository apiRegistrationRepository,
            ICellularExtensions cellularExtensions,
            IJobRepository jobRepository,
            IUserSettingsLogic userSettingsLogic,
            IIoTHubDeviceManager deviceManager,
            IDeviceIconRepository iconRepository)
        {
            _deviceLogic = deviceLogic;
            _deviceTypeLogic = deviceTypeLogic;
            _apiRegistrationRepository = apiRegistrationRepository;
            _cellularExtensions = cellularExtensions;
            _jobRepository = jobRepository;
            _userSettingsLogic = userSettingsLogic;
            _deviceManager = deviceManager;
            _iconRepository = iconRepository;

            _iotHubName = configProvider.GetConfigurationSettingValue("iotHub.HostName");
        }

        [RequirePermission(Permission.ViewDevices)]
        public async Task<ActionResult> Index(string filterId)
        {
            ViewBag.FilterId = filterId;
            ViewBag.HasManageJobsPerm = PermsChecker.HasPermission(Permission.ManageJobs);
            ViewBag.HasDeleteSuggestedClausePerm = PermsChecker.HasPermission(Permission.DeleteSuggestedClauses);
            ViewBag.IconBaseUrl = await _iconRepository.GetIconStorageUriPrefix();
            ViewBag.IconTagName = Constants.DeviceIconTagName;

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
                    var registrationModel = _apiRegistrationRepository.RecieveDetails();
                    List<DeviceModel> devices = await GetDevices();
                    ViewBag.AvailableIccids = _cellularExtensions.GetListOfAvailableIccids(devices,
                        registrationModel.ApiRegistrationProvider);
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
                (model.GetType() == typeof(object)))
            {
                model = new UnregisteredDeviceModel();
            }

            if (_apiRegistrationRepository.IsApiRegisteredInAzure())
            {
                try
                {
                    var registrationModel = _apiRegistrationRepository.RecieveDetails();
                    List<DeviceModel> devices = await GetDevices();
                    ViewBag.AvailableIccids = _cellularExtensions.GetListOfAvailableIccids(devices,
                        registrationModel.ApiRegistrationProvider);
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
                while (true)
                {
                    var random = BitConverter.ToUInt32(Guid.NewGuid().ToByteArray(), 0);
                    string deviceId = $"CoolingSampleDevice_{random % 10000:d4}";

                    if (!await GetDeviceExistsAsync(deviceId))
                    {
                        model.DeviceId = deviceId;
                        break;
                    }
                }

                //validate the model
                isModelValid = TryValidateModel(model);
            }

            if (isModelValid)
            {
                bool deviceExists = !model.IsDeviceIdSystemGenerated && await GetDeviceExistsAsync(model.DeviceId);

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
            var deviceWithKeys = await AddDeviceAsync(model);
            var newDevice = new RegisteredDeviceModel
            {
                HostName = _iotHubName,
                DeviceType = model.DeviceType,
                DeviceId = deviceWithKeys.Device.DeviceProperties.DeviceID,
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
            if (device != null)
            {
                if (device.DeviceProperties == null)
                {
                    throw new DeviceRequiredPropertyNotFoundException("Required DeviceProperties not found");
                }

                model.DeviceId = device.DeviceProperties.DeviceID;
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
                var device = await _deviceLogic.GetDeviceAsync(model.DeviceId);
                if (device != null)
                {
                    _deviceLogic.ApplyDevicePropertyValueModels(device, model.DevicePropertyValueModels);
                    await _deviceLogic.UpdateDeviceAsync(device);
                }
            }

            return RedirectToAction("Index");
        }

        [RequirePermission(Permission.ViewDevices)]
        public async Task<ActionResult> GetDeviceDetails(string deviceId)
        {
            IEnumerable<DevicePropertyValueModel> propModels;

            var device = await _deviceLogic.GetDeviceAsync(deviceId);
            if (device == null)
            {
                throw new InvalidOperationException("Unable to load device with deviceId " + deviceId);
            }

            if (device.DeviceProperties == null)
            {
                throw new DeviceRequiredPropertyNotFoundException("'DeviceProperties' property is missing");
            }

            DeviceDetailModel deviceModel = new DeviceDetailModel
            {
                DeviceID = deviceId,
                HubEnabledState = device.DeviceProperties.GetHubEnabledState(),
                DevicePropertyValueModels = new List<DevicePropertyValueModel>(),
                IsSimulatedDevice = device.IsSimulatedDevice
            };

            propModels = _deviceLogic.ExtractDevicePropertyValuesModels(device);
            propModels = ApplyDevicePropertyOrdering(propModels);

            deviceModel.DevicePropertyValueModels.AddRange(propModels);

            // check if value is cellular by checking iccid property
            deviceModel.IsCellular = device.SystemProperties.ICCID != null;
            deviceModel.Iccid = device.SystemProperties.ICCID; // todo: try get rid of null checks

            ViewBag.IconBaseUrl = await _iconRepository.GetIconStorageUriPrefix();

            return PartialView("_DeviceDetails", deviceModel);
        }

        [RequirePermission(Permission.ViewDevices)]
        public ActionResult GetDeviceCellularDetails(string iccid)
        {
            var viewModel = generateSimInformationViewModel(iccid);
            return PartialView("_CellularInformation", viewModel);
        }

        [RequirePermission(Permission.ViewDevices)]
        [HttpPost]
        public async Task<ActionResult> CellularActionRequest(CellularActionRequestModel model)
        {
            if (model == null) throw new ArgumentException(nameof(model));
            if (string.IsNullOrWhiteSpace(model.DeviceId)) throw new ArgumentException(nameof(model.DeviceId));

            var device = await _deviceLogic.GetDeviceAsync(model.DeviceId);
            if (device == null) throw new InvalidOperationException("Unable to find device with deviceId " + model.DeviceId);
            var iccid = device.SystemProperties.ICCID;
            if (string.IsNullOrWhiteSpace(iccid)) throw new InvalidOperationException("Device does not have an ICCID. Cannot complete cellular actions.");

            CellularActionUpdateResponseModel result = await processActionRequests(device, model.CellularActions);
            var viewModel = generateSimInformationViewModel(iccid, result);
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
            return null;
        }

        [RequirePermission(Permission.ViewDevices)]
        public async Task<ActionResult> GetDeviceListColumns()
        {
            var userId = PrincipalHelper.GetEmailAddress(User);
            var columns = await _userSettingsLogic.GetDeviceListColumnsAsync(userId);

            return PartialView("_DeviceListColumns", JsonConvert.SerializeObject(columns, new JsonSerializerSettings
            {
                ContractResolver = new CamelCasePropertyNamesContractResolver()
            }));
        }

        [RequirePermission(Permission.EditDeviceMetadata)]
        public ActionResult EditDesiredProperties(string deviceId)
        {
            return View(new EditDevicePropertiesModel() { DeviceId = deviceId });
        }

        [RequirePermission(Permission.EditDeviceMetadata)]
        public ActionResult EditTags(string deviceId)
        {
            return View(new EditDevicePropertiesModel() { DeviceId = deviceId });
        }

        [RequirePermission(Permission.EditDeviceMetadata)]
        public async Task<ActionResult> EditIcon(string deviceId)
        {
            var device = await _deviceLogic.GetDeviceAsync(deviceId);
            if (device == null)
            {
                throw new InvalidOperationException("Unable to load device with deviceId " + deviceId);
            }

            return View(new EditDevicePropertiesModel() { DeviceId = deviceId,IsSimulatedDevice=device.IsSimulatedDevice });
        }

        [RequirePermission(Permission.ViewDevices)]
        public async Task<FileResult> DownloadTwinJson(string deviceId)
        {
            if (string.IsNullOrWhiteSpace(deviceId))
            {
                throw new ArgumentException(nameof(deviceId));
            }

            var device = await _deviceLogic.GetDeviceAsync(deviceId);
            if (device == null)
            {
                throw new DeviceNotRegisteredException("Unable to find device with deviceId " + deviceId);
            }

            var twinJson = device.Twin.ToJson(Formatting.Indented);
            string suffix = ".json";
            return File(System.Text.Encoding.UTF8.GetBytes(twinJson), "application/json", deviceId + suffix);
        }

        private static IEnumerable<DevicePropertyValueModel> ApplyDevicePropertyOrdering(IEnumerable<DevicePropertyValueModel> devicePropertyModels)
        {
            Debug.Assert(
                devicePropertyModels != null,
                "devicePropertyModels is a null reference.");

            return devicePropertyModels.OrderByDescending(
                t => DeviceDisplayHelper.GetIsCopyControlPropertyName(
                    t.Name)).ThenBy(u => u.DisplayOrder).ThenBy(
                        v => v.Name);
        }

        private async Task<DeviceWithKeys> AddDeviceAsync(UnregisteredDeviceModel unregisteredDeviceModel)
        {
            Debug.Assert(
                unregisteredDeviceModel != null,
                "unregisteredDeviceModel is a null reference.");

            Debug.Assert(
                unregisteredDeviceModel.DeviceType != null,
                "unregisteredDeviceModel.DeviceType is a null reference.");

            DeviceModel device = DeviceCreatorHelper.BuildDeviceStructure(
                unregisteredDeviceModel.DeviceId,
                unregisteredDeviceModel.DeviceType.IsSimulatedDevice,
                unregisteredDeviceModel.Iccid);
            SampleDeviceFactory.AssignDefaultTags(device);
            SampleDeviceFactory.AssignDefaultDesiredProperties(device);

            DeviceWithKeys addedDevice = await this._deviceLogic.AddDeviceAsync(device);
            return addedDevice;
        }

        private async Task<bool> GetDeviceExistsAsync(string deviceId)
        {
            DeviceModel existingDevice = await _deviceLogic.GetDeviceAsync(deviceId);
            return (existingDevice != null);
        }

        private async Task<List<DeviceModel>> GetDevices()
        {
            var filter = new DeviceListFilter
            {
                Take = 1000
            };

            var devices = await _deviceLogic.GetDevices(filter);
            return devices.Results;
        }

        [HttpGet]
        [RequirePermission(Permission.ViewJobs)]
        public async Task<ActionResult> GetDeviceJobs(string deviceId)
        {
            var deviceJobs = await this._deviceManager.GetDeviceJobsByDeviceIdAsync(deviceId);

            var tasks = deviceJobs.Select(async j => new NamedDeviceJob
            {
                Name = await GetDeviceJobName(j),
                Job = j
            });

            return PartialView("_DeviceJobs", await Task.WhenAll(tasks));
        }

        private async Task<string> GetDeviceJobName(DeviceJob job)
        {
            try
            {
                var model = await _jobRepository.QueryByJobIDAsync(job.JobId);
                return model.JobName;
            }
            catch
            {
                return $"[{job.JobId}]";
            }
        }

        private async Task<CellularActionUpdateResponseModel> processActionRequests(DeviceModel device, List<CellularActionModel> actions)
        {
            var iccid = device.SystemProperties.ICCID;
            var terminal = _cellularExtensions.GetSingleTerminalDetails(new Iccid(iccid));
            var completedActions = new List<CellularActionModel>();
            var failedActions = new List<CellularActionModel>();
            var exceptions = new List<ErrorModel>();
            foreach (var action in actions)
            {
                var success = false;
                try
                {
                    switch (action.Type)
                    {
                        case CellularActionType.UpdateStatus:
                            success = _cellularExtensions.UpdateSimState(iccid, action.Value);
                            break;

                        case CellularActionType.UpdateSubscriptionPackage:
                            success = _cellularExtensions.UpdateSubscriptionPackage(iccid, action.Value);
                            break;

                        case CellularActionType.UpdateLocale:
                            success = _cellularExtensions.SetLocale(iccid, action.Value);
                            break;

                        case CellularActionType.ReconnectDevice:
                            success = _cellularExtensions.ReconnectDevice(iccid);
                            break;

                        case CellularActionType.SendSms:
                            success = await _cellularExtensions.SendSms(iccid, action.Value);
                            break;

                        default:
                            failedActions.Add(action);
                            break;
                    }
                }
                catch (Exception exception)
                {
                    success = false;
                    exceptions.Add(new ErrorModel(exception));
                }
                if (!success)
                {
                    failedActions.Add(action);
                }
                else
                {
                    completedActions.Add(action);
                }
            }
            return new CellularActionUpdateResponseModel()
            {
                CompletedActions = completedActions,
                FailedActions = failedActions,
                Exceptions = exceptions
            };
        }

        private SimInformationViewModel generateSimInformationViewModel(string iccid, CellularActionUpdateResponseModel actionResponstModel = null)
        {
            var viewModel = new SimInformationViewModel();
            viewModel.TerminalDevice = _cellularExtensions.GetSingleTerminalDetails(new Iccid(iccid));
            viewModel.SessionInfo = _cellularExtensions.GetSingleSessionInfo(new Iccid(iccid)).LastOrDefault() ?? new SessionInfo();

            var apiProviderDetails = _apiRegistrationRepository.RecieveDetails();
            viewModel.ApiRegistrationProvider = Convert.ToString(apiProviderDetails.ApiRegistrationProvider, CultureInfo.CurrentCulture);
            viewModel.AvailableSimStates = _cellularExtensions.GetValidTargetSimStates(iccid, viewModel.TerminalDevice.Status);

            try
            {
                viewModel.AvailableSubscriptionPackages = _cellularExtensions.GetAvailableSubscriptionPackages(iccid, viewModel.TerminalDevice.RatePlan);
            }
            catch
            {
                // [WORKAROUND] GetAvailableSubscriptionPackages does not work
                viewModel.AvailableSubscriptionPackages = new List<DeviceManagement.Infrustructure.Connectivity.Models.Other.SubscriptionPackage>();
            }

            viewModel.CellularActionUpdateResponse = actionResponstModel;

            try
            {
                IEnumerable<string> availableLocaleNames;
                string currentLocaleName = _cellularExtensions.GetLocale(iccid, out availableLocaleNames);

                viewModel.CurrentLocaleName = currentLocaleName;
                viewModel.AvailableLocaleNames = availableLocaleNames;
            }
            catch
            {
                viewModel.CurrentLocaleName = string.Empty;
                viewModel.AvailableLocaleNames = new List<string>();
            }

            var state = _cellularExtensions.GetLastSetLocaleServiceRequestState(iccid);
            if (string.Equals(state, "IN_PROGRESS") || string.Equals(state, "PENDING"))
            {
                viewModel.LastServiceRequestState = state;
            }

            return viewModel;
        }
    }
}
