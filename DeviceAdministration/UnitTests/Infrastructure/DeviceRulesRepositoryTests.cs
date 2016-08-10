using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Azure.Devices.Applications.RemoteMonitoring.Common.Configurations;
using Microsoft.Azure.Devices.Applications.RemoteMonitoring.Common.Helpers;
using Microsoft.Azure.Devices.Applications.RemoteMonitoring.Common.Models;
using Microsoft.Azure.Devices.Applications.RemoteMonitoring.DeviceAdmin.Infrastructure.Models;
using Microsoft.Azure.Devices.Applications.RemoteMonitoring.DeviceAdmin.Infrastructure.Repository;
using Microsoft.WindowsAzure.Storage.Table;
using Moq;
using Newtonsoft.Json;
using Ploeh.AutoFixture;
using Ploeh.AutoFixture.AutoMoq;
using Xunit;

namespace Microsoft.Azure.Devices.Applications.RemoteMonitoring.UnitTests.Infrastructure
{
    public class DeviceRulesRepositoryTests
    {
        private readonly Mock<IBlobStorageClient> _blobClientMock;
        private readonly Mock<IAzureTableStorageClient> _tableStorageClientMock;
        private readonly DeviceRulesRepository deviceRulesRepository;
        private readonly IFixture fixture;

        public DeviceRulesRepositoryTests()
        {
            fixture = new Fixture();
            var configProviderMock = new Mock<IConfigurationProvider>();
            _tableStorageClientMock = new Mock<IAzureTableStorageClient>();
            _blobClientMock = new Mock<IBlobStorageClient>();
            configProviderMock.Setup(x => x.GetConfigurationSettingValue(It.IsNotNull<string>()))
                .ReturnsUsingFixture(fixture);
            var tableStorageClientFactory = new AzureTableStorageClientFactory(_tableStorageClientMock.Object);
            var blobClientFactory = new BlobStorageClientFactory(_blobClientMock.Object);
            deviceRulesRepository = new DeviceRulesRepository(configProviderMock.Object, tableStorageClientFactory,
                blobClientFactory);
        }

        [Fact]
        public async void GetAllRulesAsyncTest()
        {
            var ruleEntities = fixture.Create<List<DeviceRuleTableEntity>>();
            _tableStorageClientMock.Setup(x => x.ExecuteQueryAsync(It.IsNotNull<TableQuery<DeviceRuleTableEntity>>()))
                .ReturnsAsync(ruleEntities);
            var deviceRules = await deviceRulesRepository.GetAllRulesAsync();
            Assert.NotNull(deviceRules);
            Assert.Equal(ruleEntities.Count, deviceRules.Count);
            Assert.Equal(ruleEntities[0].DataField, deviceRules[0].DataField);
            Assert.Equal(ruleEntities[0].DeviceId, deviceRules[0].DeviceID);
            Assert.Equal(ruleEntities[0].Threshold, deviceRules[0].Threshold);
            Assert.Equal(">", deviceRules[0].Operator);
            Assert.Equal(ruleEntities[0].RuleOutput, deviceRules[0].RuleOutput);
            Assert.Equal(ruleEntities[0].ETag, deviceRules[0].Etag);
        }

        [Fact]
        public async void GetDeviceRuleAsyncTest()
        {
            var ruleEntity = fixture.Create<DeviceRuleTableEntity>();
            _tableStorageClientMock.Setup(x => x.Execute(It.IsNotNull<TableOperation>()))
                .Returns(new TableResult {Result = ruleEntity});
            var ret = await deviceRulesRepository.GetDeviceRuleAsync(ruleEntity.DeviceId, ruleEntity.RuleId);
            Assert.NotNull(ret);
            Assert.Equal(ruleEntity.DeviceId, ret.DeviceID);
            Assert.Equal(ruleEntity.DataField, ret.DataField);
            Assert.Equal(ruleEntity.Threshold, ret.Threshold);
            Assert.Equal(">", ret.Operator);
            Assert.Equal(ruleEntity.RuleOutput, ret.RuleOutput);
            Assert.Equal(ruleEntity.ETag, ret.Etag);
        }

        [Fact]
        public async void GetAllRulesForDeviceAsyncTest()
        {
            var ruleEntities = fixture.Create<List<DeviceRuleTableEntity>>();
            ruleEntities.ForEach(x => x.DeviceId = "DeviceXXXId");
            _tableStorageClientMock.Setup(x => x.ExecuteQueryAsync(It.IsNotNull<TableQuery<DeviceRuleTableEntity>>()))
                .ReturnsAsync(ruleEntities);
            var deviceRules = await deviceRulesRepository.GetAllRulesForDeviceAsync("DeviceXXXId");
            Assert.NotNull(deviceRules);
            Assert.Equal(ruleEntities.Count, deviceRules.Count);
            Assert.Equal(ruleEntities[0].DataField, deviceRules[0].DataField);
            Assert.Equal(ruleEntities[0].DeviceId, deviceRules[0].DeviceID);
            Assert.Equal(ruleEntities[0].Threshold, deviceRules[0].Threshold);
            Assert.Equal(">", deviceRules[0].Operator);
            Assert.Equal(ruleEntities[0].RuleOutput, deviceRules[0].RuleOutput);
            Assert.Equal(ruleEntities[0].ETag, deviceRules[0].Etag);
        }

        [Fact]
        public async void SaveDeviceRuleAsyncTest()
        {
            var newRule = fixture.Create<DeviceRule>();
            DeviceRuleTableEntity tableEntity = null;
            var resp = new TableStorageResponse<DeviceRule>
            {
                Entity = newRule,
                Status = TableStorageResponseStatus.Successful
            };
            _tableStorageClientMock.Setup(
                x =>
                    x.DoTableInsertOrReplaceAsync(It.IsNotNull<DeviceRuleTableEntity>(),
                        It.IsNotNull<Func<DeviceRuleTableEntity, DeviceRule>>()))
                .Callback<DeviceRuleTableEntity, Func<DeviceRuleTableEntity, DeviceRule>>(
                    (entity, func) => tableEntity = entity)
                .ReturnsAsync(resp);
            var tableEntities = fixture.Create<List<DeviceRuleTableEntity>>();
            _tableStorageClientMock.Setup(x => x.ExecuteQueryAsync(It.IsNotNull<TableQuery<DeviceRuleTableEntity>>()))
                .ReturnsAsync(tableEntities);
            string blobEntititesStr = null;
            _blobClientMock.Setup(x => x.UploadTextAsync(It.IsNotNull<string>(), It.IsNotNull<string>()))
                .Callback<string, string>((name, blob) => blobEntititesStr = blob)
                .Returns(Task.FromResult(true));
            var ret = await deviceRulesRepository.SaveDeviceRuleAsync(newRule);
            Assert.NotNull(ret);
            Assert.Equal(resp, ret);
            Assert.NotNull(tableEntity);
            Assert.Equal(newRule.DeviceID, tableEntity.DeviceId);
            Assert.NotNull(blobEntititesStr);
        }

        [Fact]
        public async void DeleteDeviceRuleAsyncTest()
        {
            var newRule = fixture.Create<DeviceRule>();
            DeviceRuleTableEntity tableEntity = null;
            var resp = new TableStorageResponse<DeviceRule>
            {
                Entity = newRule,
                Status = TableStorageResponseStatus.Successful
            };
            _tableStorageClientMock.Setup(
                x =>
                    x.DoDeleteAsync(It.IsNotNull<DeviceRuleTableEntity>(),
                        It.IsNotNull<Func<DeviceRuleTableEntity, DeviceRule>>()))
                .Callback<DeviceRuleTableEntity, Func<DeviceRuleTableEntity, DeviceRule>>(
                    (entity, func) => tableEntity = entity)
                .ReturnsAsync(resp);
            var tableEntities = fixture.Create<List<DeviceRuleTableEntity>>();
            _tableStorageClientMock.Setup(x => x.ExecuteQueryAsync(It.IsNotNull<TableQuery<DeviceRuleTableEntity>>()))
                .ReturnsAsync(tableEntities);
            string blobEntititesStr = null;
            _blobClientMock.Setup(x => x.UploadTextAsync(It.IsNotNull<string>(), It.IsNotNull<string>()))
                .Callback<string, string>((name, blob) => blobEntititesStr = blob)
                .Returns(Task.FromResult(true));
            var ret = await deviceRulesRepository.DeleteDeviceRuleAsync(newRule);
            Assert.NotNull(ret);
            Assert.Equal(resp, ret);
            Assert.NotNull(tableEntity);
            Assert.Equal(newRule.DeviceID, tableEntity.DeviceId);
            Assert.NotNull(blobEntititesStr);
        }
    }
}