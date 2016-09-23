using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Web.Http;
using System.Web.Mvc;
using DeviceManagement.Infrustructure.Connectivity;
using DeviceManagement.Infrustructure.Connectivity.Exceptions;
using DeviceManagement.Infrustructure.Connectivity.Models.TerminalDevice;
using GlobalResources;
using Microsoft.Azure.Devices.Applications.RemoteMonitoring.Common.Models;
using Microsoft.Azure.Devices.Applications.RemoteMonitoring.DeviceAdmin.Infrastructure.BusinessLogic;
using Microsoft.Azure.Devices.Applications.RemoteMonitoring.DeviceAdmin.Infrastructure.Models;
using Microsoft.Azure.Devices.Applications.RemoteMonitoring.DeviceAdmin.Infrastructure.Repository;
using Microsoft.Azure.Devices.Applications.RemoteMonitoring.DeviceAdmin.Web.Helpers;
using Microsoft.Azure.Devices.Applications.RemoteMonitoring.DeviceAdmin.Web.Security;
using Microsoft.Azure.Devices.Applications.RemoteMonitoring.DeviceAdmin.Web.Extensions;

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
            DeviceAssociationModel model;
            try
            {
                if (_apiRegistrationRepository.IsApiRegisteredInAzure())
                {
                    var registrationModel = _apiRegistrationRepository.RecieveDetails();
                    model = new DeviceAssociationModel()
                    {
                        ApiRegistrationProvider = registrationModel.ApiRegistrationProvider,
                        HasRegistration = true,
                        UnassignedIccidList = _cellularExtensions.GetListOfAvailableIccids(devices),
                        UnassignedDeviceIds = _cellularExtensions.GetListOfAvailableDeviceIDs(devices)
                    };
                }
                else
                {
                    model = new DeviceAssociationModel()
                    {
                        HasRegistration = false
                    };
                }
            }
            catch (CellularConnectivityException)
            {
                model = new DeviceAssociationModel()
                {
                    HasRegistration = false
                };
            }

            return PartialView("_DeviceAssociation", model);
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

        public async Task<bool> SaveRegistration(ApiRegistrationModel apiModel)
        {
            try
            {
                // get the current registration model
                var oldRegistrationDetails = _apiRegistrationRepository.RecieveDetails();
                // ammend the new details
                _apiRegistrationRepository.AmendRegistration(apiModel);

                // check credentials work. If they do not work revert the change.
                if (!CheckCredentials())
                {
                    _apiRegistrationRepository.AmendRegistration(oldRegistrationDetails);
                    return false;
                }

                // if api provider has changed then disassociate all associated devices
                if (oldRegistrationDetails != null && oldRegistrationDetails.ApiRegistrationProvider != apiModel.ApiRegistrationProvider)
                {
                    var disassociateDeviceResult = await DisassociateAllDevices();
                    // if this has failed revert the change
                    if (!disassociateDeviceResult)
                    {
                        _apiRegistrationRepository.AmendRegistration(oldRegistrationDetails);
                        return false;
                    }
                }           
            }
            catch (Exception ex)
            {
                _apiRegistrationRepository.DeleteApiDetails();
                return false;
            }

            return true;
        }

        public async Task<bool> DeleteRegistration()
        {
            var disassociateDeviceResult = await DisassociateAllDevices();
            if (!disassociateDeviceResult)
            {
                return false;
            }
            return _apiRegistrationRepository.DeleteApiDetails();
        }

        public async Task<bool> AddIccids([FromBody]List<Iccid> Iccids)
        {
            return true;
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

        /// <summary>
        /// Disassociates all devices
        /// </summary>
        /// <returns></returns>
        private async Task<bool> DisassociateAllDevices()
        {
            try
            {
                var devices = await GetDevices();
                var connectedDevices = _cellularExtensions.GetListOfConnectedDeviceIds(devices);
                foreach (dynamic deviceId in connectedDevices)
                {
                    await UpdateDeviceAssociation(deviceId, null);
                }
            }
            catch (Exception ex)
            {
                return false;
            }
            return true;
        }

        private bool CheckCredentials()
        {
            var credentialsAreValid = _cellularExtensions.ValidateCredentials();
            if (!credentialsAreValid)
            {
                _apiRegistrationRepository.DeleteApiDetails();
                return false;
            }
            return true;
        }
    }
}