using System.Collections.Generic;
using System.Linq;
using Microsoft.Azure.Devices.Applications.RemoteMonitoring.DeviceAdmin.Infrastructure.BusinessLogic;
using Microsoft.Azure.Devices.Applications.RemoteMonitoring.DeviceAdmin.Infrastructure.Models;
using Microsoft.Azure.Devices.Applications.RemoteMonitoring.DeviceAdmin.Infrastructure.Repository;
using Moq;
using Newtonsoft.Json;
using Ploeh.AutoFixture;
using Xunit;

namespace Microsoft.Azure.Devices.Applications.RemoteMonitoring.UnitTests.Infrastructure
{
    public class UserSettingsLogicTests
    {
        private readonly IFixture fixture;

        private readonly Mock<IUserSettingsRepository> _userSettingsRepository;
        private readonly UserSettingsLogic _userSettingsLogic;

        public UserSettingsLogicTests()
        {
            fixture = new Fixture();
            _userSettingsRepository = new Mock<IUserSettingsRepository>();
            _userSettingsLogic = new UserSettingsLogic(_userSettingsRepository.Object);
        }

        [Fact]
        public async void SetDeviceListColumnsAsyncTest()
        {
            var userId = fixture.Create<string>();
            var userSetting = fixture.Create<UserSetting>();
            var columns = fixture.Create<IEnumerable<DeviceListColumns>>();

            _userSettingsRepository
                .Setup(x => x.SetUserSettingAsync(
                    It.IsAny<string>(),
                    It.IsAny<UserSetting>(),
                    false))
                .ReturnsAsync(userSetting);

            var ret = await _userSettingsLogic.SetDeviceListColumnsAsync(userId, columns);
            Assert.True(ret);
        }

        [Fact]
        public async void GetDeviceListColumnsAsyncTest()
        {
            var userId = fixture.Create<string>();
            var columns = fixture.Create<IEnumerable<DeviceListColumns>>();

            var userSetting = new UserSetting()
            {
                Key = fixture.Create<string>(),
                Value = JsonConvert.SerializeObject(columns)
            };

            _userSettingsRepository
                .Setup(x => x.GetUserSettingAsync(
                    It.IsAny<string>(),
                    It.IsAny<string>()))
                .ReturnsAsync(userSetting);

            var ret = await _userSettingsLogic.GetDeviceListColumnsAsync(userId);
            Assert.NotNull(ret);
            Assert.Equal(columns.Count(), ret.Count());
            Assert.Equal(columns.First().Name, ret.First().Name);
        }

        [Fact]
        public async void GetGlobalDeviceListColumnsAsyncTest()
        {
            var columns = fixture.Create<IEnumerable<DeviceListColumns>>();

            var userSetting = new UserSetting()
            {
                Key = fixture.Create<string>(),
                Value = JsonConvert.SerializeObject(columns)
            };

            _userSettingsRepository
                .Setup(x => x.GetGlobalUserSettingAsync(It.IsAny<string>()))
                .ReturnsAsync(userSetting);

            var ret = await _userSettingsLogic.GetGlobalDeviceListColumnsAsync();
            Assert.NotNull(ret);
            Assert.Equal(columns.Count(), ret.Count());
            Assert.Equal(columns.First().Name, ret.First().Name);
        }
    }
}
