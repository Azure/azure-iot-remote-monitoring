using Microsoft.Azure.Devices.Applications.RemoteMonitoring.DeviceAdmin.Infrastructure.BusinessLogic;
using Microsoft.Azure.Devices.Applications.RemoteMonitoring.DeviceAdmin.Infrastructure.Exceptions;
using Microsoft.Azure.Devices.Applications.RemoteMonitoring.DeviceAdmin.Infrastructure.Models;
using Microsoft.Azure.Devices.Applications.RemoteMonitoring.DeviceAdmin.Infrastructure.Repository;
using Moq;
using Ploeh.AutoFixture;
using System.Collections.Generic;
using System.Linq;
using Xunit;

namespace Microsoft.Azure.Devices.Applications.RemoteMonitoring.UnitTests.Infrastructure
{
    public class QueryLogicTests
    {
        private readonly Mock<IDeviceListQueryRepository> _deviceListQueryRepositoryMock;
        private readonly IQueryLogic _queryLogic;
        private readonly Fixture fixture;

        public QueryLogicTests ()
        {
            _deviceListQueryRepositoryMock = new Mock<IDeviceListQueryRepository>();
            _queryLogic = new QueryLogic(_deviceListQueryRepositoryMock.Object);
            fixture = new Fixture();
        }

        [Fact]
        public async void AddQueryAsyncTests()
        {
            var query = new Mock<Query>();
            _deviceListQueryRepositoryMock.Setup(x => x.SaveQueryAsync(It.IsAny<DeviceListQuery>(), true)).ReturnsAsync(true);
            var ret = await _queryLogic.AddQueryAsync(query.Object);
            Assert.True(ret);
        }

        [Fact]
        public async void GetRecentQueriesTests()
        {
            var queries = fixture.CreateMany<DeviceListQuery>(5);
            foreach(var query in queries)
            {
                query.Filters = null;
            }
            _deviceListQueryRepositoryMock.Setup(x => x.GetRecentQueriesAsync(It.IsAny<int>())).ReturnsAsync(queries.Take(3));
            var ret = await _queryLogic.GetRecentQueriesAsync(3);
            Assert.Equal(3, ret.Count());
        }

        [Fact]
        public async void GetQueryAsyncTests()
        {
            var query = new Mock<DeviceListQuery>();
            _deviceListQueryRepositoryMock.Setup(x => x.GetQueryAsync(It.IsAny<string>())).ReturnsAsync(query.Object);
            var ret = await _queryLogic.GetQueryAsync("name");
            Assert.NotNull(ret);
            _deviceListQueryRepositoryMock.Setup(x => x.GetQueryAsync(It.IsAny<string>())).ReturnsAsync(null);
            await Assert.ThrowsAsync<QueryNotFoundException>(async () => await _queryLogic.GetQueryAsync("name"));
        }

        [Fact]
        public async void GetAvailableQueryNameAsyncTests()
        {
            string prefix = "myQuery";
            _deviceListQueryRepositoryMock.Setup(x => x.CheckQueryNameAsync(prefix)).ReturnsAsync(false);
            var ret = await _queryLogic.GetAvailableQueryNameAsync(prefix);
            Assert.Equal("myQuery1", ret);
            _deviceListQueryRepositoryMock.Setup(x => x.CheckQueryNameAsync(prefix)).ReturnsAsync(true);
            Assert.True(ret.StartsWith(prefix));
        }

        [Fact]
        public void GenerateSqlTests()
        {
            var filters = fixture.CreateMany<FilterInfo>(0);
            var ret = _queryLogic.GenerateSql(filters);
            Assert.Equal("SELECT * FROM devices", ret);
            filters = new List<FilterInfo>
            {
                new FilterInfo
                {
                    ColumnName = "deviceId",
                    FilterType = FilterType.EQ,
                    FilterValue = "myDevice-1",
                }
            };
            Assert.Equal("SELECT * FROM devices WHERE deviceId = 'myDevice-1'", _queryLogic.GenerateSql(filters));
        }

        [Fact]
        public async void DeleteQueryAsyncTests()
        {
            _deviceListQueryRepositoryMock.Setup(x => x.DeleteQueryAsync(It.IsAny<string>())).ReturnsAsync(true);
            var ret = await _queryLogic.DeleteQueryAsync("myQuery");
            Assert.True(ret);
        }

        [Fact]
        public async void GetQueryNameListTests()
        {
            var queryNames = fixture.CreateMany<string>(5);
            _deviceListQueryRepositoryMock.Setup(x => x.GetQueryNameListAsync()).ReturnsAsync(queryNames);
            var ret = await _queryLogic.GetQueryNameList();
            Assert.Equal(5, ret.Count());
        }
    }
}
