using System.Collections.Generic;
using System.Web.Mvc;
using Microsoft.Azure.Devices.Applications.RemoteMonitoring.Common.Models;
using Microsoft.Azure.Devices.Applications.RemoteMonitoring.DeviceAdmin.Infrastructure.BusinessLogic;
using Microsoft.Azure.Devices.Applications.RemoteMonitoring.DeviceAdmin.Infrastructure.Models;
using Microsoft.Azure.Devices.Applications.RemoteMonitoring.DeviceAdmin.Web.Controllers;
using Microsoft.Azure.Devices.Applications.RemoteMonitoring.DeviceAdmin.Web.Models;
using Moq;
using Newtonsoft.Json;
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
        public async void IndexTest()
        {
            var result = this.deviceRulesController.Index();
            var view = result as ViewResult;
            Assert.NotNull(view);
        }

        [Fact]
        public async void GetRulePropertiesTest()
        {
            var deviceId = this.fixture.Create<string>();
            var ruleId = this.fixture.Create<string>();
            var deviceRule = this.fixture.Create<DeviceRule>();
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
        public async void UpdateRulePropertiesTest()
        {
            var model = this.fixture.Create<EditDeviceRuleModel>();
            model.Threshold = null;
            var result = await this.deviceRulesController.UpdateRuleProperties(model);
            var view = result as JsonResult;
            var data = JsonConvert.SerializeObject(view.Data);
            var obj = JsonConvert.SerializeObject(new {error = "The Threshold must be a valid double."});
            Assert.Equal(data, obj);

            var tableResponse = this.fixture.Create<TableStorageResponse<DeviceRule>>();
            tableResponse.Status = TableStorageResponseStatus.Successful;
            model = this.fixture.Create<EditDeviceRuleModel>();
            model.Threshold = "2.14";
            this.deviceRulesMock.Setup(mock => mock.SaveDeviceRuleAsync(It.IsAny<DeviceRule>())).ReturnsAsync(tableResponse).Verifiable();
            result = await this.deviceRulesController.UpdateRuleProperties(model);
            view = result as JsonResult;
            data = JsonConvert.SerializeObject(view.Data);
            obj = JsonConvert.SerializeObject(new {success = true});
            Assert.Equal(data, obj);

            tableResponse = this.fixture.Create<TableStorageResponse<DeviceRule>>();
            tableResponse.Status = TableStorageResponseStatus.ConflictError;
            model = this.fixture.Create<EditDeviceRuleModel>();
            model.Threshold = "2.14";
            this.deviceRulesMock.Setup(mock => mock.SaveDeviceRuleAsync(It.IsAny<DeviceRule>())).ReturnsAsync(tableResponse).Verifiable();
            result = await this.deviceRulesController.UpdateRuleProperties(model);
            view = result as JsonResult;
            data = JsonConvert.SerializeObject(view.Data);
            obj = JsonConvert.SerializeObject(new
                                              {
                                                  error = "There was a conflict while saving the data. Please verify the data and try again.",
                                                  entity = JsonConvert.SerializeObject(tableResponse.Entity)
                                              });
            Assert.Equal(data, obj);
        }

        [Fact]
        public async void GetNewRuleTest()
        {
            var deviceId = this.fixture.Create<string>();
            var rule = this.fixture.Create<DeviceRule>();
            this.deviceRulesMock.Setup(mock => mock.GetNewRuleAsync(deviceId)).ReturnsAsync(rule);
            var result = await this.deviceRulesController.GetNewRule(deviceId);
            var view = result as JsonResult;
            Assert.Equal(JsonConvert.SerializeObject(view.Data), JsonConvert.SerializeObject(rule));
        }

        [Fact]
        public async void UpdateRuleEnabledStateTest()
        {
            var ruleModel = this.fixture.Create<EditDeviceRuleModel>();
            var response = this.fixture.Create<TableStorageResponse<DeviceRule>>();
            response.Status = TableStorageResponseStatus.Successful;
            this.deviceRulesMock.Setup(mock => mock.UpdateDeviceRuleEnabledStateAsync(ruleModel.DeviceID, ruleModel.RuleId, ruleModel.EnabledState))
                .ReturnsAsync(response).Verifiable();
            var result = await this.deviceRulesController.UpdateRuleEnabledState(ruleModel);
            var view = result as JsonResult;
            var data = JsonConvert.SerializeObject(view.Data);
            var obj = JsonConvert.SerializeObject(new {success = true});
            Assert.Equal(data, obj);
            this.deviceRulesMock.Verify();
        }

        [Fact]
        public async void DeleteDeviceRuleTest()
        {
            var response = this.fixture.Create<TableStorageResponse<DeviceRule>>();
            response.Status = TableStorageResponseStatus.Successful;
            var deviceId = this.fixture.Create<string>();
            var ruleId = this.fixture.Create<string>();
            this.deviceRulesMock.Setup(mock => mock.DeleteDeviceRuleAsync(deviceId, ruleId)).ReturnsAsync(response);
            var result = await this.deviceRulesController.DeleteDeviceRule(deviceId, ruleId);
            var view = result as JsonResult;
            var data = JsonConvert.SerializeObject(view.Data);
            var obj = JsonConvert.SerializeObject(new {success = true});
            Assert.Equal(data, obj);
            this.deviceRulesMock.Verify();
        }

        [Fact]
        public async void EditRulePropertiesTest()
        {
            var deviceId = this.fixture.Create<string>();
            string ruleId = null;
            this.deviceRulesMock.Setup(mock => mock.CanNewRuleBeCreatedForDeviceAsync(deviceId)).ReturnsAsync(false);
            var result = await this.deviceRulesController.EditRuleProperties(deviceId, ruleId);
            var view = result as ViewResult;
            var model = view.Model as EditDeviceRuleModel;
            Assert.Equal(model.DeviceID, deviceId);

            deviceId = this.fixture.Create<string>();
            ruleId = this.fixture.Create<string>();
            var ruleModel = this.fixture.Create<DeviceRule>();
            var availableFields = new Dictionary<string, List<string>>();
            availableFields["availableDataFields"] = this.fixture.Create<List<string>>();
            availableFields["availableOperators"] = this.fixture.Create<List<string>>();
            availableFields["availableRuleOutputs"] = this.fixture.Create<List<string>>();

            this.deviceRulesMock.Setup(mock => mock.GetDeviceRuleOrDefaultAsync(deviceId, ruleId)).ReturnsAsync(ruleModel);
            this.deviceRulesMock.Setup(mock => mock.GetAvailableFieldsForDeviceRuleAsync(ruleModel.DeviceID, ruleModel.RuleId))
                .ReturnsAsync(availableFields);
            result = await this.deviceRulesController.EditRuleProperties(deviceId, ruleId);
            view = result as ViewResult;
            model = view.Model as EditDeviceRuleModel;
            Assert.Equal(model.AvailableDataFields.Count, availableFields["availableDataFields"].Count);
            Assert.Equal(model.AvailableOperators.Count, availableFields["availableOperators"].Count);
            Assert.Equal(model.AvailableRuleOutputs.Count, availableFields["availableRuleOutputs"].Count);
        }

        [Fact]
        public async void RemoveRule()
        {
            var deviceId = this.fixture.Create<string>();
            var ruleId = this.fixture.Create<string>();
            var ruleModel = this.fixture.Create<DeviceRule>();
            this.deviceRulesMock.Setup(mock => mock.GetDeviceRuleOrDefaultAsync(deviceId, ruleId)).ReturnsAsync(ruleModel);
            var result = await this.deviceRulesController.RemoveRule(deviceId, ruleId);
            var view = result as ViewResult;
            var model = view.Model as EditDeviceRuleModel;
            Assert.Equal(model.RuleId, ruleModel.RuleId);
            Assert.Equal(model.DataField, ruleModel.DataField);
            Assert.Equal(model.DeviceID, ruleModel.DeviceID);
            Assert.Equal(model.EnabledState, ruleModel.EnabledState);
            Assert.Equal(model.Operator, ruleModel.Operator);
            Assert.Equal(model.RuleOutput, ruleModel.RuleOutput);
            Assert.Equal(model.Threshold, ruleModel.Threshold.ToString());
        }
    }
}
