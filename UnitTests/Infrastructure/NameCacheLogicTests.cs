using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
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
        public async Task GetNameListAsyncTest()
        {
            var ret = await _nameCacheLogic.GetNameListAsync(NameCacheEntityType.All);
            Assert.NotNull(ret);
        }

        [Fact]
        public async Task AddNameAsyncTest()
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
        public async Task AddShortNamesAsyncTest()
        {
            var names = fixture.CreateMany<string>();

            _nameCacheRepositoryMock.Setup(x => x.AddNamesAsync(
                It.IsAny<NameCacheEntityType>(),
                It.IsAny<IEnumerable<string>>()))
                .Returns(Task.FromResult(0));

            await _nameCacheLogic.AddShortNamesAsync(NameCacheEntityType.Tag, names);

            _nameCacheRepositoryMock.Verify(x => x.AddNamesAsync(
                NameCacheEntityType.Tag,
                It.Is<IEnumerable<string>>(args => args.SequenceEqual(names.Select(s => $"{_nameCacheLogic.PREFIX_TAGS}{s}")))));

            await _nameCacheLogic.AddShortNamesAsync(NameCacheEntityType.DesiredProperty, names);

            _nameCacheRepositoryMock.Verify(x => x.AddNamesAsync(
                NameCacheEntityType.DesiredProperty,
                It.Is<IEnumerable<string>>(args => args.SequenceEqual(names.Select(s => $"{_nameCacheLogic.PREFIX_DESIRED}{s}")))));

            await _nameCacheLogic.AddShortNamesAsync(NameCacheEntityType.ReportedProperty, names);

            _nameCacheRepositoryMock.Verify(x => x.AddNamesAsync(
                NameCacheEntityType.ReportedProperty,
                It.Is<IEnumerable<string>>(args => args.SequenceEqual(names.Select(s => $"{_nameCacheLogic.PREFIX_REPORTED}{s}")))));
        }

        [Fact]
        public async Task AddShortNamesAsyncThrowArgumentOutOfRangeTest()
        {
            var names = fixture.CreateMany<string>();

            _nameCacheRepositoryMock.Setup(x => x.AddNamesAsync(
                It.IsAny<NameCacheEntityType>(),
                It.IsAny<IEnumerable<string>>()))
                .Returns(Task.FromResult(0));

            foreach (NameCacheEntityType type in Enum.GetValues(typeof(NameCacheEntityType)))
            {
                if (type == NameCacheEntityType.Tag
                     || type == NameCacheEntityType.DesiredProperty
                     || type == NameCacheEntityType.ReportedProperty)
                {
                    continue;
                }

                await Assert.ThrowsAsync<ArgumentOutOfRangeException>(async () => await _nameCacheLogic.AddShortNamesAsync(type, names));
            }
        }

        [Fact]
        public async Task AddMethodAsyncTest()
        {
            _nameCacheRepositoryMock.Setup(x => x.AddNameAsync(NameCacheEntityType.Method, It.IsAny<NameCacheEntity>())).ReturnsAsync(true);
            var method = fixture.Create<Command>();
            var ret = await _nameCacheLogic.AddMethodAsync(method);
            Assert.True(ret);
        }

        [Fact]
        public async Task DeleteNameAsyncTest()
        {
            var name = fixture.Create<string>();
            _nameCacheRepositoryMock.Setup(x => x.DeleteNameAsync(NameCacheEntityType.DeviceInfo, name)).ReturnsAsync(true);
            var ret = await _nameCacheLogic.DeleteNameAsync(name);
            Assert.True(ret);
        }

        [Fact]
        public async Task DeleteMethodAsyncTest()
        {
            var name = fixture.Create<string>();
            _nameCacheRepositoryMock.Setup(x => x.DeleteNameAsync(NameCacheEntityType.Method, name)).ReturnsAsync(true);
            var ret = await _nameCacheLogic.DeleteMethodAsync(name);
            Assert.True(ret);
        }
    }
}