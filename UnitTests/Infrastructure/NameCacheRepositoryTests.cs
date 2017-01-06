using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Azure.Devices.Applications.RemoteMonitoring.Common.Configurations;
using Microsoft.Azure.Devices.Applications.RemoteMonitoring.Common.Helpers;
using Microsoft.Azure.Devices.Applications.RemoteMonitoring.Common.Models;
using Microsoft.Azure.Devices.Applications.RemoteMonitoring.DeviceAdmin.Infrastructure.Models;
using Microsoft.Azure.Devices.Applications.RemoteMonitoring.DeviceAdmin.Infrastructure.Repository;
using Microsoft.WindowsAzure.Storage.Table;
using Moq;
using Ploeh.AutoFixture;
using Ploeh.AutoFixture.AutoMoq;
using Xunit;

namespace Microsoft.Azure.Devices.Applications.RemoteMonitoring.UnitTests.Infrastructure
{
    public class NameCacheRepositoryTests
    {
        private readonly IFixture fixture;
        private readonly Mock<IConfigurationProvider> _configurationProviderMock;
        private readonly Mock<IAzureTableStorageClient> _tableStorageClientMock;
        private readonly NameCacheRepository nameCacheRepository;

        public NameCacheRepositoryTests()
        {
            fixture = new Fixture();
            fixture.Customize(new AutoConfiguredMoqCustomization());
            _configurationProviderMock = new Mock<IConfigurationProvider>();
            _configurationProviderMock.Setup(x => x.GetConfigurationSettingValue(It.IsNotNull<string>()))
                .ReturnsUsingFixture(fixture);
            _tableStorageClientMock = new Mock<IAzureTableStorageClient>();
            var tableStorageClientFactory = new AzureTableStorageClientFactory(_tableStorageClientMock.Object);
            nameCacheRepository = new NameCacheRepository(_configurationProviderMock.Object,
                tableStorageClientFactory);
        }

        [Fact]
        public async Task GetNameListAsyncTest()
        {
            List<NameCacheTableEntity> tableEntities = fixture.Create<List<NameCacheTableEntity>>();
            foreach (var e in tableEntities)
            {
                e.MethodParameters = "[{'Name':'fake-parameter', 'Type': 'String'}]";
                e.PartitionKey = NameCacheEntityType.DesiredProperty.ToString();
            }
            _tableStorageClientMock.Setup(x => x.ExecuteQueryAsync(It.IsNotNull<TableQuery<NameCacheTableEntity>>()))
                .ReturnsAsync(tableEntities);
            var names = await nameCacheRepository.GetNameListAsync(NameCacheEntityType.Tag | NameCacheEntityType.Property);
            Assert.NotNull(names);
            Assert.Equal(3, names.Count());
            Assert.Equal(names.Select(e => e.Name).ToArray(), tableEntities.OrderBy(e => e.PartitionKey).ThenBy(e => e.RowKey).Select(e => e.Name).ToArray());
        }

        [Fact]
        public async Task AddNameAsyncTest()
        {
            var newNameCache = fixture.Create<NameCacheEntity>();
            NameCacheTableEntity tableEntity = null;
            var resp = new TableStorageResponse<NameCacheEntity>
            {
                Entity = newNameCache,
                Status = TableStorageResponseStatus.Successful
            };
            _tableStorageClientMock.Setup(
                x =>
                    x.DoTableInsertOrReplaceAsync(It.IsNotNull<NameCacheTableEntity>(),
                        It.IsNotNull<Func<NameCacheTableEntity, NameCacheEntity>>()))
                .Callback<NameCacheTableEntity, Func<NameCacheTableEntity, NameCacheEntity>>(
                    (entity, func) => tableEntity = entity)
                .ReturnsAsync(resp);
            var tableEntities = fixture.Create<List<NameCacheTableEntity>>();
            _tableStorageClientMock.Setup(x => x.ExecuteQueryAsync(It.IsNotNull<TableQuery<NameCacheTableEntity>>()))
                .ReturnsAsync(tableEntities);
            var ret = await nameCacheRepository.AddNameAsync(NameCacheEntityType.DesiredProperty, newNameCache);
            Assert.True(ret);
            Assert.NotNull(tableEntity);
            Assert.Equal(newNameCache.Name, tableEntity.Name);
            Assert.Equal(newNameCache.Description, tableEntity.MethodDescription);
            Assert.Equal(NameCacheEntityType.DesiredProperty.ToString(), tableEntity.PartitionKey);
        }

        [Fact]
        public async Task AddNameAsyncFailureTest()
        {
            var newNameCache = fixture.Create<NameCacheEntity>();
            NameCacheTableEntity tableEntity = null;
            var resp = new TableStorageResponse<NameCacheEntity>
            {
                Entity = newNameCache,
                Status = TableStorageResponseStatus.NotFound
            };
            _tableStorageClientMock.Setup(
                x =>
                    x.DoTableInsertOrReplaceAsync(It.IsNotNull<NameCacheTableEntity>(),
                        It.IsNotNull<Func<NameCacheTableEntity, NameCacheEntity>>()))
                .Callback<NameCacheTableEntity, Func<NameCacheTableEntity, NameCacheEntity>>(
                    (entity, func) => tableEntity = null)
                .ReturnsAsync(resp);
            var tableEntities = fixture.Create<List<NameCacheTableEntity>>();
            _tableStorageClientMock.Setup(x => x.ExecuteQueryAsync(It.IsNotNull<TableQuery<NameCacheTableEntity>>()))
                .ReturnsAsync(tableEntities);
            var ret = await nameCacheRepository.AddNameAsync(NameCacheEntityType.DesiredProperty, newNameCache);
            Assert.False(ret);
            Assert.Null(tableEntity);
            await Assert.ThrowsAsync<ArgumentException>(async () =>
                await nameCacheRepository.AddNameAsync(NameCacheEntityType.Property, newNameCache));
        }

        [Fact]
        public async Task AddNamesAsyncTest()
        {
            await AddNamesAsyncTest(0);
            await AddNamesAsyncTest(nameCacheRepository.MaxBatchSize - 1);
            await AddNamesAsyncTest(nameCacheRepository.MaxBatchSize);
            await AddNamesAsyncTest(nameCacheRepository.MaxBatchSize + 1);
        }

        private async Task AddNamesAsyncTest(int totalNames)
        {
            var names = fixture.CreateMany<string>(totalNames);

            _tableStorageClientMock.Reset();
            _tableStorageClientMock.Setup(x => x.ExecuteBatchAsync(
                It.IsAny<TableBatchOperation>()))
                .Returns(async () => await Task.FromResult(new List<TableResult>()));

            await nameCacheRepository.AddNamesAsync(NameCacheEntityType.Tag, names);

            // Currently, it is impossible to check if the operation inside the batch is expected
            // Here we just check if the total of operations is expected

            int times = totalNames / nameCacheRepository.MaxBatchSize;
            if (totalNames % nameCacheRepository.MaxBatchSize > 0)
            {
                times++;
            }

            int totalOperations = 0;

            _tableStorageClientMock.Verify(x => x.ExecuteBatchAsync(
                It.Is<TableBatchOperation>(batch => CountOperations(batch, ref totalOperations))),
                Times.Exactly(times));

            Assert.Equal(totalOperations, names.Count());
        }

        private bool CountOperations(TableBatchOperation batch, ref int totalOperations)
        {
            totalOperations += batch.Count;
            return true;
        }

        [Fact]
        public async Task DeleteNameAsyncTest()
        {
            var newNameCache = fixture.Create<NameCacheEntity>();
            NameCacheTableEntity tableEntity = null;
            var resp = new TableStorageResponse<NameCacheEntity>
            {
                Entity = newNameCache,
                Status = TableStorageResponseStatus.Successful
            };
            _tableStorageClientMock.Setup(
                x =>
                    x.DoDeleteAsync(It.IsNotNull<NameCacheTableEntity>(),
                        It.IsNotNull<Func<NameCacheTableEntity, NameCacheEntity>>()))
                .Callback<NameCacheTableEntity, Func<NameCacheTableEntity, NameCacheEntity>>(
                    (entity, func) => tableEntity = entity)
                .ReturnsAsync(resp);
            var ret = await nameCacheRepository.DeleteNameAsync(NameCacheEntityType.DesiredProperty, newNameCache.Name);
            Assert.True(ret);
            Assert.NotNull(tableEntity);
            Assert.Equal(newNameCache.Name, tableEntity.Name);
        }

        [Fact]
        public async Task DeleteNameAsyncFailureTest()
        {
            var newNameCache = fixture.Create<NameCacheEntity>();
            NameCacheTableEntity tableEntity = null;
            var resp = new TableStorageResponse<NameCacheEntity>
            {
                Entity = newNameCache,
                Status = TableStorageResponseStatus.NotFound
            };
            _tableStorageClientMock.Setup(
                x =>
                    x.DoDeleteAsync(It.IsNotNull<NameCacheTableEntity>(),
                        It.IsNotNull<Func<NameCacheTableEntity, NameCacheEntity>>()))
                .Callback<NameCacheTableEntity, Func<NameCacheTableEntity, NameCacheEntity>>(
                    (entity, func) => tableEntity = null)
                .ReturnsAsync(resp);
            var ret = await nameCacheRepository.DeleteNameAsync(NameCacheEntityType.DesiredProperty, newNameCache.Name);
            Assert.False(ret);
            Assert.Null(tableEntity);
            await Assert.ThrowsAsync<ArgumentException>(async () =>
                await nameCacheRepository.DeleteNameAsync(NameCacheEntityType.All, newNameCache.Name));
        }
    }
}
