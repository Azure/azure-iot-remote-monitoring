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
        public async void GetAllDeviceTagNamesAsyncTest()
        {
            var tableEntities = fixture.Create<List<DeviceTwinMethodTableEntity>>();
            _tableStorageClientMock.Setup(x => x.ExecuteQueryAsync(It.IsNotNull<TableQuery<DeviceTwinMethodTableEntity>>()))
                .ReturnsAsync(tableEntities);
            var tagNames = await deviceTwinMethodRepository.GetAllDeviceTagNamesAsync();
            Assert.NotNull(tagNames);
            Assert.Equal(tableEntities.Count, tagNames.Count());
            Assert.Equal(tagNames.ToArray(), tableEntities.OrderByDescending(e => e.Timestamp).Select(e => e.TagName).ToArray());
        }

        [Fact]
        public async void AddDeviceTwinTagNameAsyncTest()
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
            var ret = await deviceTwinMethodRepository.AddDeviceTagNameAsync(newDeviceTwinMethod.TagName);
            Assert.True(ret);
            Assert.NotNull(tableEntity);
            Assert.Equal(newDeviceTwinMethod.TagName, tableEntity.TagName);
        }

        [Fact]
        public async void AddDeviceTwinTagNameAsyncFailureTest()
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
            var ret = await deviceTwinMethodRepository.AddDeviceTagNameAsync(newDeviceTwinMethod.TagName);
            Assert.False(ret);
            Assert.Null(tableEntity);
        }

        [Fact]
        public async void DeleteDeviceTwinTagNameAsyncTest()
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
            var ret = await deviceTwinMethodRepository.DeleteDeviceTagNameAsync(newDeviceTwinMethod.TagName);
            Assert.True(ret);
            Assert.NotNull(tableEntity);
            Assert.Equal(newDeviceTwinMethod.TagName, tableEntity.TagName);
        }

        [Fact]
        public async void DeleteDeviceTwinTagNameAsyncFailureTest()
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
            var ret = await deviceTwinMethodRepository.DeleteDeviceTagNameAsync(newDeviceTwinMethod.TagName);
            Assert.False(ret);
            Assert.Null(tableEntity);
        }

        [Fact]
        public async void GetAllDevicePropertyNamesAsyncTest()
        {
            var tableEntities = fixture.Create<List<DeviceTwinMethodTableEntity>>();
            _tableStorageClientMock.Setup(x => x.ExecuteQueryAsync(It.IsNotNull<TableQuery<DeviceTwinMethodTableEntity>>()))
                .ReturnsAsync(tableEntities);
            var propertyNames = await deviceTwinMethodRepository.GetAllDevicePropertyNamesAsync();
            Assert.NotNull(propertyNames);
            Assert.Equal(tableEntities.Count, propertyNames.Count());
            Assert.Equal(propertyNames.ToArray(), tableEntities.OrderByDescending(e => e.Timestamp).Select(e => e.PropertyName).ToArray());
        }

        [Fact]
        public async void AddDeviceTwinPropertyNameAsyncTest()
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
            var ret = await deviceTwinMethodRepository.AddDevicePropertyNameAsync(newDeviceTwinMethod.PropertyName);
            Assert.True(ret);
            Assert.NotNull(tableEntity);
            Assert.Equal(newDeviceTwinMethod.PropertyName, tableEntity.PropertyName);
        }

        [Fact]
        public async void AddDeviceTwinPropertyNameAsyncFailureTest()
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
            var ret = await deviceTwinMethodRepository.AddDevicePropertyNameAsync(newDeviceTwinMethod.PropertyName);
            Assert.False(ret);
            Assert.Null(tableEntity);
        }

        [Fact]
        public async void DeleteDeviceTwinPropertyNameAsyncTest()
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
            var ret = await deviceTwinMethodRepository.DeleteDevicePropertyNameAsync(newDeviceTwinMethod.PropertyName);
            Assert.NotNull(ret);
            Assert.True(ret);
            Assert.NotNull(tableEntity);
            Assert.Equal(newDeviceTwinMethod.PropertyName, tableEntity.PropertyName);
        }

        [Fact]
        public async void DeleteDeviceTwinPropertyNameAsyncFailureTest()
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
            var ret = await deviceTwinMethodRepository.DeleteDevicePropertyNameAsync(newDeviceTwinMethod.PropertyName);
            Assert.False(ret);
            Assert.Null(tableEntity);
        }

        [Fact]
        public async void GetAllDeviceMethodsAsyncTest()
        {
            var tableEntities = fixture.Create<List<DeviceTwinMethodTableEntity>>();
            _tableStorageClientMock.Setup(x => x.ExecuteQueryAsync(It.IsNotNull<TableQuery<DeviceTwinMethodTableEntity>>()))
                .ReturnsAsync(tableEntities);
            var deviceMethods = await deviceTwinMethodRepository.GetAllDeviceMethodsAsync();
            Assert.NotNull(deviceMethods);
            Assert.Equal(tableEntities.Count, deviceMethods.Count());
            Assert.Equal(deviceMethods.Select(m => m.Name).ToArray(), tableEntities.OrderByDescending(e => e.Timestamp).Select(e => e.MethodName).ToArray());
            Assert.Equal(deviceMethods.Select(m => m.Parameters).ToArray(), tableEntities.OrderByDescending(e => e.Timestamp).Select(e => e.MethodParameters).ToArray());
            Assert.Equal(deviceMethods.Select(m => m.Description).ToArray(), tableEntities.OrderByDescending(e => e.Timestamp).Select(e => e.MethodDescription).ToArray());
        }

        [Fact]
        public async void AddDeviceMethodAsyncTest()
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
            var ret = await deviceTwinMethodRepository.AddDeviceMethodAsync(newDeviceTwinMethod.Method);
            Assert.True(ret);
            Assert.NotNull(tableEntity);
            Assert.Equal(newDeviceTwinMethod.Method.Name, tableEntity.MethodName);
            Assert.Equal(newDeviceTwinMethod.Method.Parameters, tableEntity.MethodParameters);
            Assert.Equal(newDeviceTwinMethod.Method.Description, tableEntity.MethodDescription);
        }

        [Fact]
        public async void AddDeviceMethodAsyncFailureTest()
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
            var ret = await deviceTwinMethodRepository.AddDeviceMethodAsync(newDeviceTwinMethod.Method);
            Assert.False(ret);
            Assert.Null(tableEntity);
        }

        [Fact]
        public async void DeleteDeviceMethodAsyncTest()
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
            var ret = await deviceTwinMethodRepository.DeleteDeviceMethodAsync(newDeviceTwinMethod.Method);
            Assert.True(ret);
            Assert.NotNull(tableEntity);
            Assert.Equal(newDeviceTwinMethod.Method.Name, tableEntity.MethodName);
            Assert.Equal(newDeviceTwinMethod.Method.Parameters, tableEntity.MethodParameters);
            Assert.Equal(newDeviceTwinMethod.Method.Description, tableEntity.MethodDescription);
        }

        [Fact]
        public async void DeleteDeviceMethodAsyncFailureTest()
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
                    (entity, func) => tableEntity = entity)
                .ReturnsAsync(resp);
            var ret = await deviceTwinMethodRepository.DeleteDeviceMethodAsync(newDeviceTwinMethod.Method);
            Assert.False(ret);
            Assert.NotNull(tableEntity);
        }
    }
}
