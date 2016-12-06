using System.Collections.Generic;
using System.Linq;
using Microsoft.Azure.Devices.Applications.RemoteMonitoring.DeviceAdmin.Infrastructure.BusinessLogic;
using Microsoft.Azure.Devices.Applications.RemoteMonitoring.DeviceAdmin.Infrastructure.Models;
using Microsoft.Azure.Devices.Applications.RemoteMonitoring.DeviceAdmin.Web.WebApiControllers;
using Moq;
using Ploeh.AutoFixture;
using Xunit;

namespace Microsoft.Azure.Devices.Applications.RemoteMonitoring.UnitTests.Web.WebApiControllers
{
    public class FilterApiControllerTests
    {
        private readonly FilterApiController controller;
        private readonly Mock<IFilterLogic> logicMock;
        private readonly Fixture fixture;

        public FilterApiControllerTests()
        {
            logicMock = new Mock<IFilterLogic>();
            controller = new FilterApiController(logicMock.Object);
            controller.InitializeRequest();
            fixture = new Fixture();
        }

        [Fact]
        public async void GetRecentFiltersTest()
        {
            var filters = fixture.CreateMany<Filter>(5);
            logicMock.Setup(x => x.GetRecentFiltersAsync(It.IsAny<int>())).ReturnsAsync(filters.Take(3));
            var result = await controller.GetRecentFilters(3);
            result.AssertOnError();
        }

        [Fact]
        public async void GetFilterTest()
        {
            var filter = new Mock<Filter>();
            logicMock.Setup(x => x.GetFilterAsync(It.IsAny<string>())).ReturnsAsync(filter.Object);
            var result = await controller.GetFilter("myFilter");
            result.AssertOnError();
        }

        [Fact]
        public async void AddFilterTest()
        {
            var filter = new Mock<Filter>();
            logicMock.Setup(x => x.AddFilterAsync(It.IsAny<Filter>())).ReturnsAsync(true);
            var result = await controller.AddQuery(filter.Object);
            result.AssertOnError();
        }

        [Fact]
        public async void DeleteFilterTest()
        {
            logicMock.Setup(x => x.DeleteFilterAsync(It.IsAny<string>())).ReturnsAsync(true);
            var result = await controller.DeleteFilter("myFilter");
            result.AssertOnError();
        }

        [Fact]
        public async void GetAvailableFilterNameTest()
        {
            logicMock.Setup(x => x.GetAvailableFilterNameAsync(It.IsAny<string>())).ReturnsAsync("myFilter1");
            var result = await controller.GetDefaultFilterName("mFilter");
            result.AssertOnError();
        }

        [Fact]
        public async void GenerateSqlTest()
        {
            var query = new Mock<Filter>();
            logicMock.Setup(x => x.GenerateAdvancedClause(It.IsAny<IEnumerable<Clause>>())).Returns("SELECT * FROM devices");
            var result = await controller.GenerateAdvanceClause(query.Object);
            result.AssertOnError();
        }

        [Fact]
        public async void GetFilterListTest()
        {
            var filterNames = fixture.CreateMany<string>(5);
            logicMock.Setup(x => x.GetFilterList()).ReturnsAsync(filterNames);
            var result = await controller.GetFilterList();
            result.AssertOnError();
        }
    }
}
