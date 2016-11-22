using System;
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
    public class UserSettingsRepositoryTests
    {
        private readonly IFixture fixture;
        private readonly Mock<IConfigurationProvider> _configurationProviderMock;
        private readonly Mock<IAzureTableStorageClient> _tableStorageClientMock;
        private readonly UserSettingsRepository _userSettingsRepository;

        public UserSettingsRepositoryTests()
        {
            fixture = new Fixture();
            fixture.Customize(new AutoConfiguredMoqCustomization());
            _configurationProviderMock = new Mock<IConfigurationProvider>();
            _configurationProviderMock.Setup(x => x.GetConfigurationSettingValue(It.IsNotNull<string>()))
                .ReturnsUsingFixture(fixture);
            _tableStorageClientMock = new Mock<IAzureTableStorageClient>();
            var tableStorageClientFactory = new AzureTableStorageClientFactory(_tableStorageClientMock.Object);
            _userSettingsRepository = new UserSettingsRepository(_configurationProviderMock.Object,
                tableStorageClientFactory);
        }

        [Fact]
        public async void SetUserSettingAsyncTest()
        {
            var userId = fixture.Create<string>();
            var userSetting = fixture.Create<UserSetting>();

            _tableStorageClientMock
                .Setup(x => x.DoTableInsertOrReplaceAsync(
                    It.IsAny<UserSettingTableEntity>(),
                    It.IsAny<Func<UserSettingTableEntity, UserSetting>>()))
                .ReturnsAsync(new TableStorageResponse<UserSetting> {
                    Entity = userSetting,
                    Status = TableStorageResponseStatus.Successful
                });

            var ret = await _userSettingsRepository.SetUserSettingAsync(userId, userSetting, true);
            Assert.NotNull(ret);
            Assert.Equal(userSetting.Key, ret.Key);
            Assert.Equal(userSetting.Value, ret.Value);
        }

        [Fact]
        public async void GetUserSettingAsyncTest()
        {
            var userId = fixture.Create<string>();
            var settingKey = fixture.Create<string>();
            var userSetting = fixture.Create<UserSetting>();
            
            _tableStorageClientMock
                .Setup(x => x.ExecuteAsync(It.IsAny<TableOperation>()))
                .ReturnsAsync(new TableResult()
                {
                    Result = new UserSettingTableEntity()
                    {
                        RowKey = userSetting.Key,
                        SettingValue = userSetting.Value
                    }
                });
            var ret = await _userSettingsRepository.GetUserSettingAsync(userId, settingKey);
            Assert.NotNull(ret);
            Assert.Equal(userSetting.Key, ret.Key);
            Assert.Equal(userSetting.Value, ret.Value);
        }

        [Fact]
        public async void GetGlobalUserSettingAsyncTest()
        {
            var settingKey = fixture.Create<string>();
            var userSetting = fixture.Create<UserSetting>();

            _tableStorageClientMock
                .Setup(x => x.ExecuteAsync(It.IsAny<TableOperation>()))
                .ReturnsAsync(new TableResult()
                {
                    Result = new UserSettingTableEntity()
                    {
                        RowKey = userSetting.Key,
                        SettingValue = userSetting.Value
                    }
                });
            var ret = await _userSettingsRepository.GetGlobalUserSettingAsync(settingKey);
            Assert.NotNull(ret);
            Assert.Equal(userSetting.Key, ret.Key);
            Assert.Equal(userSetting.Value, ret.Value);
        }
    }
}
