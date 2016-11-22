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
    public class QueryApiControllerTests
    {
        private readonly QueryApiController controller;
        private readonly Mock<IQueryLogic> logicMock;
        private readonly Fixture fixture;

        public QueryApiControllerTests()
        {
            logicMock = new Mock<IQueryLogic>();
            controller = new QueryApiController(logicMock.Object);
            controller.InitializeRequest();
            fixture = new Fixture();
        }

        [Fact]
        public async void GetRecentQueriesTest()
        {
            var queries = fixture.CreateMany<Query>(5);
            logicMock.Setup(x => x.GetRecentQueriesAsync(It.IsAny<int>())).ReturnsAsync(queries.Take(3));
            var result = await controller.GetRecentQueries(3);
            result.AssertOnError();
        }

        [Fact]
        public async void GetQueryTest()
        {
            var query = new Mock<Query>();
            logicMock.Setup(x => x.GetQueryAsync(It.IsAny<string>())).ReturnsAsync(query.Object);
            var result = await controller.GetQuery("myQuery");
            result.AssertOnError();
        }

        [Fact]
        public async void AddQueryTest()
        {
            var query = new Mock<Query>();
            logicMock.Setup(x => x.AddQueryAsync(It.IsAny<Query>())).ReturnsAsync(true);
            var result = await controller.AddQuery(query.Object);
            result.AssertOnError();
        }

        [Fact]
        public async void DeleteQueryTest()
        {
            logicMock.Setup(x => x.DeleteQueryAsync(It.IsAny<string>())).ReturnsAsync(true);
            var result = await controller.DeleteQuery("myQuery");
            result.AssertOnError();
        }

        [Fact]
        public async void GetAvailableQueryNameTest()
        {
            logicMock.Setup(x => x.GetAvailableQueryNameAsync(It.IsAny<string>())).ReturnsAsync("myQuery1");
            var result = await controller.GetAvailableQueryName("myQuery");
            result.AssertOnError();
        }

        [Fact]
        public async void GenerateSqlTest()
        {
            var query = new Mock<Query>();
            logicMock.Setup(x => x.GenerateSql(It.IsAny<IEnumerable<FilterInfo>>())).Returns("SELECT * FROM devices");
            var result = await controller.GenerateSql(query.Object);
            result.AssertOnError();
        }

        [Fact]
        public async void GetQueryNameListTest()
        {
            var queryNames = fixture.CreateMany<string>(5);
            logicMock.Setup(x => x.GetQueryNameList()).ReturnsAsync(queryNames);
            var result = await controller.GetQueryList();
            result.AssertOnError();
        }
    }
}
