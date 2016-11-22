using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Azure.Devices.Applications.RemoteMonitoring.Common.Configurations;
using Microsoft.Azure.Devices.Applications.RemoteMonitoring.Common.Helpers;
using Microsoft.Azure.Devices.Applications.RemoteMonitoring.Common.Models;
using Microsoft.Azure.Devices.Applications.RemoteMonitoring.DeviceAdmin.Infrastructure.Repository;
using Microsoft.Azure.Devices.Applications.RemoteMonitoring.DeviceAdmin.Infrastructure.Models;
using Microsoft.WindowsAzure.Storage.Table;
using Moq;
using Ploeh.AutoFixture;
using Ploeh.AutoFixture.AutoMoq;
using Xunit;

namespace Microsoft.Azure.Devices.Applications.RemoteMonitoring.UnitTests.Infrastructure
{
    public class DeviceListQueryRepositoryTests
    {
        private readonly IFixture fixture;
        private readonly Mock<IConfigurationProvider> _configurationProviderMock;
        private readonly Mock<IAzureTableStorageClient> _tableStorageClientMock;
        private readonly DeviceListQueryRepository deviceListQueryRepository;

        public DeviceListQueryRepositoryTests()
        {
            fixture = new Fixture();
            fixture.Customize(new AutoConfiguredMoqCustomization());
            _configurationProviderMock = new Mock<IConfigurationProvider>();
            _configurationProviderMock.Setup(x => x.GetConfigurationSettingValue(It.IsNotNull<string>()))
                .ReturnsUsingFixture(fixture);
            _tableStorageClientMock = new Mock<IAzureTableStorageClient>();
            var tableStorageClientFactory = new AzureTableStorageClientFactory(_tableStorageClientMock.Object);
            deviceListQueryRepository = new DeviceListQueryRepository(_configurationProviderMock.Object,
                tableStorageClientFactory);
        }

        [Fact]
        public async void CheckQueryNameAsyncTest()
        {
            var tableEntities = fixture.CreateMany<DeviceListQueryTableEntity>(1);
            _tableStorageClientMock.Setup(x => x.ExecuteQueryAsync(It.IsNotNull<TableQuery<DeviceListQueryTableEntity>>()))
                .ReturnsAsync(tableEntities);
            Assert.True(await deviceListQueryRepository.CheckQueryNameAsync(tableEntities.First().Name));

            _tableStorageClientMock.Setup(x => x.ExecuteQueryAsync(It.IsNotNull<TableQuery<DeviceListQueryTableEntity>>()))
               .ReturnsAsync(new List<DeviceListQueryTableEntity>());
            Assert.False(await deviceListQueryRepository.CheckQueryNameAsync(tableEntities.First().Name));
        }

        [Fact]
        public async void GetQueryAsyncTest()
        {
            var tableEntities = fixture.CreateMany<DeviceListQueryTableEntity>(1);
            tableEntities.First().Filters = "[{'ColumnName': 'Status', 'FilterType': 'EQ', 'FilterValue': 'Enabled'}]";
            _tableStorageClientMock.Setup(x => x.ExecuteQueryAsync(It.IsNotNull<TableQuery<DeviceListQueryTableEntity>>()))
                .ReturnsAsync(tableEntities);
            var ret = await deviceListQueryRepository.GetQueryAsync(tableEntities.First().Name);
            Assert.Equal(ret.Name, tableEntities.First().Name);

            _tableStorageClientMock.Setup(x => x.ExecuteQueryAsync(It.IsNotNull<TableQuery<DeviceListQueryTableEntity>>()))
               .ReturnsAsync(new List<DeviceListQueryTableEntity>());
            ret = await deviceListQueryRepository.GetQueryAsync("any");
            Assert.Null(ret);
        }

        [Fact]
        public async void SaveQueryAsyncTest()
        {
            var newQuery = fixture.Create<DeviceListQuery>();
            DeviceListQueryTableEntity tableEntity = null;
            var tableEntities = new List<DeviceListQueryTableEntity>();
            newQuery.Filters.ForEach(f => f.FilterType = FilterType.EQ);
            var resp = new TableStorageResponse<DeviceListQuery>
            {
                Entity = newQuery,
                Status = TableStorageResponseStatus.Successful
            };
            _tableStorageClientMock.Setup(
                x =>
                    x.DoTableInsertOrReplaceAsync(It.IsNotNull<DeviceListQueryTableEntity>(),
                        It.IsNotNull<Func<DeviceListQueryTableEntity, DeviceListQuery>>()))
                .Callback<DeviceListQueryTableEntity, Func<DeviceListQueryTableEntity, DeviceListQuery>>(
                        (entity, func) => {
                            tableEntity = entity;
                            tableEntities.Add(tableEntity);
                        })
                .ReturnsAsync(resp);
            var ret = await deviceListQueryRepository.SaveQueryAsync(newQuery, true);
            Assert.True(ret);
            Assert.NotNull(tableEntity);
            _tableStorageClientMock.Setup(x => x.ExecuteQueryAsync(It.IsNotNull<TableQuery<DeviceListQueryTableEntity>>()))
               .ReturnsAsync(tableEntities);
            ret = await deviceListQueryRepository.SaveQueryAsync(newQuery, false);
            Assert.False(ret);
        }

        [Fact]
        public async void TouchQueryAsyncTest()
        {
            var query = fixture.Create<DeviceListQuery>();
            DeviceListQueryTableEntity tableEntity = null;

            var resp = new TableStorageResponse<DeviceListQuery>
            {
                Entity = query,
                Status = TableStorageResponseStatus.Successful
            };
            _tableStorageClientMock.Setup(
                x =>
                    x.DoTouchAsync(It.IsNotNull<DeviceListQueryTableEntity>(),
                        It.IsNotNull<Func<DeviceListQueryTableEntity, DeviceListQuery>>()))
                .Callback<DeviceListQueryTableEntity, Func<DeviceListQueryTableEntity, DeviceListQuery>>(
                        (entity, func) => tableEntity = entity)
                .ReturnsAsync(resp);
            var ret = await deviceListQueryRepository.TouchQueryAsync(query.Name);
            Assert.True(ret);
            Assert.NotNull(tableEntity);

            resp = new TableStorageResponse<DeviceListQuery>
            {
                Entity = null,
                Status = TableStorageResponseStatus.NotFound
            };
            _tableStorageClientMock.Setup(
                x =>
                    x.DoTouchAsync(It.IsNotNull<DeviceListQueryTableEntity>(),
                        It.IsNotNull<Func<DeviceListQueryTableEntity, DeviceListQuery>>()))
                .Callback<DeviceListQueryTableEntity, Func<DeviceListQueryTableEntity, DeviceListQuery>>(
                        (entity, func) => tableEntity = null)
                .ReturnsAsync(resp);
            ret = await deviceListQueryRepository.TouchQueryAsync(query.Name);
            Assert.False(ret);
            Assert.Null(tableEntity);

            ret = await deviceListQueryRepository.TouchQueryAsync(null);
            Assert.False(ret);
        }

        [Fact]
        public async void DeleteQueryAsyncTest()
        {
            var query = fixture.Create<DeviceListQuery>();
            DeviceListQueryTableEntity tableEntity = null;

            var resp = new TableStorageResponse<DeviceListQuery>
            {
                Entity = query,
                Status = TableStorageResponseStatus.Successful
            };
            _tableStorageClientMock.Setup(
                x =>
                    x.DoDeleteAsync(It.IsNotNull<DeviceListQueryTableEntity>(),
                        It.IsNotNull<Func<DeviceListQueryTableEntity, DeviceListQuery>>()))
                .Callback<DeviceListQueryTableEntity, Func<DeviceListQueryTableEntity, DeviceListQuery>>(
                        (entity, func) => tableEntity = entity)
                .ReturnsAsync(resp);
            var ret = await deviceListQueryRepository.DeleteQueryAsync(query.Name);
            Assert.True(ret);
            Assert.NotNull(tableEntity);

            resp = new TableStorageResponse<DeviceListQuery>
            {
                Entity = null,
                Status = TableStorageResponseStatus.NotFound
            };
            _tableStorageClientMock.Setup(
                x =>
                    x.DoDeleteAsync(It.IsNotNull<DeviceListQueryTableEntity>(),
                        It.IsNotNull<Func<DeviceListQueryTableEntity, DeviceListQuery>>()))
                .Callback<DeviceListQueryTableEntity, Func<DeviceListQueryTableEntity, DeviceListQuery>>(
                        (entity, func) => tableEntity = null)
                .ReturnsAsync(resp);
            ret = await deviceListQueryRepository.DeleteQueryAsync(query.Name);
            Assert.False(ret);
            Assert.Null(tableEntity);
        }

        [Fact]
        public async void GetRecentQueriesAsyncTest()
        {
            var tableEntities = fixture.CreateMany<DeviceListQueryTableEntity>(1);
            tableEntities.First().Filters = "[{'ColumnName': 'Status', 'FilterType': 'EQ', 'FilterValue': 'Enabled'}]";
            _tableStorageClientMock.Setup(x => x.ExecuteQueryAsync(It.IsNotNull<TableQuery<DeviceListQueryTableEntity>>()))
                .ReturnsAsync(tableEntities);
            var ret = await deviceListQueryRepository.GetRecentQueriesAsync();
            Assert.Equal(1, ret.Count());
            Assert.Equal(1, ret.First().Filters.Count());
            Assert.Equal("Status", ret.First().Filters.First().ColumnName);
            Assert.Equal(FilterType.EQ, ret.First().Filters.First().FilterType);
            Assert.Equal("Enabled", ret.First().Filters.First().FilterValue);

            tableEntities = fixture.CreateMany<DeviceListQueryTableEntity>(40);
            int max = 30;
            _tableStorageClientMock.Setup(x => x.ExecuteQueryAsync(It.IsNotNull<TableQuery<DeviceListQueryTableEntity>>()))
                .ReturnsAsync(tableEntities.OrderByDescending(e => e.Timestamp).Take(max));
            ret = await deviceListQueryRepository.GetRecentQueriesAsync(max);
            Assert.Equal(max, ret.Count());
            Assert.Equal(tableEntities.OrderByDescending(e => e.Timestamp).Take(max).Select(e => e.Name).ToArray(), ret.Select(e => e.Name).ToArray());
        }

        [Fact]
        public async void GetQueryNameListAsyncTest()
        {
            var tableEntities = fixture.CreateMany<DeviceListQueryTableEntity>(40);
            tableEntities.Select(e => e.Filters = "[{'ColumnName': 'Status', 'FilterType': 'EQ', 'FilterValue': 'Enabled'}]");
            _tableStorageClientMock.Setup(x => x.ExecuteQueryAsync(It.IsNotNull<TableQuery<DeviceListQueryTableEntity>>()))
                .ReturnsAsync(tableEntities.OrderBy(e => e.Name));
            var ret = await deviceListQueryRepository.GetQueryNameListAsync();
            Assert.Equal(40, ret.Count());
            Assert.Equal(tableEntities.OrderBy(e => e.Name).Select(e => e.Name).ToArray(), ret.ToArray());
        }
    }
}
