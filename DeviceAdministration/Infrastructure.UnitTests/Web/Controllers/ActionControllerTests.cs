using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;
using Microsoft.Azure.Devices.Applications.RemoteMonitoring.DeviceAdmin.Infrastructure.BusinessLogic;
using Microsoft.Azure.Devices.Applications.RemoteMonitoring.DeviceAdmin.Web.Controllers;
using Microsoft.Azure.Devices.Applications.RemoteMonitoring.DeviceAdmin.Web.Models;
using Moq;
using Ploeh.AutoFixture;
using Xunit;

namespace Microsoft.Azure.Devices.Applications.RemoteMonitoring.DeviceAdmin.Infrastructure.UnitTests.Web
{
    public class ActionControllerTests
    {
        private readonly ActionsController actionsController;
        private readonly Mock<IActionMappingLogic> actionMappingMock;
        private readonly Mock<IActionLogic> actionLogicMock;
        private readonly Fixture fixture;

        public ActionControllerTests()
        {
            this.actionMappingMock = new Mock<IActionMappingLogic>();
            this.actionLogicMock = new Mock<IActionLogic>();
            this.actionsController = new ActionsController(this.actionMappingMock.Object,
                                                           this.actionLogicMock.Object);

            this.fixture = new Fixture();
        }

        [Fact]
        public void IndexTest()
        {
            var result = this.actionsController.Index();
            var view = result as ViewResult;
            var model = view.Model as ActionPropertiesModel;
            Assert.NotNull(model);
        }

        [Fact]
        public async void GetAvailableLogicAppActionsTest()
        {
            var actionIds = this.fixture.Create<List<string>>();
            this.actionLogicMock.Setup(mock => mock.GetAllActionIdsAsync()).ReturnsAsync(actionIds);

            var result = await this.actionsController.GetAvailableLogicAppActions();
            var viewResult = result as PartialViewResult;
            var model = viewResult.Model as ActionPropertiesModel;
            Assert.Equal(model.UpdateActionModel.ActionSelectList.Count, actionIds.Count);
            Assert.Equal(model.UpdateActionModel.ActionSelectList.First().Text, actionIds.First());
            Assert.Equal(model.UpdateActionModel.ActionSelectList.First().Value, actionIds.First());

            this.actionLogicMock.Setup(mock => mock.GetAllActionIdsAsync()).ReturnsAsync(null);
            result = await this.actionsController.GetAvailableLogicAppActions();
            viewResult = result as PartialViewResult;
            model = viewResult.Model as ActionPropertiesModel;
            Assert.Equal(model.UpdateActionModel.ActionSelectList.Count, 0);
        }

        [Fact]
        public async void UpdateActionTest()
        {
            string ruleOutput = this.fixture.Create<string>();
            string actionId = this.fixture.Create<string>();
            var actionMapping = await this.actionsController.UpdateAction(ruleOutput, actionId);
            Assert.Equal(actionMapping.RuleOutput, ruleOutput);
            Assert.Equal(actionMapping.ActionId, actionId);
        }
    }
}
