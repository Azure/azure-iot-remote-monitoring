using System;
using System.Collections.Generic;
using System.Linq;
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
    public class DeviceListColumnsRepositoryTests
    {
        private readonly IFixture fixture;
        private readonly Mock<IConfigurationProvider> _configurationProviderMock;
        private readonly Mock<IAzureTableStorageClient> _tableStorageClientMock;
        private readonly DeviceListColumnsRepository deviceListColumnsRepository;

        public DeviceListColumnsRepositoryTests()
        {
            fixture = new Fixture();
            fixture.Customize(new AutoConfiguredMoqCustomization());
            _configurationProviderMock = new Mock<IConfigurationProvider>();
            _configurationProviderMock.Setup(x => x.GetConfigurationSettingValue(It.IsNotNull<string>()))
                .ReturnsUsingFixture(fixture);
            _tableStorageClientMock = new Mock<IAzureTableStorageClient>();
            var tableStorageClientFactory = new AzureTableStorageClientFactory(_tableStorageClientMock.Object);
            deviceListColumnsRepository = new DeviceListColumnsRepository(_configurationProviderMock.Object,
                tableStorageClientFactory);
        }

        [Fact]
        public async void SaveAsyncTest()
        {
            var userId = fixture.Create<string>();
            var newColumns = fixture.Create<IEnumerable<DeviceListColumns>>();
            DeviceListColumnsTableEntity tableEntity = null;
            var tableEntities = new List<DeviceListColumnsTableEntity>();

            var resp = new TableStorageResponse<IEnumerable<DeviceListColumns>>
            {
                Entity = newColumns,
                Status = TableStorageResponseStatus.Successful
            };
            _tableStorageClientMock.Setup(x => x.DoTableInsertOrReplaceAsync(It.IsNotNull<DeviceListColumnsTableEntity>(),
                It.IsNotNull<Func<DeviceListColumnsTableEntity, IEnumerable<DeviceListColumns>>>()))
                .Callback<DeviceListColumnsTableEntity, Func<DeviceListColumnsTableEntity, IEnumerable<DeviceListColumns>>>(
                    (entity, func) => {
                        tableEntity = entity;
                        tableEntities.Add(tableEntity);
                    })
                .ReturnsAsync(resp);
            var ret = await deviceListColumnsRepository.SaveAsync(userId, newColumns);
            Assert.True(ret);
            Assert.NotNull(tableEntity);
        }

        [Fact]
        public async void GetAsyncTest()
        {
            var userId = fixture.Create<string>();
            var columns = fixture.Create<IEnumerable<DeviceListColumns>>();
            var tableResult = new TableResult()
            {
                Result = new DeviceListColumnsTableEntity()
                {
                    Columns = JsonConvert.SerializeObject(columns)
                }
            };
            
            _tableStorageClientMock.Setup(x => x.ExecuteAsync(It.IsNotNull<TableOperation>())).ReturnsAsync(tableResult);
            var ret = await deviceListColumnsRepository.GetAsync(userId);
            Assert.NotNull(ret);
            Assert.Equal(columns.Count(), ret.Count());
            Assert.Equal(columns.First().Name, ret.First().Name);
        }
    }
}
