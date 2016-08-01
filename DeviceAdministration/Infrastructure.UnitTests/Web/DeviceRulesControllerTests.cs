using System.Web.Mvc;
using Microsoft.Azure.Devices.Applications.RemoteMonitoring.DeviceAdmin.Infrastructure.BusinessLogic;
using Microsoft.Azure.Devices.Applications.RemoteMonitoring.DeviceAdmin.Infrastructure.Models;
using Microsoft.Azure.Devices.Applications.RemoteMonitoring.DeviceAdmin.Web.Controllers;
using Microsoft.Azure.Devices.Applications.RemoteMonitoring.DeviceAdmin.Web.Models;
using Moq;
using Ploeh.AutoFixture;
using Xunit;

namespace Microsoft.Azure.Devices.Applications.RemoteMonitoring.DeviceAdmin.Infrastructure.UnitTests.Web
{
    public class DeviceRulesControllerTests
    {
        private readonly DeviceRulesController deviceRulesController;
        private readonly Mock<IDeviceRulesLogic> deviceRulesMock;
        private readonly Fixture fixture;

        public DeviceRulesControllerTests()
        {
            this.deviceRulesMock = new Mock<IDeviceRulesLogic>();
            this.deviceRulesController = new DeviceRulesController(this.deviceRulesMock.Object);
            this.fixture = new Fixture();
        }

        [Fact]
        public async void Index()
        {
            var result = this.deviceRulesController.Index();
            var view = result as ViewResult;
            Assert.NotNull(view);
        }

        [Fact]
        public async void GetRuleProperties()
        {
            string deviceId = this.fixture.Create<string>();
            string ruleId = this.fixture.Create<string>();
            DeviceRule deviceRule = this.fixture.Create<DeviceRule>();
            this.deviceRulesMock.Setup(mock => mock.GetDeviceRuleAsync(deviceId, ruleId)).ReturnsAsync(deviceRule);
            var result = await this.deviceRulesController.GetRuleProperties(deviceId, ruleId);
            var view = result as PartialViewResult;
            var model = view.Model as EditDeviceRuleModel;
            Assert.Equal(model.RuleId, deviceRule.RuleId);
            Assert.Equal(model.DataField, deviceRule.DataField);
            Assert.Equal(model.DeviceID, deviceRule.DeviceID);
            Assert.Equal(model.EnabledState, deviceRule.EnabledState);
            Assert.Equal(model.Operator, deviceRule.Operator);
            Assert.Equal(model.RuleOutput, deviceRule.RuleOutput);
            Assert.Equal(model.Threshold, deviceRule.Threshold.ToString());
        }

        [Fact]
        public async void UpdateRuleProperties()
        {
        }


        [Fact]
        public async void GetNewRule()
        {
        }

        [Fact]
        public async void UpdateRuleEnabledState()
        {
        }

        [Fact]
        public async void DeleteDeviceRule()
        {
        }


        [Fact]
        public async void EditRuleProperties()
        {
        }

        [Fact]
        public async void RemoveRule()
        {
        }
    }
}
