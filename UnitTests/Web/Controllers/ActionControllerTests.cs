using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;
using Microsoft.Azure.Devices.Applications.RemoteMonitoring.DeviceAdmin.Infrastructure.BusinessLogic;
using Microsoft.Azure.Devices.Applications.RemoteMonitoring.DeviceAdmin.Web.Controllers;
using Microsoft.Azure.Devices.Applications.RemoteMonitoring.DeviceAdmin.Web.Models;
using Moq;
using Ploeh.AutoFixture;
using Xunit;

namespace Microsoft.Azure.Devices.Applications.RemoteMonitoring.UnitTests.Web
{
    public class ActionControllerTests
    {
        private readonly Fixture _fixture;
        private readonly Mock<IActionLogic> _actionLogicMock;
        private readonly ActionsController _actionsController;

        public ActionControllerTests()
        {
            var actionMappingMock = new Mock<IActionMappingLogic>();
            _actionLogicMock = new Mock<IActionLogic>();
            _actionsController = new ActionsController(actionMappingMock.Object,
                _actionLogicMock.Object);

            _fixture = new Fixture();
        }

        [Fact]
        public void IndexTest()
        {
            var result = _actionsController.Index();
            var view = result as ViewResult;
            var model = view.Model as ActionPropertiesModel;
            Assert.NotNull(model);
        }

        [Fact]
        public async void GetAvailableLogicAppActionsTest()
        {
            var actionIds = _fixture.Create<List<string>>();
            _actionLogicMock.Setup(mock => mock.GetAllActionIdsAsync()).ReturnsAsync(actionIds);

            var result = await _actionsController.GetAvailableLogicAppActions();
            var viewResult = result as PartialViewResult;
            var model = viewResult.Model as ActionPropertiesModel;
            Assert.Equal(model.UpdateActionModel.ActionSelectList.Count, actionIds.Count);
            Assert.Equal(model.UpdateActionModel.ActionSelectList.First().Text, actionIds.First());
            Assert.Equal(model.UpdateActionModel.ActionSelectList.First().Value, actionIds.First());

            _actionLogicMock.Setup(mock => mock.GetAllActionIdsAsync()).ReturnsAsync(null);
            result = await _actionsController.GetAvailableLogicAppActions();
            viewResult = result as PartialViewResult;
            model = viewResult.Model as ActionPropertiesModel;
            Assert.Equal(model.UpdateActionModel.ActionSelectList.Count, 0);
        }

        [Fact]
        public async void UpdateActionTest()
        {
            var ruleOutput = _fixture.Create<string>();
            var actionId = _fixture.Create<string>();
            var actionMapping = await _actionsController.UpdateAction(ruleOutput, actionId);
            Assert.Equal(actionMapping.RuleOutput, ruleOutput);
            Assert.Equal(actionMapping.ActionId, actionId);
        }
    }
}