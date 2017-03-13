using System;
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

namespace Microsoft.Azure.Devices.Applications.RemoteMonitoring.UnitTests.Web
{
    public class DeviceRulesControllerTests : IDisposable
    {
        private readonly DeviceRulesController _deviceRulesController;
        private readonly Mock<IDeviceRulesLogic> _deviceRulesMock;
        private readonly Fixture fixture;

        public DeviceRulesControllerTests()
        {
            _deviceRulesMock = new Mock<IDeviceRulesLogic>();
            _deviceRulesController = new DeviceRulesController(_deviceRulesMock.Object);
            fixture = new Fixture();
        }

        [Fact]
        public void IndexTest()
        {
            var result = _deviceRulesController.Index();
            var view = result as ViewResult;
            Assert.NotNull(view);
        }

        [Fact]
        public async void GetRulePropertiesTest()
        {
            var deviceId = fixture.Create<string>();
            var ruleId = fixture.Create<string>();
            var deviceRule = fixture.Create<DeviceRule>();

            _deviceRulesMock.Setup(mock => mock.GetDeviceRuleAsync(deviceId, ruleId)).ReturnsAsync(deviceRule);

            var result = await _deviceRulesController.GetRuleProperties(deviceId, ruleId);
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
            var result = await _deviceRulesController.UpdateRuleProperties(model);
            var view = result as JsonResult;
            var data = JsonConvert.SerializeObject(view.Data);
            var obj = JsonConvert.SerializeObject(new { error = "The Threshold must be a valid double." });
            Assert.Equal(data, obj);

            var tableResponse = fixture.Create<TableStorageResponse<DeviceRule>>();
            tableResponse.Status = TableStorageResponseStatus.Successful;
            model = fixture.Create<EditDeviceRuleModel>();
            model.Threshold = "2.14";
            _deviceRulesMock.Setup(mock => mock.SaveDeviceRuleAsync(It.IsAny<DeviceRule>()))
                .ReturnsAsync(tableResponse)
                .Verifiable();
            result = await _deviceRulesController.UpdateRuleProperties(model);
            view = result as JsonResult;
            data = JsonConvert.SerializeObject(view.Data);
            obj = JsonConvert.SerializeObject(new { success = true });
            Assert.Equal(data, obj);

            tableResponse = fixture.Create<TableStorageResponse<DeviceRule>>();
            tableResponse.Status = TableStorageResponseStatus.ConflictError;
            model = fixture.Create<EditDeviceRuleModel>();
            model.Threshold = "2.14";
            _deviceRulesMock.Setup(mock => mock.SaveDeviceRuleAsync(It.IsAny<DeviceRule>()))
                .ReturnsAsync(tableResponse)
                .Verifiable();
            result = await _deviceRulesController.UpdateRuleProperties(model);
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
            _deviceRulesMock.Setup(mock => mock.GetNewRuleAsync(deviceId)).ReturnsAsync(rule);
            var result = await _deviceRulesController.GetNewRule(deviceId);
            var view = result as JsonResult;
            Assert.Equal(JsonConvert.SerializeObject(view.Data), JsonConvert.SerializeObject(rule));
        }

        [Fact]
        public async void UpdateRuleEnabledStateTest()
        {
            var ruleModel = fixture.Create<EditDeviceRuleModel>();
            var response = fixture.Create<TableStorageResponse<DeviceRule>>();
            response.Status = TableStorageResponseStatus.Successful;
            _deviceRulesMock.Setup(
                mock =>
                    mock.UpdateDeviceRuleEnabledStateAsync(ruleModel.DeviceID, ruleModel.RuleId, ruleModel.EnabledState))
                .ReturnsAsync(response).Verifiable();
            var result = await _deviceRulesController.UpdateRuleEnabledState(ruleModel);
            var view = result as JsonResult;
            var data = JsonConvert.SerializeObject(view.Data);
            var obj = JsonConvert.SerializeObject(new { success = true });
            Assert.Equal(data, obj);
            _deviceRulesMock.Verify();
        }

        [Fact]
        public async void DeleteDeviceRuleTest()
        {
            var response = fixture.Create<TableStorageResponse<DeviceRule>>();
            response.Status = TableStorageResponseStatus.Successful;
            var deviceId = fixture.Create<string>();
            var ruleId = fixture.Create<string>();
            _deviceRulesMock.Setup(mock => mock.DeleteDeviceRuleAsync(deviceId, ruleId)).ReturnsAsync(response);
            var result = await _deviceRulesController.DeleteDeviceRule(deviceId, ruleId);
            var view = result as JsonResult;
            var data = JsonConvert.SerializeObject(view.Data);
            var obj = JsonConvert.SerializeObject(new { success = true });
            Assert.Equal(data, obj);
            _deviceRulesMock.Verify();
        }

        [Fact]
        public async void EditRulePropertiesTest()
        {
            var deviceId = fixture.Create<string>();
            string ruleId = null;
            _deviceRulesMock.Setup(mock => mock.CanNewRuleBeCreatedForDeviceAsync(deviceId)).ReturnsAsync(false);
            var result = await _deviceRulesController.EditRuleProperties(deviceId, ruleId);
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

            _deviceRulesMock.Setup(mock => mock.GetDeviceRuleOrDefaultAsync(deviceId, ruleId)).ReturnsAsync(ruleModel);
            _deviceRulesMock.Setup(
                mock => mock.GetAvailableFieldsForDeviceRuleAsync(ruleModel.DeviceID, ruleModel.RuleId))
                .ReturnsAsync(availableFields);
            result = await _deviceRulesController.EditRuleProperties(deviceId, ruleId);
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
            _deviceRulesMock.Setup(mock => mock.GetDeviceRuleOrDefaultAsync(deviceId, ruleId)).ReturnsAsync(ruleModel);
            var result = await _deviceRulesController.RemoveRule(deviceId, ruleId);
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

        #region IDisposable Support
        private bool disposedValue = false; // To detect redundant calls

        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                    _deviceRulesController.Dispose();
                }

                // TODO: free unmanaged resources (unmanaged objects) and override a finalizer below.
                // TODO: set large fields to null.

                disposedValue = true;
            }
        }

        // TODO: override a finalizer only if Dispose(bool disposing) above has code to free unmanaged resources.
        // ~DeviceRulesControllerTests() {
        //   // Do not change this code. Put cleanup code in Dispose(bool disposing) above.
        //   Dispose(false);
        // }

        // This code added to correctly implement the disposable pattern.
        public void Dispose()
        {
            // Do not change this code. Put cleanup code in Dispose(bool disposing) above.
            Dispose(true);
            // TODO: uncomment the following line if the finalizer is overridden above.
            // GC.SuppressFinalize(this);
        }
        #endregion
    }
}