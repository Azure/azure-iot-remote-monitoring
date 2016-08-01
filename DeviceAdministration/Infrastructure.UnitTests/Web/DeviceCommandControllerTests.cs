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

namespace Microsoft.Azure.Devices.Applications.RemoteMonitoring.DeviceAdmin.Infrastructure.UnitTests.Web
{
    public class DeviceCommandControllerTests
    {
        private readonly DeviceCommandController deviceCommandController;
        private readonly Mock<ICommandParameterTypeLogic> commandParamLogicMock;
        private readonly Mock<IDeviceLogic> deviceLogicMock;

        private readonly Fixture fixture;

        public DeviceCommandControllerTests()
        {
            this.commandParamLogicMock = new Mock<ICommandParameterTypeLogic>();
            this.deviceLogicMock = new Mock<IDeviceLogic>();
            this.deviceCommandController = new DeviceCommandController(this.deviceLogicMock.Object, this.commandParamLogicMock.Object);
            this.fixture = new Fixture();
        }

        [Fact]
        public async void Index()
        {
            var deviceID = this.fixture.Create<string>();
            var device = this.fixture.Create<DeviceModel>();
            device.DeviceProperties.HubEnabledState = false;
            device.Commands = this.fixture.Create<List<Command>>();
            this.deviceLogicMock.Setup(mock => mock.GetDeviceAsync(deviceID)).ReturnsAsync(device);
            var result = await this.deviceCommandController.Index(deviceID);
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
        public void Command()
        {
            var deviceId = this.fixture.Create<string>();
            var command = this.fixture.Create<Command>();

            var result = this.deviceCommandController.Command(deviceId, command);
            var view = result as PartialViewResult;
            var model = view.Model as CommandModel;
            Assert.Equal(model.DeviceId, deviceId);
            Assert.Equal(model.Name, command.Name);
            Assert.Equal(model.Parameters.Count, command.Parameters.Count);
        }

        [Fact]
        public async void SendCommand()
        {
            var parameters = this.fixture.Create<object>();
            var commandModel = this.fixture.Create<CommandModel>();
            this.commandParamLogicMock.Setup(mock => mock.Get(It.IsAny<string>(), It.IsAny<object>())).Returns(parameters);
            this.deviceLogicMock.Setup(
                                       mock =>
                                       mock.SendCommandAsync(commandModel.DeviceId, commandModel.Name, It.IsAny<IDictionary<String, Object>>()))
                .Returns(Task.FromResult(true)).Verifiable();

            var result = await this.deviceCommandController.SendCommand(commandModel);
            var view = result as JsonResult;
            Assert.NotNull(view);
            this.commandParamLogicMock.Verify(mock => mock.Get(commandModel.Parameters.First().Type, commandModel.Parameters.First().Value));
            this.deviceLogicMock.Verify();
        }

        [Fact]
        public async void ResendCommand()
        {
            var deviceId = this.fixture.Create<string>();
            var name = this.fixture.Create<string>();
            var commandJson = this.fixture.Create<IDictionary<string, object>>();
            this.deviceLogicMock.Setup(mock => mock.SendCommandAsync(deviceId, name, It.IsAny<IDictionary<string, object>>()))
                .Returns(Task.FromResult(true));

            var result = await this.deviceCommandController.ResendCommand(deviceId, name, JsonConvert.SerializeObject(commandJson));
            var view = result as JsonResult;
            var data = JsonConvert.SerializeObject(view.Data);
            var obj = JsonConvert.SerializeObject(new {wasSent = true});
            Assert.Equal(data, obj);

            this.deviceLogicMock.Setup(mock => mock.SendCommandAsync(deviceId, name, It.IsAny<IDictionary<string, object>>()))
                .Throws(new Exception());
            result = await this.deviceCommandController.ResendCommand(deviceId, name, JsonConvert.SerializeObject(commandJson));
            view = result as JsonResult;
            data = JsonConvert.SerializeObject(view.Data);
            obj = JsonConvert.SerializeObject(new {error = "Failed to send device"});
            Assert.Equal(data, obj);
        }
    }
}
