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
            deviceRulesMock = new Mock<IDeviceRulesLogic>();
            deviceRulesController = new DeviceRulesController(deviceRulesMock.Object);
            fixture = new Fixture();
        }

        [Fact]
        public async void IndexTest()
        {
            var result = deviceRulesController.Index();
            var view = result as ViewResult;
            Assert.NotNull(view);
        }

        [Fact]
        public async void GetRulePropertiesTest()
        {
            var deviceId = fixture.Create<string>();
            var ruleId = fixture.Create<string>();
            var deviceRule = fixture.Create<DeviceRule>();
            deviceRulesMock.Setup(mock => mock.GetDeviceRuleAsync(deviceId, ruleId)).ReturnsAsync(deviceRule);
            var result = await deviceRulesController.GetRuleProperties(deviceId, ruleId);
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
            var model = fixture.Create<EditDeviceRuleModel>();
            model.Threshold = null;
            var result = await deviceRulesController.UpdateRuleProperties(model);
            var view = result as JsonResult;
            var data = JsonConvert.SerializeObject(view.Data);
            var obj = JsonConvert.SerializeObject(new {error = "The Threshold must be a valid double."});
            Assert.Equal(data, obj);

            var tableResponse = fixture.Create<TableStorageResponse<DeviceRule>>();
            tableResponse.Status = TableStorageResponseStatus.Successful;
            model = fixture.Create<EditDeviceRuleModel>();
            model.Threshold = "2.14";
            deviceRulesMock.Setup(mock => mock.SaveDeviceRuleAsync(It.IsAny<DeviceRule>()))
                .ReturnsAsync(tableResponse)
                .Verifiable();
            result = await deviceRulesController.UpdateRuleProperties(model);
            view = result as JsonResult;
            data = JsonConvert.SerializeObject(view.Data);
            obj = JsonConvert.SerializeObject(new {success = true});
            Assert.Equal(data, obj);

            tableResponse = fixture.Create<TableStorageResponse<DeviceRule>>();
            tableResponse.Status = TableStorageResponseStatus.ConflictError;
            model = fixture.Create<EditDeviceRuleModel>();
            model.Threshold = "2.14";
            deviceRulesMock.Setup(mock => mock.SaveDeviceRuleAsync(It.IsAny<DeviceRule>()))
                .ReturnsAsync(tableResponse)
                .Verifiable();
            result = await deviceRulesController.UpdateRuleProperties(model);
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
            var deviceId = fixture.Create<string>();
            var rule = fixture.Create<DeviceRule>();
            deviceRulesMock.Setup(mock => mock.GetNewRuleAsync(deviceId)).ReturnsAsync(rule);
            var result = await deviceRulesController.GetNewRule(deviceId);
            var view = result as JsonResult;
            Assert.Equal(JsonConvert.SerializeObject(view.Data), JsonConvert.SerializeObject(rule));
        }

        [Fact]
        public async void UpdateRuleEnabledStateTest()
        {
            var ruleModel = fixture.Create<EditDeviceRuleModel>();
            var response = fixture.Create<TableStorageResponse<DeviceRule>>();
            response.Status = TableStorageResponseStatus.Successful;
            deviceRulesMock.Setup(
                mock =>
                    mock.UpdateDeviceRuleEnabledStateAsync(ruleModel.DeviceID, ruleModel.RuleId, ruleModel.EnabledState))
                .ReturnsAsync(response).Verifiable();
            var result = await deviceRulesController.UpdateRuleEnabledState(ruleModel);
            var view = result as JsonResult;
            var data = JsonConvert.SerializeObject(view.Data);
            var obj = JsonConvert.SerializeObject(new {success = true});
            Assert.Equal(data, obj);
            deviceRulesMock.Verify();
        }

        [Fact]
        public async void DeleteDeviceRuleTest()
        {
            var response = fixture.Create<TableStorageResponse<DeviceRule>>();
            response.Status = TableStorageResponseStatus.Successful;
            var deviceId = fixture.Create<string>();
            var ruleId = fixture.Create<string>();
            deviceRulesMock.Setup(mock => mock.DeleteDeviceRuleAsync(deviceId, ruleId)).ReturnsAsync(response);
            var result = await deviceRulesController.DeleteDeviceRule(deviceId, ruleId);
            var view = result as JsonResult;
            var data = JsonConvert.SerializeObject(view.Data);
            var obj = JsonConvert.SerializeObject(new {success = true});
            Assert.Equal(data, obj);
            deviceRulesMock.Verify();
        }

        [Fact]
        public async void EditRulePropertiesTest()
        {
            var deviceId = fixture.Create<string>();
            string ruleId = null;
            deviceRulesMock.Setup(mock => mock.CanNewRuleBeCreatedForDeviceAsync(deviceId)).ReturnsAsync(false);
            var result = await deviceRulesController.EditRuleProperties(deviceId, ruleId);
            var view = result as ViewResult;
            var model = view.Model as EditDeviceRuleModel;
            Assert.Equal(model.DeviceID, deviceId);

            deviceId = fixture.Create<string>();
            ruleId = fixture.Create<string>();
            var ruleModel = fixture.Create<DeviceRule>();
            var availableFields = new Dictionary<string, List<string>>();
            availableFields["availableDataFields"] = fixture.Create<List<string>>();
            availableFields["availableOperators"] = fixture.Create<List<string>>();
            availableFields["availableRuleOutputs"] = fixture.Create<List<string>>();

            deviceRulesMock.Setup(mock => mock.GetDeviceRuleOrDefaultAsync(deviceId, ruleId)).ReturnsAsync(ruleModel);
            deviceRulesMock.Setup(
                mock => mock.GetAvailableFieldsForDeviceRuleAsync(ruleModel.DeviceID, ruleModel.RuleId))
                .ReturnsAsync(availableFields);
            result = await deviceRulesController.EditRuleProperties(deviceId, ruleId);
            view = result as ViewResult;
            model = view.Model as EditDeviceRuleModel;
            Assert.Equal(model.AvailableDataFields.Count, availableFields["availableDataFields"].Count);
            Assert.Equal(model.AvailableOperators.Count, availableFields["availableOperators"].Count);
            Assert.Equal(model.AvailableRuleOutputs.Count, availableFields["availableRuleOutputs"].Count);
        }

        [Fact]
        public async void RemoveRule()
        {
            var deviceId = fixture.Create<string>();
            var ruleId = fixture.Create<string>();
            var ruleModel = fixture.Create<DeviceRule>();
            deviceRulesMock.Setup(mock => mock.GetDeviceRuleOrDefaultAsync(deviceId, ruleId)).ReturnsAsync(ruleModel);
            var result = await deviceRulesController.RemoveRule(deviceId, ruleId);
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