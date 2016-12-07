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
    public class DeviceListFilterRepositoryTests
    {
        private readonly IFixture fixture;
        private readonly Mock<IConfigurationProvider> _configurationProviderMock;
        private readonly Mock<IAzureTableStorageClient> _tableStorageClientMock;
        private readonly DeviceListFilterRepository deviceListFilterRepository;

        public DeviceListFilterRepositoryTests()
        {
            fixture = new Fixture();
            fixture.Customize(new AutoConfiguredMoqCustomization());
            _configurationProviderMock = new Mock<IConfigurationProvider>();
            _configurationProviderMock.Setup(x => x.GetConfigurationSettingValue(It.IsNotNull<string>()))
                .ReturnsUsingFixture(fixture);
            _tableStorageClientMock = new Mock<IAzureTableStorageClient>();
            var tableStorageClientFactory = new AzureTableStorageClientFactory(_tableStorageClientMock.Object);
            deviceListFilterRepository = new DeviceListFilterRepository(_configurationProviderMock.Object,
                tableStorageClientFactory);
        }

        [Fact]
        public async void CheckFilterNameAsyncTest()
        {
            var tableEntities = fixture.CreateMany<DeviceListFilterTableEntity>(1);
            _tableStorageClientMock.Setup(x => x.ExecuteQueryAsync(It.IsNotNull<TableQuery<DeviceListFilterTableEntity>>()))
                .ReturnsAsync(tableEntities);
            Assert.True(await deviceListFilterRepository.CheckFilterNameAsync(tableEntities.First().Name));

            _tableStorageClientMock.Setup(x => x.ExecuteQueryAsync(It.IsNotNull<TableQuery<DeviceListFilterTableEntity>>()))
               .ReturnsAsync(new List<DeviceListFilterTableEntity>());
            Assert.False(await deviceListFilterRepository.CheckFilterNameAsync(tableEntities.First().Name));
        }

        [Fact]
        public async void GetFilterAsyncTest()
        {
            var tableEntities = new List<DeviceListFilterTableEntity>();
            tableEntities.Add(new DeviceListFilterTableEntity("filterId", "filterName") {
                Clauses = "[{'ColumnName': 'Status', 'ClauseType': 'EQ', 'ClauseValue': 'Enabled'}]",
            });

            _tableStorageClientMock.Setup(x => x.ExecuteQueryAsync(It.IsNotNull<TableQuery<DeviceListFilterTableEntity>>()))
                .ReturnsAsync(tableEntities);
            var ret = await deviceListFilterRepository.GetFilterAsync(tableEntities.First().PartitionKey);
            Assert.Equal(ret.Id, tableEntities.First().Id);

            _tableStorageClientMock.Setup(x => x.ExecuteQueryAsync(It.IsNotNull<TableQuery<DeviceListFilterTableEntity>>()))
               .ReturnsAsync(new List<DeviceListFilterTableEntity>());
            ret = await deviceListFilterRepository.GetFilterAsync("any");
            Assert.Null(ret);
        }

        [Fact]
        public async void SaveFilterAsyncTest()
        {
            var newFilter = fixture.Create<DeviceListFilter>();
            DeviceListFilterTableEntity tableEntity = null;
            var tableEntities = new List<DeviceListFilterTableEntity>();
            newFilter.Clauses.ForEach(f => f.ClauseType = ClauseType.EQ);
            var resp = new TableStorageResponse<DeviceListFilter>
            {
                Entity = newFilter,
                Status = TableStorageResponseStatus.Successful
            };
            _tableStorageClientMock.Setup(
                x =>
                    x.DoTableInsertOrReplaceAsync(It.IsNotNull<DeviceListFilterTableEntity>(),
                        It.IsNotNull<Func<DeviceListFilterTableEntity, DeviceListFilter>>()))
                .Callback<DeviceListFilterTableEntity, Func<DeviceListFilterTableEntity, DeviceListFilter>>(
                        (entity, func) => {
                            tableEntity = entity;
                            tableEntities.Add(tableEntity);
                        })
                .ReturnsAsync(resp);
            var ret = await deviceListFilterRepository.SaveFilterAsync(newFilter, true);
            Assert.True(ret);
            Assert.NotNull(tableEntity);
            _tableStorageClientMock.Setup(x => x.ExecuteQueryAsync(It.IsNotNull<TableQuery<DeviceListFilterTableEntity>>()))
               .ReturnsAsync(tableEntities);
            ret = await deviceListFilterRepository.SaveFilterAsync(newFilter, false);
            Assert.False(ret);
        }

        [Fact]
        public async void TouchFilterAsyncTest()
        {
            var filter = fixture.Create<DeviceListFilter>();
            var tableEntities = fixture.CreateMany<DeviceListFilterTableEntity>(1);
            tableEntities.First().Clauses = "[{'ColumnName': 'Status', 'ClauseType': 'EQ', 'ClauseValue': 'Enabled'}]";
            DeviceListFilterTableEntity tableEntity = tableEntities.First();

            var resp = new TableStorageResponse<DeviceListFilter>
            {
                Entity = filter,
                Status = TableStorageResponseStatus.Successful
            };

            _tableStorageClientMock.Setup(x => x.ExecuteQueryAsync(It.IsNotNull<TableQuery<DeviceListFilterTableEntity>>()))
                .ReturnsAsync(tableEntities);

            _tableStorageClientMock.Setup(
                x =>
                    x.DoTouchAsync(It.IsNotNull<DeviceListFilterTableEntity>(),
                        It.IsNotNull<Func<DeviceListFilterTableEntity, DeviceListFilter>>()))
                .Callback<DeviceListFilterTableEntity, Func<DeviceListFilterTableEntity, DeviceListFilter>>(
                        (entity, func) => tableEntity = entity)
                .ReturnsAsync(resp);
            var ret = await deviceListFilterRepository.TouchFilterAsync(filter.Id);
            Assert.True(ret);
            Assert.NotNull(tableEntity);

            resp = new TableStorageResponse<DeviceListFilter>
            {
                Entity = null,
                Status = TableStorageResponseStatus.NotFound
            };
            _tableStorageClientMock.Setup(
                x =>
                    x.DoTouchAsync(It.IsNotNull<DeviceListFilterTableEntity>(),
                        It.IsNotNull<Func<DeviceListFilterTableEntity, DeviceListFilter>>()))
                .Callback<DeviceListFilterTableEntity, Func<DeviceListFilterTableEntity, DeviceListFilter>>(
                        (entity, func) => tableEntity = null)
                .ReturnsAsync(resp);
            ret = await deviceListFilterRepository.TouchFilterAsync(filter.Id);
            Assert.False(ret);
            Assert.Null(tableEntity);

            ret = await deviceListFilterRepository.TouchFilterAsync(null);
            Assert.False(ret);
        }

        [Fact]
        public async void DeleteFilterAsyncTest()
        {
            var filter = fixture.Create<DeviceListFilter>();
            var tableEntities = fixture.CreateMany<DeviceListFilterTableEntity>(1);
            tableEntities.First().Clauses = "[{'ColumnName': 'Status', 'ClauseType': 'EQ', 'ClauseValue': 'Enabled'}]";
            DeviceListFilterTableEntity tableEntity = tableEntities.First();

            var resp = new TableStorageResponse<DeviceListFilter>
            {
                Entity = filter,
                Status = TableStorageResponseStatus.Successful
            };

            _tableStorageClientMock.Setup(x => x.ExecuteQueryAsync(It.IsNotNull<TableQuery<DeviceListFilterTableEntity>>()))
                .ReturnsAsync(tableEntities);

            _tableStorageClientMock.Setup(
                x =>
                    x.DoDeleteAsync(It.IsNotNull<DeviceListFilterTableEntity>(),
                        It.IsNotNull<Func<DeviceListFilterTableEntity, DeviceListFilter>>()))
                .Callback<DeviceListFilterTableEntity, Func<DeviceListFilterTableEntity, DeviceListFilter>>(
                        (entity, func) => tableEntity = entity)
                .ReturnsAsync(resp);
            var ret = await deviceListFilterRepository.DeleteFilterAsync(filter.Id);
            Assert.True(ret);
            Assert.NotNull(tableEntity);

            resp = new TableStorageResponse<DeviceListFilter>
            {
                Entity = null,
                Status = TableStorageResponseStatus.NotFound
            };

            _tableStorageClientMock.Setup(
                x =>
                    x.DoDeleteAsync(It.IsNotNull<DeviceListFilterTableEntity>(),
                        It.IsNotNull<Func<DeviceListFilterTableEntity, DeviceListFilter>>()))
                .Callback<DeviceListFilterTableEntity, Func<DeviceListFilterTableEntity, DeviceListFilter>>(
                        (entity, func) => tableEntity = null)
                .ReturnsAsync(resp);
            ret = await deviceListFilterRepository.DeleteFilterAsync(filter.Id);
            Assert.False(ret);
            Assert.Null(tableEntity);

            _tableStorageClientMock.Setup(x => x.ExecuteQueryAsync(It.IsNotNull<TableQuery<DeviceListFilterTableEntity>>()))
                .ReturnsAsync(new List<DeviceListFilterTableEntity>());
            ret = await deviceListFilterRepository.DeleteFilterAsync(filter.Id);
            Assert.False(ret);
        }

        [Fact]
        public async void GetRecentFiltersAsyncTest()
        {
            var tableEntities = fixture.CreateMany<DeviceListFilterTableEntity>(1);
            tableEntities.First().Clauses = "[{'ColumnName': 'Status', 'ClauseType': 'EQ', 'ClauseValue': 'Enabled'}]";
            _tableStorageClientMock.Setup(x => x.ExecuteQueryAsync(It.IsNotNull<TableQuery<DeviceListFilterTableEntity>>()))
                .ReturnsAsync(tableEntities);
            var ret = await deviceListFilterRepository.GetRecentFiltersAsync();
            Assert.Equal(1, ret.Count());
            Assert.Equal(1, ret.First().Clauses.Count());
            Assert.Equal("Status", ret.First().Clauses.First().ColumnName);
            Assert.Equal(ClauseType.EQ, ret.First().Clauses.First().ClauseType);
            Assert.Equal("Enabled", ret.First().Clauses.First().ClauseValue);

            tableEntities = fixture.CreateMany<DeviceListFilterTableEntity>(40);
            int max = 30;
            _tableStorageClientMock.Setup(x => x.ExecuteQueryAsync(It.IsNotNull<TableQuery<DeviceListFilterTableEntity>>()))
                .ReturnsAsync(tableEntities.OrderByDescending(e => e.Timestamp).Take(max));
            ret = await deviceListFilterRepository.GetRecentFiltersAsync(max);
            Assert.Equal(max, ret.Count());
            Assert.Equal(tableEntities.OrderByDescending(e => e.Timestamp).Take(max).Select(e => e.Name).ToArray(), ret.Select(e => e.Name).ToArray());
        }

        [Fact]
        public async void GetFilterListAsyncTest()
        {
            var tableEntities = fixture.CreateMany<DeviceListFilterTableEntity>(40);
            tableEntities.Select(e => e.Clauses = "[{'ColumnName': 'Status', 'ClauseType': 'EQ', 'ClauseValue': 'Enabled'}]");
            _tableStorageClientMock.Setup(x => x.ExecuteQueryAsync(It.IsNotNull<TableQuery<DeviceListFilterTableEntity>>()))
                .ReturnsAsync(tableEntities.OrderBy(e => e.Name));
            var ret = await deviceListFilterRepository.GetFilterListAsync(0, 1000);
            Assert.Equal(40, ret.Count());
            Assert.Equal(tableEntities.OrderBy(e => e.Name).Select(e => new string[] { e.PartitionKey, e.Name }).ToArray(), ret.Select(e => new string[] { e.Id, e.Name }).ToArray());
        }
    }
}
