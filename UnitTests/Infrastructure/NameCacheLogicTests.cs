using Microsoft.Azure.Devices.Applications.RemoteMonitoring.Common.Models.Commands;
using Microsoft.Azure.Devices.Applications.RemoteMonitoring.DeviceAdmin.Infrastructure.BusinessLogic;
using Microsoft.Azure.Devices.Applications.RemoteMonitoring.DeviceAdmin.Infrastructure.Models;
using Microsoft.Azure.Devices.Applications.RemoteMonitoring.DeviceAdmin.Infrastructure.Repository;
using Moq;
using Ploeh.AutoFixture;
using Xunit;

namespace Microsoft.Azure.Devices.Applications.RemoteMonitoring.UnitTests.Infrastructure
{
    public class NameCacheLogicTests
    {
        private readonly Mock<INameCacheRepository> _nameCacheRepositoryMock;
        private readonly INameCacheLogic _nameCacheLogic;
        private readonly Fixture fixture;

        public NameCacheLogicTests()
        {
            _nameCacheRepositoryMock = new Mock<INameCacheRepository>();
            _nameCacheLogic = new NameCacheLogic(_nameCacheRepositoryMock.Object);
            fixture = new Fixture();
        }

        [Fact]
        public async void GetNameListAsyncTest()
        {
            var ret = await _nameCacheLogic.GetNameListAsync(NameCacheEntityType.All);
            Assert.NotNull(ret);
        }

        [Fact]
        public async void AddNameAsyncTest()
        {
            var name = "desired.test";
            _nameCacheRepositoryMock.Setup(x => x.AddNameAsync(NameCacheEntityType.DesiredProperty, It.IsAny<NameCacheEntity>())).ReturnsAsync(true);
            var ret = await _nameCacheLogic.AddNameAsync(name);
            Assert.True(ret);

            name = "reported.test";
            _nameCacheRepositoryMock.Setup(x => x.AddNameAsync(NameCacheEntityType.ReportedProperty, It.IsAny<NameCacheEntity>())).ReturnsAsync(true);
            ret = await _nameCacheLogic.AddNameAsync(name);
            Assert.True(ret);

            name = "tags.test";
            _nameCacheRepositoryMock.Setup(x => x.AddNameAsync(NameCacheEntityType.Tag, It.IsAny<NameCacheEntity>())).ReturnsAsync(true);
            ret = await _nameCacheLogic.AddNameAsync(name);
            Assert.True(ret);

            name = "deviceId";
            _nameCacheRepositoryMock.Setup(x => x.AddNameAsync(NameCacheEntityType.DeviceInfo, It.IsAny<NameCacheEntity>())).ReturnsAsync(true);
            ret = await _nameCacheLogic.AddNameAsync(name);
            Assert.True(ret);
        }

        [Fact]
        public async void AddMethodAsyncTest()
        {
            _nameCacheRepositoryMock.Setup(x => x.AddNameAsync(NameCacheEntityType.Method, It.IsAny<NameCacheEntity>())).ReturnsAsync(true);
            var method = fixture.Create<Command>();
            var ret = await _nameCacheLogic.AddMethodAsync(method);
            Assert.True(ret);
        }

        [Fact]
        public async void DeleteNameAsyncTest()
        {
            var name = fixture.Create<string>();
            _nameCacheRepositoryMock.Setup(x => x.DeleteNameAsync(NameCacheEntityType.DeviceInfo, name)).ReturnsAsync(true);
            var ret = await _nameCacheLogic.DeleteNameAsync(name);
            Assert.True(ret);
        }

        [Fact]
        public async void DeleteMethodAsyncTest()
        {
            var name = fixture.Create<string>();
            _nameCacheRepositoryMock.Setup(x => x.DeleteNameAsync(NameCacheEntityType.Method, name)).ReturnsAsync(true);
            var ret = await _nameCacheLogic.DeleteMethodAsync(name);
            Assert.True(ret);
        }
    }
}