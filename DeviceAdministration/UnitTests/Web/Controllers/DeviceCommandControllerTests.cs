using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web.Mvc;
using Microsoft.Azure.Devices.Applications.RemoteMonitoring.Common.Models;
using Microsoft.Azure.Devices.Applications.RemoteMonitoring.Common.Models.Commands;
using Microsoft.Azure.Devices.Applications.RemoteMonitoring.DeviceAdmin.Infrastructure.BusinessLogic;
using Microsoft.Azure.Devices.Applications.RemoteMonitoring.DeviceAdmin.Web.Controllers;
using Microsoft.Azure.Devices.Applications.RemoteMonitoring.DeviceAdmin.Web.Models;
using Moq;
using Newtonsoft.Json;
using Ploeh.AutoFixture;
using Xunit;

namespace Microsoft.Azure.Devices.Applications.RemoteMonitoring.UnitTests.Web
{
    public class DeviceCommandControllerTests
    {
        private readonly Mock<ICommandParameterTypeLogic> _commandParamLogicMock;
        private readonly DeviceCommandController _deviceCommandController;
        private readonly Mock<IDeviceLogic> _deviceLogicMock;

        private readonly Fixture fixture;

        public DeviceCommandControllerTests()
        {
            _commandParamLogicMock = new Mock<ICommandParameterTypeLogic>();
            _deviceLogicMock = new Mock<IDeviceLogic>();
            _deviceCommandController = new DeviceCommandController(_deviceLogicMock.Object, _commandParamLogicMock.Object);
            fixture = new Fixture();
        }

        [Fact]
        public async void IndexTest()
        {
            var deviceId = fixture.Create<string>();
            var device = fixture.Create<DeviceModel>();
            device.DeviceProperties.HubEnabledState = false;
            device.Commands = fixture.Create<List<Command>>();
            _deviceLogicMock.Setup(mock => mock.GetDeviceAsync(deviceId)).ReturnsAsync(device);

            var result = await _deviceCommandController.Index(deviceId);

            var view = result as ViewResult;
            var model = view.Model as DeviceCommandModel;
            Assert.Equal(model.CommandHistory, device.CommandHistory);
            Assert.Equal(model.CommandsJson, JsonConvert.SerializeObject(device.Commands));
            Assert.Equal(model.CommandHistory, device.CommandHistory);
            Assert.Equal(model.SendCommandModel.DeviceId, device.DeviceProperties.DeviceID);
            Assert.Equal(model.SendCommandModel.CommandSelectList.Count, device.Commands.Count);
            Assert.False(model.SendCommandModel.CanSendDeviceCommands);
            Assert.Equal(model.DeviceId, device.DeviceProperties.DeviceID);
        }

        [Fact]
        public void CommandTest()
        {
            var deviceId = fixture.Create<string>();
            var command = fixture.Create<Command>();

            var result = _deviceCommandController.Command(deviceId, command);

            var view = result as PartialViewResult;
            var model = view.Model as CommandModel;
            Assert.Equal(model.DeviceId, deviceId);
            Assert.Equal(model.Name, command.Name);
            Assert.Equal(model.Parameters.Count, command.Parameters.Count);
        }

        [Fact]
        public async void SendCommandTest()
        {
            var parameters = fixture.Create<object>();
            var commandModel = fixture.Create<CommandModel>();
            _commandParamLogicMock
                .Setup(mock => mock.Get(It.IsAny<string>(), It.IsAny<object>()))
                .Returns(parameters);

            _deviceLogicMock
                .Setup(mock => mock.SendCommandAsync(commandModel.DeviceId, commandModel.Name, It.IsAny<IDictionary<string, object>>()))
                .Returns(Task.FromResult(true)).Verifiable();

            var result = await _deviceCommandController.SendCommand(commandModel);
            var view = result as JsonResult;

            Assert.NotNull(view);
            _commandParamLogicMock.Verify(mock => mock.Get(commandModel.Parameters.First().Type, commandModel.Parameters.First().Value));
            _deviceLogicMock.Verify();
        }

        [Fact]
        public async void ResendCommandTest()
        {
            var deviceId = fixture.Create<string>();
            var name = fixture.Create<string>();
            var commandJson = fixture.Create<IDictionary<string, object>>();
            _deviceLogicMock
                .Setup(mock => mock.SendCommandAsync(deviceId, name, It.IsAny<IDictionary<string, object>>()))
                .Returns(Task.FromResult(true));

            var result = await _deviceCommandController.ResendCommand(deviceId, name, JsonConvert.SerializeObject(commandJson));

            var view = result as JsonResult;
            var data = JsonConvert.SerializeObject(view.Data);
            var obj = JsonConvert.SerializeObject(new {wasSent = true});
            Assert.Equal(data, obj);

            _deviceLogicMock
                .Setup(mock => mock.SendCommandAsync(deviceId, name, It.IsAny<IDictionary<string, object>>()))
                .Throws(new Exception());

            result = await _deviceCommandController.ResendCommand(deviceId, name, JsonConvert.SerializeObject(commandJson));

            view = result as JsonResult;
            data = JsonConvert.SerializeObject(view.Data);
            obj = JsonConvert.SerializeObject(new {error = "Failed to send device"});
            Assert.Equal(data, obj);
        }
    }
}