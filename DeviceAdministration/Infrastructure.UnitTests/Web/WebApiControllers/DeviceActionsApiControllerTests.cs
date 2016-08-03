using System.Collections.Generic;
using System.Net.Http;
using System.Web.Http;
using Castle.Components.DictionaryAdapter.Xml;
using Microsoft.Azure.Devices.Applications.RemoteMonitoring.DeviceAdmin.Infrastructure.BusinessLogic;
using Microsoft.Azure.Devices.Applications.RemoteMonitoring.DeviceAdmin.Infrastructure.Models;
using Microsoft.Azure.Devices.Applications.RemoteMonitoring.DeviceAdmin.Web.DataTables;
using Microsoft.Azure.Devices.Applications.RemoteMonitoring.DeviceAdmin.Web.WebApiControllers;
using Moq;
using Ploeh.AutoFixture;
using Xunit;
using System.Threading.Tasks;

namespace Microsoft.Azure.Devices.Applications.RemoteMonitoring.DeviceAdmin.Infrastructure.UnitTests.Web.WebApiControllers
{
    public class DeviceActionsApiControllerTests
    {
        private readonly DeviceActionsApiController deviceActionsApiController;
        private readonly Mock<IActionMappingLogic> actionMappingLogic;
        private readonly IFixture fixture;

        public DeviceActionsApiControllerTests()
        {
            this.actionMappingLogic = new Mock<IActionMappingLogic>();
            this.deviceActionsApiController = new DeviceActionsApiController(this.actionMappingLogic.Object);
            this.deviceActionsApiController.InitializeRequest();
            this.fixture = new Fixture();
        }

        [Fact]
        public async void GetDeviceActionsAsyncTest()
        {
            var actionMappings = this.fixture.Create<List<ActionMappingExtended>>();
            this.actionMappingLogic.Setup(mock => mock.GetAllMappingsAsync()).ReturnsAsync(actionMappings);
            var result = await this.deviceActionsApiController.GetDeviceActionsAsync();
            result.AssertOnError();
            var data = result.ExtractContentDataAs<List<ActionMappingExtended>>();
            Assert.Equal(data, actionMappings);
        }

        [Fact]
        public async void GetDeviceActionsAsDataTablesResponseAsyncTest()
        {
            var actionMappings = this.fixture.Create<List<ActionMappingExtended>>();
            this.actionMappingLogic.Setup(mock => mock.GetAllMappingsAsync()).ReturnsAsync(actionMappings);
            var result = await this.deviceActionsApiController.GetDeviceActionsAsDataTablesResponseAsync();
            result.AssertOnError();
            var data = result.ExtractContentAs<DataTablesResponse<ActionMappingExtended>>();
            Assert.Equal(data.RecordsTotal, actionMappings.Count);
            Assert.Equal(data.RecordsFiltered, actionMappings.Count);
            Assert.Equal(data.Data, actionMappings.ToArray());
        }

        [Fact]
        public async void UpdateActionAsyncTest()
        {
            string ruleOutput = this.fixture.Create<string>();
            string actionId = this.fixture.Create<string>();
            ActionMapping saveObject = null;

            this.actionMappingLogic.Setup(mock => mock.SaveMappingAsync(It.IsAny<ActionMapping>()))
                .Callback<ActionMapping>(obj => saveObject = obj)
                .Returns(Task.FromResult(true)).Verifiable();
            var result = await this.deviceActionsApiController.UpdateActionAsync(ruleOutput, actionId);
            this.actionMappingLogic.Verify();
            result.AssertOnError();
            Assert.Equal(saveObject.RuleOutput, ruleOutput);
            Assert.Equal(saveObject.ActionId, actionId);
        }


        [Fact]
        public async void GetActionIdFromRuleOutputAsyncTest()
        {
            string ruleOutput = this.fixture.Create<string>();
            string actionId = this.fixture.Create<string>();
            this.actionMappingLogic.Setup(mock => mock.GetActionIdFromRuleOutputAsync(ruleOutput)).ReturnsAsync(actionId).Verifiable();
            var result = await this.deviceActionsApiController.GetActionIdFromRuleOutputAsync(ruleOutput);
            this.actionMappingLogic.Verify();
            result.AssertOnError();
            var data = result.ExtractContentDataAs<string>();
            Assert.Equal(data, actionId);
        }

        [Fact]
        public async void GetAvailableRuleOutputsAsyncTest()
        {
            List<string> ruleOutList = this.fixture.Create<List<string>>();
            this.actionMappingLogic.Setup(mock => mock.GetAvailableRuleOutputsAsync()).ReturnsAsync(ruleOutList).Verifiable();

            var result = await this.deviceActionsApiController.GetAvailableRuleOutputsAsync();
            this.actionMappingLogic.Verify();
            result.AssertOnError();
            var data = result.ExtractContentDataAs<List<string>>();
            Assert.Equal(data, ruleOutList);
        }
    }
}
