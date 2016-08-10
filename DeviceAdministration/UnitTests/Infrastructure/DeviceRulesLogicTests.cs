using System.Collections.Generic;
using Microsoft.Azure.Devices.Applications.RemoteMonitoring.Common.Models;
using Microsoft.Azure.Devices.Applications.RemoteMonitoring.DeviceAdmin.Infrastructure.BusinessLogic;
using Microsoft.Azure.Devices.Applications.RemoteMonitoring.DeviceAdmin.Infrastructure.Models;
using Microsoft.Azure.Devices.Applications.RemoteMonitoring.DeviceAdmin.Infrastructure.Repository;
using Moq;
using Ploeh.AutoFixture;
using Xunit;

namespace Microsoft.Azure.Devices.Applications.RemoteMonitoring.UnitTests.Infrastructure
{
    public class DeviceRulesLogicTests
    {
        private readonly Mock<IActionMappingLogic> _actionMappingLogicMock;
        private readonly Mock<IDeviceRulesRepository> _deviceRulesRepositoryMock;
        private readonly IDeviceRulesLogic deviceRulesLogic;
        private readonly Fixture fixture;

        public DeviceRulesLogicTests()
        {
            _deviceRulesRepositoryMock = new Mock<IDeviceRulesRepository>();
            _actionMappingLogicMock = new Mock<IActionMappingLogic>();
            deviceRulesLogic = new DeviceRulesLogic(_deviceRulesRepositoryMock.Object, _actionMappingLogicMock.Object);
            fixture = new Fixture();
        }

        [Fact]
        public async void GetDeviceRuleOrDefaultAsyncTest()
        {
            var deviceId = fixture.Create<string>();
            var rules = fixture.Create<List<DeviceRule>>();
            rules.ForEach(x => x.DeviceID = deviceId);
            _deviceRulesRepositoryMock.Setup(x => x.GetAllRulesForDeviceAsync(deviceId)).ReturnsAsync(rules);

            var ret = await deviceRulesLogic.GetDeviceRuleOrDefaultAsync(deviceId, rules[0].RuleId);
            Assert.Equal(rules[0], ret);

            ret = await deviceRulesLogic.GetDeviceRuleOrDefaultAsync(deviceId, "RuleNotPresent");
            Assert.NotNull(ret);
            Assert.Equal(deviceId, ret.DeviceID);
        }

        [Fact]
        public async void SaveDeviceRuleAsyncTest()
        {
            var deviceId = fixture.Create<string>();
            var rules = fixture.Create<List<DeviceRule>>();
            rules.ForEach(x => x.DeviceID = deviceId);
            _deviceRulesRepositoryMock.Setup(x => x.GetAllRulesForDeviceAsync(deviceId)).ReturnsAsync(rules);
            _deviceRulesRepositoryMock.Setup(x => x.SaveDeviceRuleAsync(It.IsNotNull<DeviceRule>()))
                .ReturnsAsync(new TableStorageResponse<DeviceRule>());

            var newRule = new DeviceRule();
            newRule.InitializeNewRule(deviceId);
            newRule.DataField = rules[0].DataField;
            var ret = await deviceRulesLogic.SaveDeviceRuleAsync(newRule);
            Assert.NotNull(ret.Entity);
            Assert.Equal(TableStorageResponseStatus.DuplicateInsert, ret.Status);

            newRule.InitializeNewRule(deviceId);
            newRule.DataField = "New data in DataField";
            ret = await deviceRulesLogic.SaveDeviceRuleAsync(newRule);
            Assert.NotNull(ret);
        }

        [Fact]
        public async void GetNewRuleAsyncTest()
        {
            var deviceId = fixture.Create<string>();
            var rule = await deviceRulesLogic.GetNewRuleAsync(deviceId);
            Assert.NotNull(rule);
            Assert.Equal(deviceId, rule.DeviceID);
        }

        [Fact]
        public async void UpdateDeviceRuleEnabledStateAsyncTest()
        {
            var rule = fixture.Create<DeviceRule>();
            var prevState = false;
            rule.EnabledState = prevState;
            _deviceRulesRepositoryMock.Setup(x => x.GetDeviceRuleAsync(rule.DeviceID, rule.RuleId)).ReturnsAsync(rule);
            _deviceRulesRepositoryMock.Setup(
                x => x.SaveDeviceRuleAsync(It.Is<DeviceRule>(o => o.EnabledState != prevState)))
                .ReturnsAsync(new TableStorageResponse<DeviceRule>())
                .Verifiable();

            var res = await deviceRulesLogic.UpdateDeviceRuleEnabledStateAsync(rule.DeviceID, rule.RuleId, true);
            Assert.NotNull(res);
            _deviceRulesRepositoryMock.Verify();

            prevState = rule.EnabledState;
            res = await deviceRulesLogic.UpdateDeviceRuleEnabledStateAsync(rule.DeviceID, rule.RuleId, false);
            Assert.NotNull(res);
            _deviceRulesRepositoryMock.Verify();
        }
    }
}