using System;
using System.Collections.Generic;
using Microsoft.Azure.Devices.Applications.RemoteMonitoring.Common.Models;
using Microsoft.Azure.Devices.Applications.RemoteMonitoring.DeviceAdmin.Infrastructure.BusinessLogic;
using Microsoft.Azure.Devices.Applications.RemoteMonitoring.DeviceAdmin.Infrastructure.Models;
using Microsoft.Azure.Devices.Applications.RemoteMonitoring.DeviceAdmin.Web.DataTables;
using Microsoft.Azure.Devices.Applications.RemoteMonitoring.DeviceAdmin.Web.WebApiControllers;
using Moq;
using Ploeh.AutoFixture;
using Xunit;

namespace Microsoft.Azure.Devices.Applications.RemoteMonitoring.UnitTests.Web.WebApiControllers
{
    public class DeviceRulesApiControllerTests : IDisposable
    {
        private readonly DeviceRulesApiController deviceRulesApiController;
        private readonly Mock<IDeviceRulesLogic> deviceRulesLogic;
        private readonly IFixture fixture;

        public DeviceRulesApiControllerTests()
        {
            deviceRulesLogic = new Mock<IDeviceRulesLogic>();
            deviceRulesApiController = new DeviceRulesApiController(deviceRulesLogic.Object);
            deviceRulesApiController.InitializeRequest();
            fixture = new Fixture();
        }

        [Fact]
        public async void GetDeviceRulesAsyncTest()
        {
            var rules = fixture.Create<List<DeviceRule>>();
            deviceRulesLogic.Setup(mock => mock.GetAllRulesAsync()).ReturnsAsync(rules);
            var res = await deviceRulesApiController.GetDeviceRulesAsync();
            res.AssertOnError();
            var data = res.ExtractContentDataAs<List<DeviceRule>>();
            Assert.Equal(data, rules);
        }

        [Fact]
        public async void GetDeviceRulesAsyncTest2()
        {
            var rules = fixture.Create<List<DeviceRule>>();
            deviceRulesLogic.Setup(mock => mock.GetAllRulesAsync()).ReturnsAsync(rules);
            var res = await deviceRulesApiController.GetDeviceRulesAsDataTablesResponseAsync();
            res.AssertOnError();
            var data = res.ExtractContentAs<DataTablesResponse<DeviceRule>>();
            Assert.Equal(data.RecordsTotal, rules.Count);
            Assert.Equal(data.RecordsFiltered, rules.Count);
            Assert.Equal(data.Data, rules.ToArray());
        }

        [Fact]
        public async void GetDeviceRuleOrDefaultAsyncTest()
        {
            var deviceRule = fixture.Create<DeviceRule>();
            var deviceId = fixture.Create<string>();
            var ruleId = fixture.Create<string>();
            deviceRulesLogic.Setup(mock => mock.GetDeviceRuleOrDefaultAsync(deviceId, ruleId)).ReturnsAsync(deviceRule);
            var res = await deviceRulesApiController.GetDeviceRuleOrDefaultAsync(deviceId, ruleId);
            res.AssertOnError();
            var data = res.ExtractContentDataAs<DeviceRule>();
            Assert.Equal(data, deviceRule);
        }

        [Fact]
        public async void GetAvailableFieldsForDeviceRuleAsyncTest()
        {
            var dict = fixture.Create<Dictionary<string, List<string>>>();
            var deviceId = fixture.Create<string>();
            var ruleId = fixture.Create<string>();

            deviceRulesLogic.Setup(mock => mock.GetAvailableFieldsForDeviceRuleAsync(deviceId, ruleId))
                .ReturnsAsync(dict);
            var res = await deviceRulesApiController.GetAvailableFieldsForDeviceRuleAsync(deviceId, ruleId);
            res.AssertOnError();
            var data = res.ExtractContentDataAs<Dictionary<string, List<string>>>();
            Assert.Equal(data, dict);
        }

        [Fact]
        public async void SaveDeviceRuleAsyncTest()
        {
            var tableResp = fixture.Create<TableStorageResponse<DeviceRule>>();
            var updatedRule = fixture.Create<DeviceRule>();
            deviceRulesLogic.Setup(mock => mock.SaveDeviceRuleAsync(updatedRule)).ReturnsAsync(tableResp);
            var res = await deviceRulesApiController.SaveDeviceRuleAsync(updatedRule);
            res.AssertOnError();
            var data = res.ExtractContentDataAs<TableStorageResponse<DeviceRule>>();
            Assert.Equal(data, tableResp);
        }

        [Fact]
        public async void GetNewRuleAsyncTest()
        {
            var deviceRule = fixture.Create<DeviceRule>();
            var deviceId = fixture.Create<string>();
            deviceRulesLogic.Setup(mock => mock.GetNewRuleAsync(deviceId)).ReturnsAsync(deviceRule);
            var res = await deviceRulesApiController.GetNewRuleAsync(deviceId);
            res.AssertOnError();
            var data = res.ExtractContentDataAs<DeviceRule>();
            Assert.Equal(data, deviceRule);
        }

        [Fact]
        public async void UpdateRuleEnabledStateAsyncTest()
        {
            var tableResp = fixture.Create<TableStorageResponse<DeviceRule>>();
            var deviceId = fixture.Create<string>();
            var ruleId = fixture.Create<string>();
            var enabled = fixture.Create<bool>();

            deviceRulesLogic.Setup(mock => mock.UpdateDeviceRuleEnabledStateAsync(deviceId, ruleId, enabled))
                .ReturnsAsync(tableResp);
            var res = await deviceRulesApiController.UpdateRuleEnabledStateAsync(deviceId, ruleId, enabled);
            res.AssertOnError();
            var data = res.ExtractContentDataAs<TableStorageResponse<DeviceRule>>();
            Assert.Equal(data, tableResp);
        }

        [Fact]
        public async void DeleteRuleAsyncTest()
        {
            var tableResp = fixture.Create<TableStorageResponse<DeviceRule>>();
            var deviceId = fixture.Create<string>();
            var ruleId = fixture.Create<string>();

            deviceRulesLogic.Setup(mock => mock.DeleteDeviceRuleAsync(deviceId, ruleId)).ReturnsAsync(tableResp);
            var res = await deviceRulesApiController.DeleteRuleAsync(deviceId, ruleId);
            res.AssertOnError();
            var data = res.ExtractContentDataAs<TableStorageResponse<DeviceRule>>();
            Assert.Equal(data, tableResp);
        }

        #region IDisposable Support
        private bool disposedValue = false; // To detect redundant calls

        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                    deviceRulesApiController.Dispose();
                }

                // TODO: free unmanaged resources (unmanaged objects) and override a finalizer below.
                // TODO: set large fields to null.

                disposedValue = true;
            }
        }

        // TODO: override a finalizer only if Dispose(bool disposing) above has code to free unmanaged resources.
        // ~DeviceRulesApiControllerTests() {
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