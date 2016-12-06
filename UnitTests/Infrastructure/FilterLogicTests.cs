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
    public class FilterLogicTests
    {
        private readonly Mock<IDeviceListFilterRepository> _deviceListFilterRepositoryMock;
        private readonly Mock<IJobRepository> _jobRepositoryMock;
        private readonly IFilterLogic _filterLogic;
        private readonly Fixture fixture;

        public FilterLogicTests ()
        {
            _deviceListFilterRepositoryMock = new Mock<IDeviceListFilterRepository>();
            _jobRepositoryMock = new Mock<IJobRepository>();
            _filterLogic = new FilterLogic(_deviceListFilterRepositoryMock.Object, _jobRepositoryMock.Object);
            fixture = new Fixture();
        }

        [Fact]
        public async void AddFilterAsyncTests()
        {
            var query = new Mock<Filter>();
            var jobs = fixture.CreateMany<JobRepositoryModel>(3);
            _deviceListFilterRepositoryMock.Setup(x => x.SaveFilterAsync(It.IsAny<DeviceListFilter>(), true)).ReturnsAsync(true);
            var ret = await _filterLogic.AddFilterAsync(query.Object);
            Assert.True(ret);
            _jobRepositoryMock.Setup(x => x.QueryByQueryNameAsync(It.IsAny<string>())).ReturnsAsync(jobs);
            await Assert.ThrowsAnyAsync<FilterAssociatedWithJobException>(async () => await _filterLogic.AddFilterAsync(query.Object));
        }

        [Fact]
        public async void GetRecentFiltersTests()
        {
            var filters = fixture.CreateMany<DeviceListFilter>(5);
            foreach(var filter in filters)
            {
                filter.Clauses = null;
            }
            _deviceListFilterRepositoryMock.Setup(x => x.GetRecentFiltersAsync(It.IsAny<int>())).ReturnsAsync(filters.Take(3));
            var ret = await _filterLogic.GetRecentFiltersAsync(3);
            Assert.Equal(3, ret.Count());
        }

        [Fact]
        public async void GetFilterAsyncTests()
        {
            var filter = new Mock<DeviceListFilter>();
            _deviceListFilterRepositoryMock.Setup(x => x.GetFilterAsync(It.IsAny<string>())).ReturnsAsync(filter.Object);
            var ret = await _filterLogic.GetFilterAsync("name");
            Assert.NotNull(ret);
            _deviceListFilterRepositoryMock.Setup(x => x.GetFilterAsync(It.IsAny<string>())).ReturnsAsync(null);
            await Assert.ThrowsAsync<FilterNotFoundException>(async () => await _filterLogic.GetFilterAsync("name"));
        }

        [Fact]
        public async void GetAvailableFilterNameAsyncTests()
        {
            string prefix = "myFilter";
            _deviceListFilterRepositoryMock.Setup(x => x.CheckFilterNameAsync(prefix)).ReturnsAsync(false);
            var ret = await _filterLogic.GetAvailableFilterNameAsync(prefix);
            Assert.Equal("myFilter1", ret);
            _deviceListFilterRepositoryMock.Setup(x => x.CheckFilterNameAsync(prefix)).ReturnsAsync(true);
            Assert.True(ret.StartsWith(prefix));
        }

        [Fact]
        public void GenerateSqlTests()
        {
            var filters = fixture.CreateMany<Clause>(0);
            var ret = _filterLogic.GenerateAdvancedClause(filters);
            Assert.Equal("SELECT * FROM devices", ret);
            filters = new List<Clause>
            {
                new Clause
                {
                    ColumnName = "deviceId",
                    ClauseType = ClauseType.EQ,
                    ClauseValue = "myDevice-1",
                }
            };
            Assert.Equal("SELECT * FROM devices WHERE deviceId = 'myDevice-1'", _filterLogic.GenerateAdvancedClause(filters));
        }

        [Fact]
        public async void DeleteFilterAsyncTests()
        {
            var jobs = fixture.CreateMany<JobRepositoryModel>(3);
            _deviceListFilterRepositoryMock.Setup(x => x.DeleteFilterAsync(It.IsAny<string>())).ReturnsAsync(true);
            var ret = await _filterLogic.DeleteFilterAsync("myFilter");
            Assert.True(ret);
            _jobRepositoryMock.Setup(x => x.QueryByQueryNameAsync(It.IsAny<string>())).ReturnsAsync(jobs);
            await Assert.ThrowsAnyAsync<FilterAssociatedWithJobException>(async () => await _filterLogic.DeleteFilterAsync("myQuery"));
        }

        [Fact]
        public async void GetFilterListTests()
        {
            var queryNames = fixture.CreateMany<string>(5);
            _deviceListFilterRepositoryMock.Setup(x => x.GetFilterListAsync()).ReturnsAsync(queryNames);
            var ret = await _filterLogic.GetFilterList();
            Assert.Equal(5, ret.Count());
        }
    }
}
