using System.Collections.Generic;
using System.Linq;
using Microsoft.Azure.Devices.Applications.RemoteMonitoring.DeviceAdmin.Infrastructure.BusinessLogic;
using Microsoft.Azure.Devices.Applications.RemoteMonitoring.DeviceAdmin.Infrastructure.Exceptions;
using Microsoft.Azure.Devices.Applications.RemoteMonitoring.DeviceAdmin.Infrastructure.Models;
using Microsoft.Azure.Devices.Applications.RemoteMonitoring.DeviceAdmin.Infrastructure.Repository;
using Moq;
using Ploeh.AutoFixture;
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
        public async void SaveFilterAsyncTests()
        {
            var deviceListFilter = fixture.Create<DeviceListFilter>();
            var jobs = fixture.CreateMany<JobRepositoryModel>(3);
            _jobRepositoryMock.Setup(x => x.QueryByFilterIdAsync(It.IsNotNull<string>())).ReturnsAsync(jobs);
            _deviceListFilterRepositoryMock.Setup(x => x.SaveFilterAsync(It.IsAny<DeviceListFilter>(), true)).ReturnsAsync(deviceListFilter);
            var filter = new Filter(deviceListFilter);
            var ret = await _filterLogic.SaveFilterAsync(filter);
            _jobRepositoryMock.Verify(x => x.QueryByFilterIdAsync(It.IsNotNull<string>()), Times.AtLeastOnce);
            _deviceListFilterRepositoryMock.Verify(x => x.SaveFilterAsync(It.IsNotNull<DeviceListFilter>(), It.IsAny<bool>()), Times.AtLeastOnce);
            Assert.NotNull(ret);
            Assert.Equal(ret.Id, filter.Id);
            Assert.Equal(ret.Name, filter.Name);
            Assert.Equal(ret.Clauses, filter.Clauses);

            deviceListFilter.Name = "changedName";
            ret = await _filterLogic.SaveFilterAsync(filter);
            _jobRepositoryMock.Verify(x => x.UpdateAssociatedFilterNameAsync(It.IsNotNull<IEnumerable<JobRepositoryModel>>()), Times.AtLeastOnce);
            Assert.Equal(ret.Name, "changedName");
        }

        [Fact]
        public async void GetRecentFiltersTests()
        {
            var filters = fixture.CreateMany<DeviceListFilter>(5);
            foreach(var filter in filters)
            {
                filter.Clauses = null;
            }
            _deviceListFilterRepositoryMock.Setup(x => x.GetRecentFiltersAsync(It.IsAny<int>(), It.IsAny<bool>())).ReturnsAsync(filters.Take(3));
            var ret = await _filterLogic.GetRecentFiltersAsync(3);
            Assert.Equal(3, ret.Count());
        }

        [Fact]
        public async void GetFilterAsyncTests()
        {
            var filter = new Mock<DeviceListFilter>();
            var jobs = fixture.CreateMany<JobRepositoryModel>();
            _deviceListFilterRepositoryMock.Setup(x => x.GetFilterAsync(It.IsAny<string>())).ReturnsAsync(null);
            await Assert.ThrowsAsync<FilterNotFoundException>(async () => await _filterLogic.GetFilterAsync("filterId"));
            _deviceListFilterRepositoryMock.Setup(x => x.GetFilterAsync(It.IsAny<string>())).ReturnsAsync(filter.Object);
            _jobRepositoryMock.Setup(x => x.QueryByFilterIdAsync(It.IsNotNull<string>())).ReturnsAsync(jobs);
            var ret = await _filterLogic.GetFilterAsync("filterId");
            Assert.NotNull(ret);
            Assert.Equal(ret.AssociatedJobsCount, jobs.Count());
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
            Assert.Equal(string.Empty, ret);
            filters = new List<Clause>
            {
                new Clause
                {
                    ColumnName = "deviceId",
                    ClauseType = ClauseType.EQ,
                    ClauseValue = "myDevice-1",
                }
            };
            Assert.Equal("deviceId = 'myDevice-1'", _filterLogic.GenerateAdvancedClause(filters));
        }

        [Fact]
        public async void DeleteFilterAsyncTests()
        {
            var jobs = fixture.CreateMany<JobRepositoryModel>(3);
            _deviceListFilterRepositoryMock.Setup(x => x.DeleteFilterAsync(It.IsAny<string>())).ReturnsAsync(true);
            var ret = await _filterLogic.DeleteFilterAsync("myFilter", true);
            Assert.True(ret);
            _jobRepositoryMock.Setup(x => x.QueryByFilterIdAsync(It.IsAny<string>())).ReturnsAsync(jobs);
            ret = await _filterLogic.DeleteFilterAsync("myFilter", false);
            Assert.False(ret);
        }

        [Fact]
        public async void GetFilterListTests()
        {
            var filters = fixture.CreateMany<DeviceListFilter>(5);
            _deviceListFilterRepositoryMock.Setup(x => x.GetFilterListAsync(It.IsNotNull<int>(), It.IsNotNull<int>(), It.IsAny<bool>())).ReturnsAsync(filters);
            var ret = await _filterLogic.GetFilterList(0, 10);
            Assert.Equal(5, ret.Count());
        }

        [Fact]
        public async void GetSuggestClausesTest()
        {
            var clauses = fixture.CreateMany<Clause>(5);
            _deviceListFilterRepositoryMock.Setup(x => x.GetSuggestClausesAsync(It.IsNotNull<int>(), It.IsNotNull<int>())).ReturnsAsync(clauses);
            var ret = await _filterLogic.GetSuggestClauses(0, 10);
            Assert.Equal(5, ret.Count());
        }

        [Fact]
        public async void DeleteSuggestClausesTest()
        {
            var clauses = fixture.CreateMany<Clause>(5);
            _deviceListFilterRepositoryMock.Setup(x => x.DeleteSuggestClausesAsync(clauses)).ReturnsAsync(3);
            int ret = await _filterLogic.DeleteSuggestClausesAsync(clauses);
            Assert.Equal(3, ret);
        }
    }
}
