using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Azure.Devices.Applications.RemoteMonitoring.Common.Configurations;
using Microsoft.Azure.Devices.Applications.RemoteMonitoring.Common.Helpers;
using Microsoft.Azure.Devices.Applications.RemoteMonitoring.Common.Models;
using Microsoft.Azure.Devices.Applications.RemoteMonitoring.DeviceAdmin.Infrastructure;
using Microsoft.Azure.Devices.Applications.RemoteMonitoring.DeviceAdmin.Infrastructure.Exceptions;
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
        private readonly Mock<IAzureTableStorageClient> _filterTableStorageClientMock, _clauseTableStorageClientMock;
        private readonly DeviceListFilterRepository deviceListFilterRepository;

        public DeviceListFilterRepositoryTests()
        {
            fixture = new Fixture();
            fixture.Customize(new AutoConfiguredMoqCustomization());
            _configurationProviderMock = new Mock<IConfigurationProvider>();
            _configurationProviderMock.Setup(x => x.GetConfigurationSettingValue(It.IsNotNull<string>()))
                .ReturnsUsingFixture(fixture);
            _filterTableStorageClientMock = new Mock<IAzureTableStorageClient>();
            _clauseTableStorageClientMock = new Mock<IAzureTableStorageClient>();
            var filterTableStorageClientFactory = new AzureTableStorageClientFactory(_filterTableStorageClientMock.Object);
            var clauseTableStorageClientFactory = new AzureTableStorageClientFactory(_clauseTableStorageClientMock.Object);
            deviceListFilterRepository = new DeviceListFilterRepository(_configurationProviderMock.Object,
                filterTableStorageClientFactory, clauseTableStorageClientFactory);
        }

        [Fact]
        public async void CheckFilterNameAsyncTest()
        {
            var tableEntities = fixture.CreateMany<DeviceListFilterTableEntity>(1);
            _filterTableStorageClientMock.Setup(x => x.ExecuteQueryAsync(It.IsNotNull<TableQuery<DeviceListFilterTableEntity>>()))
                .ReturnsAsync(tableEntities);
            Assert.True(await deviceListFilterRepository.CheckFilterNameAsync(tableEntities.First().Name));

            _filterTableStorageClientMock.Setup(x => x.ExecuteQueryAsync(It.IsNotNull<TableQuery<DeviceListFilterTableEntity>>()))
               .ReturnsAsync(new List<DeviceListFilterTableEntity>());
            Assert.False(await deviceListFilterRepository.CheckFilterNameAsync(tableEntities.First().Name));
        }

        [Fact]
        public async void GetFilterAsyncTest()
        {
            var filter = fixture.Create<DeviceListFilter>();
            DeviceListFilterTableEntity tableEntity = new DeviceListFilterTableEntity(filter);
            var tableEntities = new List<DeviceListFilterTableEntity>();
            tableEntities.Add(tableEntity);

            _filterTableStorageClientMock.Setup(x => x.ExecuteQueryAsync(It.IsNotNull<TableQuery<DeviceListFilterTableEntity>>()))
                .ReturnsAsync(tableEntities);
            var ret = await deviceListFilterRepository.GetFilterAsync(tableEntities.First().PartitionKey);
            Assert.Equal(ret.Id, tableEntities.First().Id);

            _filterTableStorageClientMock.Setup(x => x.ExecuteQueryAsync(It.IsNotNull<TableQuery<DeviceListFilterTableEntity>>()))
               .ReturnsAsync(new List<DeviceListFilterTableEntity>());
            ret = await deviceListFilterRepository.GetFilterAsync("any");
            Assert.Null(ret);
        }

        [Fact]
        public async void SaveFilterAsyncTest()
        {
            var newFilter = fixture.Create<DeviceListFilter>();
            var oldEntity = fixture.Create<DeviceListFilterTableEntity>();
            DeviceListFilterTableEntity tableEntity = null;
            var tableEntities = new List<DeviceListFilterTableEntity>();

            var resp = new TableStorageResponse<DeviceListFilter>
            {
                Entity = newFilter,
                Status = TableStorageResponseStatus.Successful
            };

            _filterTableStorageClientMock.Setup(x => x.ExecuteQueryAsync(It.IsNotNull<TableQuery<DeviceListFilterTableEntity>>()))
                .ReturnsAsync(tableEntities);
            _filterTableStorageClientMock.Setup(
                x =>
                    x.DoTableInsertOrReplaceAsync(It.IsNotNull<DeviceListFilterTableEntity>(),
                        It.IsNotNull<Func<DeviceListFilterTableEntity, DeviceListFilter>>()))
                .Callback<DeviceListFilterTableEntity, Func<DeviceListFilterTableEntity, DeviceListFilter>>(
                        (entity, func) =>
                        {
                            tableEntity = entity;
                            tableEntities.Add(tableEntity);
                        })
                .ReturnsAsync(resp);
            var ret = await deviceListFilterRepository.SaveFilterAsync(newFilter, true);
            Assert.NotNull(ret);
            Assert.NotNull(tableEntity);

            _filterTableStorageClientMock.Setup(x => x.ExecuteQueryAsync(It.IsNotNull<TableQuery<DeviceListFilterTableEntity>>()))
                .ReturnsAsync(tableEntities);
            ret = await deviceListFilterRepository.SaveFilterAsync(newFilter, false);
            Assert.Equal(ret.Id, newFilter.Id);
            Assert.Equal(ret.Name, newFilter.Name);
            Assert.Equal(ret.Clauses.Count, newFilter.Clauses.Count);

            tableEntity.Name = "changedName";
            _filterTableStorageClientMock.SetupSequence(x => x.ExecuteQueryAsync(It.IsNotNull<TableQuery<DeviceListFilterTableEntity>>()))
                .ReturnsAsync(tableEntities)
                .ReturnsAsync(new List<DeviceListFilterTableEntity>());
            _filterTableStorageClientMock.Setup(x => x.DoDeleteAsync(It.IsNotNull<DeviceListFilterTableEntity>(), It.IsAny<Func<DeviceListFilterTableEntity, DeviceListFilter>>()));
            ret = await deviceListFilterRepository.SaveFilterAsync(newFilter, true);

            resp = new TableStorageResponse<DeviceListFilter>
            {
                Entity = newFilter,
                Status = TableStorageResponseStatus.NotFound
            };
            _filterTableStorageClientMock.Setup(
                x =>
                    x.DoTableInsertOrReplaceAsync(It.IsNotNull<DeviceListFilterTableEntity>(),
                        It.IsNotNull<Func<DeviceListFilterTableEntity, DeviceListFilter>>()))
               .ReturnsAsync(resp);
            _filterTableStorageClientMock.SetupSequence(x => x.ExecuteQueryAsync(It.IsNotNull<TableQuery<DeviceListFilterTableEntity>>()))
                .ReturnsAsync(tableEntities)
                .ReturnsAsync(new List<DeviceListFilterTableEntity>());
            await Assert.ThrowsAnyAsync<FilterSaveException>(async () => await deviceListFilterRepository.SaveFilterAsync(newFilter, true));
        }

        [Fact]
        public async void TouchFilterAsyncTest()
        {
            var filter = fixture.Create<DeviceListFilter>();
            DeviceListFilterTableEntity tableEntity = new DeviceListFilterTableEntity(filter);
            var tableEntities = new List<DeviceListFilterTableEntity>();
            tableEntities.Add(tableEntity);

            var resp = new TableStorageResponse<DeviceListFilter>
            {
                Entity = filter,
                Status = TableStorageResponseStatus.Successful
            };

            _filterTableStorageClientMock.Setup(x => x.ExecuteQueryAsync(It.IsNotNull<TableQuery<DeviceListFilterTableEntity>>()))
                .ReturnsAsync(tableEntities);

            _filterTableStorageClientMock.Setup(
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
            _filterTableStorageClientMock.Setup(
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
            DeviceListFilterTableEntity tableEntity = new DeviceListFilterTableEntity(filter);
            var tableEntities = new List<DeviceListFilterTableEntity>();
            tableEntities.Add(tableEntity);

            var resp = new TableStorageResponse<DeviceListFilter>
            {
                Entity = filter,
                Status = TableStorageResponseStatus.Successful
            };

            _filterTableStorageClientMock.Setup(x => x.ExecuteQueryAsync(It.IsNotNull<TableQuery<DeviceListFilterTableEntity>>()))
                .ReturnsAsync(tableEntities);

            _filterTableStorageClientMock.Setup(
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

            _filterTableStorageClientMock.Setup(
                x =>
                    x.DoDeleteAsync(It.IsNotNull<DeviceListFilterTableEntity>(),
                        It.IsNotNull<Func<DeviceListFilterTableEntity, DeviceListFilter>>()))
                .Callback<DeviceListFilterTableEntity, Func<DeviceListFilterTableEntity, DeviceListFilter>>(
                        (entity, func) => tableEntity = null)
                .ReturnsAsync(resp);
            ret = await deviceListFilterRepository.DeleteFilterAsync(filter.Id);
            Assert.False(ret);
            Assert.Null(tableEntity);

            _filterTableStorageClientMock.Setup(x => x.ExecuteQueryAsync(It.IsNotNull<TableQuery<DeviceListFilterTableEntity>>()))
                .ReturnsAsync(new List<DeviceListFilterTableEntity>());
            ret = await deviceListFilterRepository.DeleteFilterAsync(filter.Id);
            Assert.True(ret);
        }

        [Fact]
        public async void GetRecentFiltersAsyncTest()
        {
            var filters = fixture.CreateMany<DeviceListFilter>(40);
            var tableEntities = filters.Select(f => new DeviceListFilterTableEntity(f));
            _filterTableStorageClientMock.Setup(x => x.ExecuteQueryAsync(It.IsNotNull<TableQuery<DeviceListFilterTableEntity>>()))
                .ReturnsAsync(tableEntities);
            var ret = await deviceListFilterRepository.GetRecentFiltersAsync();
            Assert.Equal(20, ret.Count());

            int max = 30;
            _filterTableStorageClientMock.Setup(x => x.ExecuteQueryAsync(It.IsNotNull<TableQuery<DeviceListFilterTableEntity>>()))
                .ReturnsAsync(tableEntities);
            ret = await deviceListFilterRepository.GetRecentFiltersAsync(max);
            var expectedNams = tableEntities.OrderByDescending(e => e.Timestamp).Take(max).Select(e => e.Name).ToArray();
            Assert.Equal(max, ret.Count());
            Assert.Equal(expectedNams, ret.Select(e => e.Name).ToArray());

            filters.Take(max).All(f => { f.IsTemporary = true; return true; });
            filters.Take(max + 1).All(f => { f.Name = Constants.UnnamedFilterName; return true; });
            tableEntities = filters.Select(f => new DeviceListFilterTableEntity(f));
            ret = await deviceListFilterRepository.GetRecentFiltersAsync(max, false);
            Assert.Equal(40 - max - 1, ret.Count());
            Assert.False(ret.Any(f => f.Name.Equals(Constants.UnnamedFilterName, StringComparison.InvariantCultureIgnoreCase)));
            ret = await deviceListFilterRepository.GetRecentFiltersAsync(max, true);
            Assert.Equal(40 - max - 1, ret.Count());
            Assert.False(ret.Any(f => f.Name.Equals(Constants.UnnamedFilterName, StringComparison.InvariantCultureIgnoreCase)));

            filters = fixture.CreateMany<DeviceListFilter>(40);
            tableEntities = filters.Select(f => new DeviceListFilterTableEntity(f));
            _filterTableStorageClientMock.Setup(x => x.ExecuteQueryAsync(It.IsNotNull<TableQuery<DeviceListFilterTableEntity>>()))
                .ReturnsAsync(tableEntities);
            filters.Take(max).All(f => { f.IsTemporary = true; return true; });
            filters.Take(max - 1).All(f => { f.Name = Constants.UnnamedFilterName; return true; });
            tableEntities = filters.Select(f => new DeviceListFilterTableEntity(f));
            ret = await deviceListFilterRepository.GetRecentFiltersAsync(max, false);
            Assert.Equal(40 - max + 1, ret.Count());
            Assert.False(ret.Any(f => f.Name.Equals(Constants.UnnamedFilterName, StringComparison.InvariantCultureIgnoreCase)));
            ret = await deviceListFilterRepository.GetRecentFiltersAsync(max, true);
            Assert.Equal(40 - max, ret.Count());
            Assert.False(ret.Any(f => f.Name.Equals(Constants.UnnamedFilterName, StringComparison.InvariantCultureIgnoreCase)));

        }

        [Fact]
        public async void GetFilterListAsyncTest()
        {
            var filters = fixture.CreateMany<DeviceListFilter>(10);
            var tableEntities = filters.Select(f => new DeviceListFilterTableEntity(f));
            _filterTableStorageClientMock.Setup(x => x.ExecuteQueryAsync(It.IsNotNull<TableQuery<DeviceListFilterTableEntity>>()))
                .ReturnsAsync(tableEntities.OrderBy(e => e.Name));
            var ret = await deviceListFilterRepository.GetFilterListAsync(0, 1000);
            Assert.Equal(10, ret.Count());
            Assert.Equal(tableEntities.OrderBy(e => e.Name).Select(e => new string[] { e.PartitionKey, e.Name }).ToArray(), ret.Select(e => new string[] { e.Id, e.Name }).ToArray());
            filters.Take(4).All(f => { f.IsTemporary = true; return true; });
            filters.Take(5).All(f => { f.Name = Constants.UnnamedFilterName; return true; });
            ret = await deviceListFilterRepository.GetFilterListAsync(0, 10, true);
            Assert.Equal(5, ret.Count());
            ret = await deviceListFilterRepository.GetFilterListAsync(0, 10, false);
            Assert.Equal(5, ret.Count());

            filters = fixture.CreateMany<DeviceListFilter>(10);
            tableEntities = filters.Select(f => new DeviceListFilterTableEntity(f));
            _filterTableStorageClientMock.Setup(x => x.ExecuteQueryAsync(It.IsNotNull<TableQuery<DeviceListFilterTableEntity>>()))
                .ReturnsAsync(tableEntities.OrderBy(e => e.Name));
            filters.Take(4).All(f => { f.IsTemporary = true; return true; });
            filters.Take(3).All(f => { f.Name = Constants.UnnamedFilterName; return true; });
            ret = await deviceListFilterRepository.GetFilterListAsync(0, 10, true);
            Assert.Equal(6, ret.Count());
            ret = await deviceListFilterRepository.GetFilterListAsync(0, 10, false);
            Assert.Equal(7, ret.Count());
        }

        [Fact]
        public async void GetSuggestClausesAsyncTest()
        {
            var tableEntities = fixture.CreateMany<ClauseTableEntity>(3);
            _clauseTableStorageClientMock.Setup(x => x.ExecuteQueryAsync(It.IsNotNull<TableQuery<ClauseTableEntity>>()))
                .ReturnsAsync(tableEntities);
            var ret = await deviceListFilterRepository.GetSuggestClausesAsync(0, 1000);
            Assert.Equal(3, ret.Count());
        }

        [Fact]
        public async void SaveSuggestClausesAsyncTest()
        {
            var clauses = fixture.CreateMany<Clause>(3).ToList();
            var tableEntities = fixture.CreateMany<ClauseTableEntity>(1);
            _clauseTableStorageClientMock.Setup(x => x.ExecuteQueryAsync(It.IsNotNull<TableQuery<ClauseTableEntity>>()))
                .ReturnsAsync(tableEntities);
            _clauseTableStorageClientMock.Setup(x => x.ExecuteAsync(It.IsNotNull<TableOperation>())).ReturnsAsync(It.IsAny<TableResult>());
            int result = await deviceListFilterRepository.SaveSuggestClausesAsync(clauses);
            Assert.Equal(clauses.Count, result);
            result = await deviceListFilterRepository.SaveSuggestClausesAsync(null);
            Assert.Equal(0, result);
            result = await deviceListFilterRepository.SaveSuggestClausesAsync(new List<Clause>());
            Assert.Equal(0, result);
        }

        [Fact]
        public async void DeleteSuggestClausesAsyncTest()
        {
            var clauses = fixture.CreateMany<Clause>(3).ToList();
            var tableEntities = fixture.CreateMany<ClauseTableEntity>(1);

            var resp = fixture.Create<TableStorageResponse<Clause>>();

            _clauseTableStorageClientMock.Setup(x => x.DoDeleteAsync(It.IsNotNull<ClauseTableEntity>(),
                It.IsNotNull<Func<ClauseTableEntity, Clause>>()))
                .ReturnsAsync(resp);
            int result = await deviceListFilterRepository.DeleteSuggestClausesAsync(clauses);
            Assert.Equal(clauses.Count, result);
            result = await deviceListFilterRepository.DeleteSuggestClausesAsync(null);
            Assert.Equal(0, result);
            result = await deviceListFilterRepository.DeleteSuggestClausesAsync(new List<Clause>());
            Assert.Equal(0, result);
        }
    }
}
