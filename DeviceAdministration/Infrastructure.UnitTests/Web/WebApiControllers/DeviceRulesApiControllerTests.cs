using System.Collections.Generic;
using Microsoft.Azure.Devices.Applications.RemoteMonitoring.Common.Models;
using Microsoft.Azure.Devices.Applications.RemoteMonitoring.DeviceAdmin.Infrastructure.BusinessLogic;
using Microsoft.Azure.Devices.Applications.RemoteMonitoring.DeviceAdmin.Infrastructure.Models;
using Microsoft.Azure.Devices.Applications.RemoteMonitoring.DeviceAdmin.Web.DataTables;
using Microsoft.Azure.Devices.Applications.RemoteMonitoring.DeviceAdmin.Web.WebApiControllers;
using Moq;
using Ploeh.AutoFixture;
using Xunit;

namespace Microsoft.Azure.Devices.Applications.RemoteMonitoring.DeviceAdmin.Infrastructure.UnitTests.Web.WebApiControllers
{
    public class DeviceRulesApiControllerTests
    {
        private readonly Mock<IDeviceRulesLogic> deviceRulesLogic;
        private readonly DeviceRulesApiController deviceRulesApiController;
        private readonly IFixture fixture;

        public DeviceRulesApiControllerTests()
        {
            this.deviceRulesLogic = new Mock<IDeviceRulesLogic>();
            this.deviceRulesApiController = new DeviceRulesApiController(this.deviceRulesLogic.Object);
            this.deviceRulesApiController.InitializeRequest();
            this.fixture = new Fixture();
        }

        [Fact]
        public async void GetDeviceRulesAsyncTest()
        {
            var rules = this.fixture.Create<List<DeviceRule>>();
            this.deviceRulesLogic.Setup(mock => mock.GetAllRulesAsync()).ReturnsAsync(rules);
            var res = await this.deviceRulesApiController.GetDeviceRulesAsync();
            res.AssertOnError();
            var data = res.ExtractContentDataAs<List<DeviceRule>>();
            Assert.Equal(data, rules);
        }

        [Fact]
        public async void GetDeviceRulesAsyncTest2()
        {
            var rules = this.fixture.Create<List<DeviceRule>>();
            this.deviceRulesLogic.Setup(mock => mock.GetAllRulesAsync()).ReturnsAsync(rules);
            var res = await this.deviceRulesApiController.GetDeviceRulesAsDataTablesResponseAsync();
            res.AssertOnError();
            var data = res.ExtractContentAs<DataTablesResponse<DeviceRule>>();
            Assert.Equal(data.RecordsTotal, rules.Count);
            Assert.Equal(data.RecordsFiltered, rules.Count);
            Assert.Equal(data.Data, rules.ToArray());
        }

        [Fact]
        public async void GetDeviceRuleOrDefaultAsyncTest()
        {
            var deviceRule = this.fixture.Create<DeviceRule>();
            var deviceId = this.fixture.Create<string>();
            var ruleId = this.fixture.Create<string>();
            this.deviceRulesLogic.Setup(mock => mock.GetDeviceRuleOrDefaultAsync(deviceId, ruleId)).ReturnsAsync(deviceRule);
            var res = await this.deviceRulesApiController.GetDeviceRuleOrDefaultAsync(deviceId, ruleId);
            res.AssertOnError();
            var data = res.ExtractContentDataAs<DeviceRule>();
            Assert.Equal(data, deviceRule);
        }

        [Fact]
        public async void GetAvailableFieldsForDeviceRuleAsyncTest()
        {
            var dict = this.fixture.Create<Dictionary<string, List<string>>>();
            var deviceId = this.fixture.Create<string>();
            var ruleId = this.fixture.Create<string>();

            this.deviceRulesLogic.Setup(mock => mock.GetAvailableFieldsForDeviceRuleAsync(deviceId, ruleId)).ReturnsAsync(dict);
            var res = await this.deviceRulesApiController.GetAvailableFieldsForDeviceRuleAsync(deviceId, ruleId);
            res.AssertOnError();
            var data = res.ExtractContentDataAs<Dictionary<string, List<string>>>();
            Assert.Equal(data, dict);
        }

        [Fact]
        public async void SaveDeviceRuleAsyncTest()
        {
            var tableResp = this.fixture.Create<TableStorageResponse<DeviceRule>>();
            var updatedRule = this.fixture.Create<DeviceRule>();
            this.deviceRulesLogic.Setup(mock => mock.SaveDeviceRuleAsync(updatedRule)).ReturnsAsync(tableResp);
            var res = await this.deviceRulesApiController.SaveDeviceRuleAsync(updatedRule);
            res.AssertOnError();
            var data = res.ExtractContentDataAs<TableStorageResponse<DeviceRule>>();
            Assert.Equal(data, tableResp);
        }

        [Fact]
        public async void GetNewRuleAsyncTest()
        {
            var deviceRule = this.fixture.Create<DeviceRule>();
            var deviceId = this.fixture.Create<string>();
            this.deviceRulesLogic.Setup(mock => mock.GetNewRuleAsync(deviceId)).ReturnsAsync(deviceRule);
            var res = await this.deviceRulesApiController.GetNewRuleAsync(deviceId);
            res.AssertOnError();
            var data = res.ExtractContentDataAs<DeviceRule>();
            Assert.Equal(data, deviceRule);
        }

        [Fact]
        public async void UpdateRuleEnabledStateAsyncTest()
        {
            var tableResp = this.fixture.Create<TableStorageResponse<DeviceRule>>();
            var deviceId = this.fixture.Create<string>();
            var ruleId = this.fixture.Create<string>();
            var enabled = this.fixture.Create<bool>();

            this.deviceRulesLogic.Setup(mock => mock.UpdateDeviceRuleEnabledStateAsync(deviceId, ruleId, enabled)).ReturnsAsync(tableResp);
            var res = await this.deviceRulesApiController.UpdateRuleEnabledStateAsync(deviceId, ruleId, enabled);
            res.AssertOnError();
            var data = res.ExtractContentDataAs<TableStorageResponse<DeviceRule>>();
            Assert.Equal(data, tableResp);
        }

        [Fact]
        public async void DeleteRuleAsyncTest()
        {
            var tableResp = this.fixture.Create<TableStorageResponse<DeviceRule>>();
            var deviceId = this.fixture.Create<string>();
            var ruleId = this.fixture.Create<string>();

            this.deviceRulesLogic.Setup(mock => mock.DeleteDeviceRuleAsync(deviceId, ruleId)).ReturnsAsync(tableResp);
            var res = await this.deviceRulesApiController.DeleteRuleAsync(deviceId, ruleId);
            res.AssertOnError();
            var data = res.ExtractContentDataAs<TableStorageResponse<DeviceRule>>();
            Assert.Equal(data, tableResp);
        }
    }
}
