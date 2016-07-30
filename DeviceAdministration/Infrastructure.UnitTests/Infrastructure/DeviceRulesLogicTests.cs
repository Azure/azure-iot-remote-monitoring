using System.Collections.Generic;
using Microsoft.Azure.Devices.Applications.RemoteMonitoring.Common.Models;
using Microsoft.Azure.Devices.Applications.RemoteMonitoring.DeviceAdmin.Infrastructure.BusinessLogic;
using Microsoft.Azure.Devices.Applications.RemoteMonitoring.DeviceAdmin.Infrastructure.Models;
using Microsoft.Azure.Devices.Applications.RemoteMonitoring.DeviceAdmin.Infrastructure.Repository;
using Moq;
using Ploeh.AutoFixture;
using Xunit;

namespace Microsoft.Azure.Devices.Applications.RemoteMonitoring.DeviceAdmin.Infrastructure.UnitTests.Infrastructure
{
    public class DeviceRulesLogicTests
    {
        private Mock<IDeviceRulesRepository> _deviceRulesRepositoryMock;
        private Mock<IActionMappingLogic> _actionMappingLogicMock;
        private IDeviceRulesLogic deviceRulesLogic;
        private Fixture fixture;

        public DeviceRulesLogicTests()
        {
            _deviceRulesRepositoryMock = new Mock<IDeviceRulesRepository>();
            _actionMappingLogicMock = new Mock<IActionMappingLogic>();
            deviceRulesLogic = new DeviceRulesLogic(_deviceRulesRepositoryMock.Object,_actionMappingLogicMock.Object);
            fixture = new Fixture();
        }

        [Fact]
        public async void GetDeviceRuleOrDefaultAsyncTest()
        {
            var deviceId = fixture.Create<string>();
            var rules = fixture.Create<List<DeviceRule>>();
            rules.ForEach(x => x.DeviceID = deviceId);
            _deviceRulesRepositoryMock.Setup(x => x.GetAllRulesForDeviceAsync(deviceId)).ReturnsAsync(rules);

            DeviceRule ret = await deviceRulesLogic.GetDeviceRuleOrDefaultAsync(deviceId, rules[0].RuleId);
            Assert.Equal(rules[0],ret);

            ret = await deviceRulesLogic.GetDeviceRuleOrDefaultAsync(deviceId, "RuleNotPresent");
            Assert.NotNull(ret);
            Assert.Equal(deviceId,ret.DeviceID);
        }

        [Fact]
        public async void SaveDeviceRuleAsyncTest()
        {
            var deviceId = fixture.Create<string>();
            var rules = fixture.Create<List<DeviceRule>>();
            rules.ForEach(x => x.DeviceID = deviceId);
            _deviceRulesRepositoryMock.Setup(x => x.GetAllRulesForDeviceAsync(deviceId)).ReturnsAsync(rules);
            _deviceRulesRepositoryMock.Setup(x => x.SaveDeviceRuleAsync(It.IsNotNull<DeviceRule>())).ReturnsAsync(new TableStorageResponse<DeviceRule>());
            
            DeviceRule newRule = new DeviceRule();
            newRule.InitializeNewRule(deviceId);
            newRule.DataField = rules[0].DataField;
            TableStorageResponse<DeviceRule> ret = await deviceRulesLogic.SaveDeviceRuleAsync(newRule);
            Assert.NotNull(ret.Entity);
            Assert.Equal(TableStorageResponseStatus.DuplicateInsert, ret.Status);

            newRule.InitializeNewRule(deviceId);
            newRule.DataField = "New data in DataField";
            ret = await deviceRulesLogic.SaveDeviceRuleAsync(newRule);
            Assert.NotNull(ret);
        }
    }
}