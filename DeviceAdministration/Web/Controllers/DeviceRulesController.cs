using System.Collections.Generic;
using System.Globalization;
using System.Threading.Tasks;
using System.Web.Mvc;
using GlobalResources;
using Microsoft.Azure.Devices.Applications.RemoteMonitoring.Common.Models;
using Microsoft.Azure.Devices.Applications.RemoteMonitoring.DeviceAdmin.Infrastructure.BusinessLogic;
using Microsoft.Azure.Devices.Applications.RemoteMonitoring.DeviceAdmin.Infrastructure.Models;
using Microsoft.Azure.Devices.Applications.RemoteMonitoring.DeviceAdmin.Web.Models;
using Microsoft.Azure.Devices.Applications.RemoteMonitoring.DeviceAdmin.Web.Security;
using Newtonsoft.Json;

namespace Microsoft.Azure.Devices.Applications.RemoteMonitoring.DeviceAdmin.Web.Controllers
{
    public class DeviceRulesController : Controller
    {
        private readonly IDeviceRulesLogic _deviceRulesLogic;

        public DeviceRulesController(IDeviceRulesLogic deviceRulesLogic)
        {
            this._deviceRulesLogic = deviceRulesLogic;
        }

        [RequirePermission(Permission.ViewRules)]
        public ActionResult Index()
        {
            return View();
        }

        /// <summary>
        /// Return a view for the right panel on the DeviceRules index screen
        /// </summary>
        /// <param name="deviceId"></param>
        /// <param name="ruleId"></param>
        /// <returns></returns>
        [HttpGet]
        [RequirePermission(Permission.ViewRules)]
        public async Task<ActionResult> GetRuleProperties(string deviceId, string ruleId)
        {
            DeviceRule rule = await _deviceRulesLogic.GetDeviceRuleAsync(deviceId, ruleId);
            EditDeviceRuleModel editModel = CreateEditModelFromDeviceRule(rule);
            return PartialView("_DeviceRuleProperties", editModel);
        }

        /// <summary>
        /// Update rule properties. This method returns json with either a success = true or an error = errorMessage
        /// After calling this method the user should update the UI by explicitly retrieving fresh data in
        /// a subsequent call
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        [HttpPost]
        [RequirePermission(Permission.EditRules)]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> UpdateRuleProperties(EditDeviceRuleModel model)
        {
            string errorMessage = model.CheckForErrorMessage();
            if(!string.IsNullOrWhiteSpace(errorMessage))
            {
                return Json(new { error = errorMessage });
            }

            DeviceRule rule = CreateDeviceRuleFromEditModel(model);
            TableStorageResponse<DeviceRule> response = await _deviceRulesLogic.SaveDeviceRuleAsync(rule);

            return BuildRuleUpdateResponse(response);
        }

        /// <summary>
        /// Get a new rule with bare-bones data to be used in creating a new rule for a device
        /// </summary>
        /// <param name="deviceId"></param>
        /// <returns></returns>
        [HttpGet]
        [RequirePermission(Permission.EditRules)]
        public async Task<ActionResult> GetNewRule(string deviceId)
        {
            DeviceRule rule = await _deviceRulesLogic.GetNewRuleAsync(deviceId);
            return Json(rule);
        }

        /// <summary>
        /// Update the enabled state for a rule. No other properties will be updated on the rule, 
        /// even if they are included in the device rule model. This method return json with either 
        /// success = true or error = errorMessage. If the user wants to update the ui with fresh
        /// data subsequent explicit calls should be made for new data
        /// </summary>
        /// <param name="ruleModel"></param>
        /// <returns></returns>
        [HttpPost]
        [RequirePermission(Permission.EditRules)]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> UpdateRuleEnabledState(EditDeviceRuleModel ruleModel)
        {
            TableStorageResponse<DeviceRule> response = await _deviceRulesLogic.UpdateDeviceRuleEnabledStateAsync(
                ruleModel.DeviceID, 
                ruleModel.RuleId, 
                ruleModel.EnabledState);

            return BuildRuleUpdateResponse(response);
        }

        /// <summary>
        /// Delete the given rule for a device
        /// </summary>
        /// <param name="ruleModel"></param>
        /// <returns></returns>
        [HttpDelete]
        [RequirePermission(Permission.DeleteRules)]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> DeleteDeviceRule(string deviceId, string ruleId)
        {
            TableStorageResponse<DeviceRule> response = await _deviceRulesLogic.DeleteDeviceRuleAsync(deviceId, ruleId);

            return BuildRuleUpdateResponse(response);
        }

        private JsonResult BuildRuleUpdateResponse(TableStorageResponse<DeviceRule> response)
        {
            switch (response.Status)
            {
                case TableStorageResponseStatus.Successful:
                    return Json(new 
                    { 
                        success = true
                    });
                case TableStorageResponseStatus.ConflictError:
                    return Json(new
                    {
                        error = Strings.TableDataSaveConflictErrorMessage,
                        entity = JsonConvert.SerializeObject(response.Entity)
                    });
                case TableStorageResponseStatus.DuplicateInsert:
                    return Json(new
                    {
                        error = Strings.RuleAlreadyAddedError,
                        entity = JsonConvert.SerializeObject(response.Entity)
                    });
                case TableStorageResponseStatus.NotFound:
                    return Json(new
                    {
                        error = Strings.UnableToRetrieveRuleFromService,
                        entity = JsonConvert.SerializeObject(response.Entity)
                    });
                case TableStorageResponseStatus.UnknownError:
                default:
                    return Json(new
                    {
                        error = Strings.RuleUpdateError,
                        entity = JsonConvert.SerializeObject(response.Entity)
                    });
            }
        }

        /// <summary>
        /// Navigate to the EditRuleProperties screen
        /// </summary>
        /// <param name="deviceId"></param>
        /// <param name="ruleId"></param>
        /// <returns></returns>
        [RequirePermission(Permission.EditRules)]
        public async Task<ActionResult> EditRuleProperties(string deviceId, string ruleId)
        {
            EditDeviceRuleModel editModel = null;
            //empty ruleId implies that we are creating new
            if (string.IsNullOrWhiteSpace(ruleId))
            {
                bool canCreate = await _deviceRulesLogic.CanNewRuleBeCreatedForDeviceAsync(deviceId);
                if (!canCreate)
                {
                    editModel = new EditDeviceRuleModel()
                    {
                        DeviceID = deviceId
                    };
                    return View("AllRulesAssigned", editModel);
                }
            }

            DeviceRule ruleModel = await _deviceRulesLogic.GetDeviceRuleOrDefaultAsync(deviceId, ruleId);
            Dictionary<string, List<string>> availableFields = await _deviceRulesLogic.GetAvailableFieldsForDeviceRuleAsync(ruleModel.DeviceID, ruleModel.RuleId);

            List<SelectListItem> availableDataFields = this.ConvertStringListToSelectList(availableFields["availableDataFields"]);
            List<SelectListItem> availableOperators = this.ConvertStringListToSelectList(availableFields["availableOperators"]);
            List<SelectListItem> availableRuleOutputs = this.ConvertStringListToSelectList(availableFields["availableRuleOutputs"]);

            editModel = CreateEditModelFromDeviceRule(ruleModel);
            editModel.AvailableDataFields = availableDataFields;
            editModel.AvailableOperators = availableOperators;
            editModel.AvailableRuleOutputs = availableRuleOutputs;

            return View("EditDeviceRuleProperties", editModel);
        }

        /// <summary>
        /// Navigate to the DeleteRuleProperties screen
        /// </summary>
        /// <param name="deviceId"></param>
        /// <param name="ruleId"></param>
        /// <returns></returns>
        [RequirePermission(Permission.DeleteRules)]
        public async Task<ActionResult> RemoveRule(string deviceId, string ruleId)
        {
            DeviceRule ruleModel = await _deviceRulesLogic.GetDeviceRuleOrDefaultAsync(deviceId, ruleId);
            EditDeviceRuleModel editModel = CreateEditModelFromDeviceRule(ruleModel);
            return View("RemoveDeviceRule", editModel);
        }

        private DeviceRule CreateDeviceRuleFromEditModel(EditDeviceRuleModel editModel)
        {
            DeviceRule rule = new DeviceRule(editModel.RuleId);
            rule.DataField = editModel.DataField;
            rule.DeviceID = editModel.DeviceID;
            rule.EnabledState = editModel.EnabledState;
            rule.Etag = editModel.Etag;
            rule.Operator = editModel.Operator;
            rule.RuleOutput = editModel.RuleOutput;
            if (!string.IsNullOrWhiteSpace(editModel.Threshold))
            {
                rule.Threshold =
                    double.Parse(
                        editModel.Threshold,
                        NumberStyles.Float,
                        CultureInfo.CurrentCulture);
            }

            return rule;
        }

        private EditDeviceRuleModel CreateEditModelFromDeviceRule(DeviceRule rule)
        {
            EditDeviceRuleModel editModel = new EditDeviceRuleModel();
            editModel.RuleId = rule.RuleId;
            editModel.DataField = rule.DataField;
            editModel.DeviceID = rule.DeviceID;
            editModel.EnabledState = rule.EnabledState;
            editModel.Etag = rule.Etag;
            editModel.Operator = rule.Operator;
            editModel.RuleOutput = rule.RuleOutput;
            if (rule.Threshold != null)
            {
                editModel.Threshold = rule.Threshold.ToString();
            }

            return editModel;
        }
        private List<SelectListItem> ConvertStringListToSelectList(List<string> stringList)
        {
            List<SelectListItem> result = new List<SelectListItem>();
            foreach (string item in stringList)
            {
                SelectListItem selectItem = new SelectListItem();
                selectItem.Value = item;
                selectItem.Text = item;
                result.Add(selectItem);
            }

            return result;
        }
    }
}