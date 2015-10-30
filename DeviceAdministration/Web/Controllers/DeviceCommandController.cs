using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web.Mvc;
using Microsoft.Azure.Devices.Applications.RemoteMonitoring.Common.DeviceSchema;
using Microsoft.Azure.Devices.Applications.RemoteMonitoring.Common.Exceptions;
using Microsoft.Azure.Devices.Applications.RemoteMonitoring.Common.Helpers;
using Microsoft.Azure.Devices.Applications.RemoteMonitoring.Common.Models.Commands;
using Microsoft.Azure.Devices.Applications.RemoteMonitoring.DeviceAdmin.Infrastructure.BusinessLogic;
using Microsoft.Azure.Devices.Applications.RemoteMonitoring.DeviceAdmin.Web.Models;
using Microsoft.Azure.Devices.Applications.RemoteMonitoring.DeviceAdmin.Web.Security;
using Newtonsoft.Json;

namespace Microsoft.Azure.Devices.Applications.RemoteMonitoring.DeviceAdmin.Web.Controllers
{
    [Authorize]
    [OutputCache(CacheProfile = "NoCacheProfile")]
    public class DeviceCommandController : Controller
    {
        private static readonly List<string> PrivateCommands = new List<string>
        {
            // currently there are no commands which need to be hidden in the
            // generic command view (like change device key, etc)
        };

        private readonly IDeviceLogic _deviceLogic;
        private readonly ICommandParameterTypeLogic _commandParameterTypeLogic;

        public DeviceCommandController(IDeviceLogic deviceLogic, ICommandParameterTypeLogic commandParameterTypeLogic)
        {
            _deviceLogic = deviceLogic;
            _commandParameterTypeLogic = commandParameterTypeLogic;
        }

        [RequirePermission(Permission.ViewDevices)]
        public async Task<ActionResult> Index(string deviceId)
        {
            dynamic device = await _deviceLogic.GetDeviceAsync(deviceId);

            List<SelectListItem> commandListItems = CommandListItems(device);

            bool deviceIsEnabled = DeviceSchemaHelper.GetHubEnabledState(device) == true;
            var deviceCommandsModel = new DeviceCommandModel
            {
                CommandHistory = new List<dynamic>(CommandHistorySchemaHelper.GetCommandHistory(device)),
                CommandsJson = JsonConvert.SerializeObject(device.Commands),
                SendCommandModel = new SendCommandModel
                {
                    DeviceId = DeviceSchemaHelper.GetDeviceID(device),
                    CommandSelectList = commandListItems,
                    CanSendDeviceCommands = deviceIsEnabled &&
                        PermsChecker.HasPermission(Permission.SendCommandToDevices)
                },
                DeviceId = DeviceSchemaHelper.GetDeviceID(device)
            };

            return View(deviceCommandsModel);
        }

        [HttpPost]
        [RequirePermission(Permission.SendCommandToDevices)]
        [ValidateAntiForgeryToken]
        public ActionResult Command(string deviceId, Command command)
        {
            var model = new CommandModel
            {
                DeviceId = deviceId,
                Name = command.Name,
                Parameters = command.Parameters.ToParametersModel().ToList()
            };
            return PartialView("_SendCommandForm", model);
        }

        [HttpPost]
        [RequirePermission(Permission.SendCommandToDevices)]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> SendCommand(CommandModel model)
        {
            if (ModelState.IsValid)
            {
                IDictionary<String, Object> commands = new Dictionary<string, object>();

                if (model.Parameters != null)
                {
                    foreach (var parameter in model.Parameters)
                    {
                        commands.Add(new KeyValuePair<string, object>(parameter.Name,
                            _commandParameterTypeLogic.Get(parameter.Type, parameter.Value)));
                    }
                }

                await _deviceLogic.SendCommandAsync(model.DeviceId, model.Name, commands);

                return Json(new {data = model});
            }

            return PartialView("_SendCommandForm", model);
        }

        [HttpPost]
        [RequirePermission(Permission.SendCommandToDevices)]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> ResendCommand(string deviceId, string name, string commandJson)
        {
            try
            {
                dynamic commandParameters = JsonConvert.DeserializeObject<Dictionary<string, object>>(commandJson);

                await _deviceLogic.SendCommandAsync(deviceId, name, commandParameters);
            }
            catch
            {
                return Json(new {error = "Failed to send device"});
            }


            return Json(new { wasSent = true });
        }

        private List<SelectListItem> CommandListItems(dynamic device)
        {
            if (device.Commands != null)
            {
                return GetCommandListItems(device);
            }

            return new List<SelectListItem>();
        }


        private List<SelectListItem> GetCommandListItems(dynamic device)
        {
            IEnumerable commands;

            List<SelectListItem> result = new List<SelectListItem>();

            commands =
                ReflectionHelper.GetNamedPropertyValue(
                    (object)device,
                    "Commands",
                    true,
                    false) as IEnumerable;

            if (commands != null)
            {
                foreach (dynamic command in commands)
                {
                    if (this.IsCommandPublic(command))
                    {
                        SelectListItem item = new SelectListItem();
                        item.Value = command.Name;
                        item.Text = command.Name;
                        result.Add(item);
                    }
                }
            }

            return result;
        }

        private bool IsCommandPublic(dynamic command)
        {
            if (command == null)
            {
                throw new ArgumentNullException("command");
            }

            if (command.Name == null)
            {
                throw new DeviceRequiredPropertyNotFoundException("'Name' property on command not found");
            }

            return !PrivateCommands.Contains(command.Name.ToString());
        }
    }
}