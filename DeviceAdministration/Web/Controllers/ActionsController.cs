using System.Collections.Generic;
using System.Threading.Tasks;
using System.Web.Mvc;
using Microsoft.Azure.Devices.Applications.RemoteMonitoring.DeviceAdmin.Infrastructure.BusinessLogic;
using Microsoft.Azure.Devices.Applications.RemoteMonitoring.DeviceAdmin.Infrastructure.Models;
using Microsoft.Azure.Devices.Applications.RemoteMonitoring.DeviceAdmin.Web.Models;
using Microsoft.Azure.Devices.Applications.RemoteMonitoring.DeviceAdmin.Web.Security;

namespace Microsoft.Azure.Devices.Applications.RemoteMonitoring.DeviceAdmin.Web.Controllers
{
    public class ActionsController : Controller
    {
        private readonly IActionMappingLogic _actionMappingLogic;
        private readonly IActionLogic _actionLogic;

        public ActionsController(IActionMappingLogic actionMappingLogic, IActionLogic actionLogic)
        {
            _actionMappingLogic = actionMappingLogic;
            _actionLogic = actionLogic;
        }

        [RequirePermission(Permission.ViewActions)]
        public ActionResult Index()
        {
            var model = new ActionPropertiesModel();
            return View(model);
        }

        [HttpGet]
        [RequirePermission(Permission.AssignAction)]
        public async Task<ActionResult> GetAvailableLogicAppActions()
        {
            List<SelectListItem> actionIds = await ActionListItems();

            var actionPropertiesModel = new ActionPropertiesModel
            {
                UpdateActionModel = new UpdateActionModel
                {
                    ActionSelectList = actionIds,
                },
            };

            return PartialView("_ActionProperties", actionPropertiesModel);
        }


        [HttpPost]
        [ValidateAntiForgeryToken]
        [RequirePermission(Permission.AssignAction)]
        public async Task<ActionMapping> UpdateAction(string ruleOutput, string actionId)
        {
            var actionMapping = new ActionMapping();

            actionMapping.RuleOutput = ruleOutput;
            actionMapping.ActionId = actionId;
            await _actionMappingLogic.SaveMappingAsync(actionMapping);
            return actionMapping;
        }

        private async Task<List<SelectListItem>> ActionListItems()
        {
            List<string> actionIds = await _actionLogic.GetAllActionIdsAsync();
            if (actionIds != null)
            {
                var actionListItems = new List<SelectListItem>();
                foreach(string actionId in actionIds)
                {
                    var item = new SelectListItem();
                    item.Value = actionId;
                    item.Text = actionId;
                    actionListItems.Add(item);
                }
                return actionListItems;
            }

            return new List<SelectListItem>();
        }
    }
}