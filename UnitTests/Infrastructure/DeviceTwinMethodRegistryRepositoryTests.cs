using System;
using System.Linq;
using System.Collections.Generic;
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
    public class DeviceTwinMethodRegistryRepositoryTests
    {
        private readonly IFixture fixture;
        private readonly Mock<IConfigurationProvider> _configurationProviderMock;
        private readonly Mock<IAzureTableStorageClient> _tableStorageClientMock;
        private readonly DeviceTwinMethodRegistrationRepository deviceTwinMethodRepository;

        public DeviceTwinMethodRegistryRepositoryTests()
        {
            fixture = new Fixture();
            fixture.Customize(new AutoConfiguredMoqCustomization());
            _configurationProviderMock = new Mock<IConfigurationProvider>();
            _configurationProviderMock.Setup(x => x.GetConfigurationSettingValue(It.IsNotNull<string>()))
                .ReturnsUsingFixture(fixture);
            _tableStorageClientMock = new Mock<IAzureTableStorageClient>();
            var tableStorageClientFactory = new AzureTableStorageClientFactory(_tableStorageClientMock.Object);
            deviceTwinMethodRepository = new DeviceTwinMethodRegistrationRepository(_configurationProviderMock.Object,
                tableStorageClientFactory);
        }

        [Fact]
        public async void GetNameListAsyncTest()
        {
            List<DeviceTwinMethodTableEntity> tableEntities = fixture.Create<List<DeviceTwinMethodTableEntity>>();
            foreach (var e in tableEntities) {
                e.MethodParameters = "[{'Name':'fake-parameter', 'Type': 'String'}]";
                e.PartitionKey = DeviceTwinMethodEntityType.DesiredProperty.ToString();
            }
            _tableStorageClientMock.Setup(x => x.ExecuteQueryAsync(It.IsNotNull<TableQuery<DeviceTwinMethodTableEntity>>()))
                .ReturnsAsync(tableEntities);
            var names = await deviceTwinMethodRepository.GetNameListAsync(DeviceTwinMethodEntityType.Tag | DeviceTwinMethodEntityType.Property);
            Assert.NotNull(names);
            Assert.Equal(3, names.Count());
            Assert.Equal(names.Select(e => e.Name).ToArray(), tableEntities.OrderByDescending(e => e.Timestamp).Select(e => e.Name).ToArray());
        }

        [Fact]
        public async void AddNameAsyncTest()
        {
            var newDeviceTwinMethod = fixture.Create<DeviceTwinMethodEntity>();
            DeviceTwinMethodTableEntity tableEntity = null;
            var resp = new TableStorageResponse<DeviceTwinMethodEntity>
            {
                Entity = newDeviceTwinMethod,
                Status = TableStorageResponseStatus.Successful
            };
            _tableStorageClientMock.Setup(
                x =>
                    x.DoTableInsertOrReplaceAsync(It.IsNotNull<DeviceTwinMethodTableEntity>(),
                        It.IsNotNull<Func<DeviceTwinMethodTableEntity, DeviceTwinMethodEntity>>()))
                .Callback<DeviceTwinMethodTableEntity, Func<DeviceTwinMethodTableEntity, DeviceTwinMethodEntity>>(
                    (entity, func) => tableEntity = entity)
                .ReturnsAsync(resp);
            var tableEntities = fixture.Create<List<DeviceTwinMethodTableEntity>>();
            _tableStorageClientMock.Setup(x => x.ExecuteQueryAsync(It.IsNotNull<TableQuery<DeviceTwinMethodTableEntity>>()))
                .ReturnsAsync(tableEntities);
            var ret = await deviceTwinMethodRepository.AddNameAsync(DeviceTwinMethodEntityType.DesiredProperty, newDeviceTwinMethod);
            Assert.True(ret);
            Assert.NotNull(tableEntity);
            Assert.Equal(newDeviceTwinMethod.Name, tableEntity.Name);
            Assert.Equal(newDeviceTwinMethod.Description, tableEntity.MethodDescription);
            Assert.Equal(DeviceTwinMethodEntityType.DesiredProperty.ToString(), tableEntity.PartitionKey);
        }

        [Fact]
        public async void AddNameAsyncFailureTest()
        {
            var newDeviceTwinMethod = fixture.Create<DeviceTwinMethodEntity>();
            DeviceTwinMethodTableEntity tableEntity = null;
            var resp = new TableStorageResponse<DeviceTwinMethodEntity>
            {
                Entity = newDeviceTwinMethod,
                Status = TableStorageResponseStatus.NotFound
            };
            _tableStorageClientMock.Setup(
                x =>
                    x.DoTableInsertOrReplaceAsync(It.IsNotNull<DeviceTwinMethodTableEntity>(),
                        It.IsNotNull<Func<DeviceTwinMethodTableEntity, DeviceTwinMethodEntity>>()))
                .Callback<DeviceTwinMethodTableEntity, Func<DeviceTwinMethodTableEntity, DeviceTwinMethodEntity>>(
                    (entity, func) => tableEntity = null)
                .ReturnsAsync(resp);
            var tableEntities = fixture.Create<List<DeviceTwinMethodTableEntity>>();
            _tableStorageClientMock.Setup(x => x.ExecuteQueryAsync(It.IsNotNull<TableQuery<DeviceTwinMethodTableEntity>>()))
                .ReturnsAsync(tableEntities);
            var ret = await deviceTwinMethodRepository.AddNameAsync(DeviceTwinMethodEntityType.DesiredProperty, newDeviceTwinMethod);
            Assert.False(ret);
            Assert.Null(tableEntity);
            await Assert.ThrowsAsync<ArgumentException>(async () =>
                await deviceTwinMethodRepository.AddNameAsync(DeviceTwinMethodEntityType.Property, newDeviceTwinMethod));
        }

        [Fact]
        public async void DeleteNameAsyncTest()
        {
            var newDeviceTwinMethod = fixture.Create<DeviceTwinMethodEntity>();
            DeviceTwinMethodTableEntity tableEntity = null;
            var resp = new TableStorageResponse<DeviceTwinMethodEntity>
            {
                Entity = newDeviceTwinMethod,
                Status = TableStorageResponseStatus.Successful
            };
            _tableStorageClientMock.Setup(
                x =>
                    x.DoDeleteAsync(It.IsNotNull<DeviceTwinMethodTableEntity>(),
                        It.IsNotNull<Func<DeviceTwinMethodTableEntity, DeviceTwinMethodEntity>>()))
                .Callback<DeviceTwinMethodTableEntity, Func<DeviceTwinMethodTableEntity, DeviceTwinMethodEntity>>(
                    (entity, func) => tableEntity = entity)
                .ReturnsAsync(resp);
            var ret = await deviceTwinMethodRepository.DeleteNameAsync(DeviceTwinMethodEntityType.DesiredProperty, newDeviceTwinMethod.Name);
            Assert.True(ret);
            Assert.NotNull(tableEntity);
            Assert.Equal(newDeviceTwinMethod.Name, tableEntity.Name);
        }

        [Fact]
        public async void DeleteNameAsyncFailureTest()
        {
            var newDeviceTwinMethod = fixture.Create<DeviceTwinMethodEntity>();
            DeviceTwinMethodTableEntity tableEntity = null;
            var resp = new TableStorageResponse<DeviceTwinMethodEntity>
            {
                Entity = newDeviceTwinMethod,
                Status = TableStorageResponseStatus.NotFound
            };
            _tableStorageClientMock.Setup(
                x =>
                    x.DoDeleteAsync(It.IsNotNull<DeviceTwinMethodTableEntity>(),
                        It.IsNotNull<Func<DeviceTwinMethodTableEntity, DeviceTwinMethodEntity>>()))
                .Callback<DeviceTwinMethodTableEntity, Func<DeviceTwinMethodTableEntity, DeviceTwinMethodEntity>>(
                    (entity, func) => tableEntity = null)
                .ReturnsAsync(resp);
            var ret = await deviceTwinMethodRepository.DeleteNameAsync(DeviceTwinMethodEntityType.DesiredProperty, newDeviceTwinMethod.Name);
            Assert.False(ret);
            Assert.Null(tableEntity);
            await Assert.ThrowsAsync<ArgumentException>(async () =>
                await deviceTwinMethodRepository.DeleteNameAsync(DeviceTwinMethodEntityType.All, newDeviceTwinMethod.Name));
        }
    }
}
