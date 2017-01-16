using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web.Mvc;
using Microsoft.Azure.Devices.Applications.RemoteMonitoring.Common.Exceptions;
using Microsoft.Azure.Devices.Applications.RemoteMonitoring.Common.Models;
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
            DeviceModel device = await _deviceLogic.GetDeviceAsync(deviceId);
            if (device.DeviceProperties == null)
            {
                throw new DeviceRequiredPropertyNotFoundException("'DeviceProperties' property is missing");
            }
           
            IList<SelectListItem> commandListItems = CommandListItems(device);

            bool deviceIsEnabled = device.DeviceProperties.GetHubEnabledState();

            DeviceCommandModel deviceCommandsModel = new DeviceCommandModel
            {
                CommandHistory = device.CommandHistory.Where(c => c.DeliveryType == DeliveryType.Message).ToList(),
                CommandsJson = JsonConvert.SerializeObject(device.Commands.Where(c => c.DeliveryType == DeliveryType.Message)),
                SendCommandModel = new SendCommandModel
                {
                    DeviceId = device.DeviceProperties.DeviceID,
                    CommandSelectList = commandListItems,
                    CanSendDeviceCommands = deviceIsEnabled && PermsChecker.HasPermission(Permission.SendCommandToDevices)
                },
                DeviceId = device.DeviceProperties.DeviceID
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
                DeliveryType = command.DeliveryType,
                Parameters = command.Parameters.ToParametersModel().ToList(),
                Description = command.Description
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
                IDictionary<String, Object> parameters = new Dictionary<string, object>();

                if (model.Parameters != null)
                {
                    foreach (var parameter in model.Parameters)
                    {
                        parameters.Add(new KeyValuePair<string, object>(parameter.Name,
                            _commandParameterTypeLogic.Get(parameter.Type, parameter.Value)));
                    }
                }

                await _deviceLogic.SendCommandAsync(model.DeviceId, model.Name, model.DeliveryType, parameters);
 
                return Json(new {data = model});
            }

            return PartialView("_SendCommandForm", model);
        }

        [HttpPost]
        [RequirePermission(Permission.SendCommandToDevices)]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> ResendCommand(string deviceId, string name, DeliveryType deliveryType, string commandJson)
        {
            try
            {
                IDictionary<string, object> commandParameters = JsonConvert.DeserializeObject<Dictionary<string, object>>(commandJson);

                await _deviceLogic.SendCommandAsync(deviceId, name, deliveryType, commandParameters);
            }
            catch
            {
                return Json(new {error = "Failed to send device"});
            }


            return Json(new { wasSent = true });
        }

        private IList<SelectListItem> CommandListItems(DeviceModel device)
        {
            if (device.Commands != null)
            {
                return GetCommandListItems(device);
            }

            return new List<SelectListItem>();
        }


        private IList<SelectListItem> GetCommandListItems(DeviceModel device)
        {
            IList<SelectListItem> result = new List<SelectListItem>();
            IList<Command> commands = device.Commands;

            if (commands != null)
            {
                foreach (Command command in commands)
                {
                    if (IsCommandPublic(command) && command.DeliveryType == DeliveryType.Message)
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

        private static bool IsCommandPublic(Command command)
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