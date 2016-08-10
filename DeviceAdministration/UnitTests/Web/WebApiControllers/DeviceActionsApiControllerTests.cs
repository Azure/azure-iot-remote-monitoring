using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Azure.Devices.Applications.RemoteMonitoring.DeviceAdmin.Infrastructure.BusinessLogic;
using Microsoft.Azure.Devices.Applications.RemoteMonitoring.DeviceAdmin.Infrastructure.Models;
using Microsoft.Azure.Devices.Applications.RemoteMonitoring.DeviceAdmin.Web.DataTables;
using Microsoft.Azure.Devices.Applications.RemoteMonitoring.DeviceAdmin.Web.WebApiControllers;
using Moq;
using Ploeh.AutoFixture;
using Xunit;

namespace Microsoft.Azure.Devices.Applications.RemoteMonitoring.UnitTests.Web.
    WebApiControllers
{
    public class DeviceActionsApiControllerTests
    {
        private readonly Mock<IActionMappingLogic> actionMappingLogic;
        private readonly DeviceActionsApiController deviceActionsApiController;
        private readonly IFixture fixture;

        public DeviceActionsApiControllerTests()
        {
            actionMappingLogic = new Mock<IActionMappingLogic>();
            deviceActionsApiController = new DeviceActionsApiController(actionMappingLogic.Object);
            deviceActionsApiController.InitializeRequest();
            fixture = new Fixture();
        }

        [Fact]
        public async void GetDeviceActionsAsyncTest()
        {
            var actionMappings = fixture.Create<List<ActionMappingExtended>>();
            actionMappingLogic.Setup(mock => mock.GetAllMappingsAsync()).ReturnsAsync(actionMappings);
            var result = await deviceActionsApiController.GetDeviceActionsAsync();
            result.AssertOnError();
            var data = result.ExtractContentDataAs<List<ActionMappingExtended>>();
            Assert.Equal(data, actionMappings);
        }

        [Fact]
        public async void GetDeviceActionsAsDataTablesResponseAsyncTest()
        {
            var actionMappings = fixture.Create<List<ActionMappingExtended>>();
            actionMappingLogic.Setup(mock => mock.GetAllMappingsAsync()).ReturnsAsync(actionMappings);
            var result = await deviceActionsApiController.GetDeviceActionsAsDataTablesResponseAsync();
            result.AssertOnError();
            var data = result.ExtractContentAs<DataTablesResponse<ActionMappingExtended>>();
            Assert.Equal(data.RecordsTotal, actionMappings.Count);
            Assert.Equal(data.RecordsFiltered, actionMappings.Count);
            Assert.Equal(data.Data, actionMappings.ToArray());
        }

        [Fact]
        public async void UpdateActionAsyncTest()
        {
            var ruleOutput = fixture.Create<string>();
            var actionId = fixture.Create<string>();
            ActionMapping saveObject = null;

            actionMappingLogic.Setup(mock => mock.SaveMappingAsync(It.IsAny<ActionMapping>()))
                .Callback<ActionMapping>(obj => saveObject = obj)
                .Returns(Task.FromResult(true)).Verifiable();
            var result = await deviceActionsApiController.UpdateActionAsync(ruleOutput, actionId);
            actionMappingLogic.Verify();
            result.AssertOnError();
            Assert.Equal(saveObject.RuleOutput, ruleOutput);
            Assert.Equal(saveObject.ActionId, actionId);
        }


        [Fact]
        public async void GetActionIdFromRuleOutputAsyncTest()
        {
            var ruleOutput = fixture.Create<string>();
            var actionId = fixture.Create<string>();
            actionMappingLogic.Setup(mock => mock.GetActionIdFromRuleOutputAsync(ruleOutput))
                .ReturnsAsync(actionId)
                .Verifiable();
            var result = await deviceActionsApiController.GetActionIdFromRuleOutputAsync(ruleOutput);
            actionMappingLogic.Verify();
            result.AssertOnError();
            var data = result.ExtractContentDataAs<string>();
            Assert.Equal(data, actionId);
        }

        [Fact]
        public async void GetAvailableRuleOutputsAsyncTest()
        {
            var ruleOutList = fixture.Create<List<string>>();
            actionMappingLogic.Setup(mock => mock.GetAvailableRuleOutputsAsync()).ReturnsAsync(ruleOutList).Verifiable();

            var result = await deviceActionsApiController.GetAvailableRuleOutputsAsync();
            actionMappingLogic.Verify();
            result.AssertOnError();
            var data = result.ExtractContentDataAs<List<string>>();
            Assert.Equal(data, ruleOutList);
        }
    }
}