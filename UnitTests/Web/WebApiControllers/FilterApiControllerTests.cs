using System;
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
    public class FilterApiControllerTests : IDisposable
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
        public async void SaveFilterTest()
        {
            var filter = new Mock<Filter>();
            logicMock.Setup(x => x.SaveFilterAsync(It.IsAny<Filter>())).ReturnsAsync(filter.Object);
            var result = await controller.SaveFilter(filter.Object);
            result.AssertOnError();
        }

        [Fact]
        public async void DeleteFilterTest()
        {
            logicMock.Setup(x => x.DeleteFilterAsync(It.IsAny<string>(), It.IsAny<bool>())).ReturnsAsync(true);
            var result = await controller.DeleteFilter("myFilter");
            result.AssertOnError();
        }

        [Fact]
        public async void GetAvailableFilterNameTest()
        {
            logicMock.Setup(x => x.GetAvailableFilterNameAsync(It.IsAny<string>())).ReturnsAsync("myFilter1");
            var result = await controller.GetDefaultFilterName("myFilter1");
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
            var filters = fixture.CreateMany<Filter>(5);
            logicMock.Setup(x => x.GetFilterList(0, 1000)).ReturnsAsync(filters);
            var result = await controller.GetFilterList(0, 1000);
            result.AssertOnError();
        }

        [Fact]
        public async void GetSuggestClausesTest()
        {
            var clauses = fixture.CreateMany<Clause>(5);
            logicMock.Setup(x => x.GetSuggestClauses(0, 1000)).ReturnsAsync(clauses);
            var result = await controller.GetSuggestClauses(0, 1000);
            result.AssertOnError();
        }

        [Fact]
        public async void DeleteSuggestClausesTest()
        {
            var clauses = fixture.CreateMany<Clause>(5);
            logicMock.Setup(x => x.DeleteSuggestClausesAsync(clauses)).ReturnsAsync(3);
            var result = await controller.DeleteSuggestClauses(clauses);
            result.AssertOnError();
        }

        #region IDisposable Support
        private bool disposedValue = false; // To detect redundant calls

        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                    controller.Dispose();
                }

                // TODO: free unmanaged resources (unmanaged objects) and override a finalizer below.
                // TODO: set large fields to null.

                disposedValue = true;
            }
        }

        // TODO: override a finalizer only if Dispose(bool disposing) above has code to free unmanaged resources.
        // ~FilterApiControllerTests() {
        //   // Do not change this code. Put cleanup code in Dispose(bool disposing) above.
        //   Dispose(false);
        // }

        // This code added to correctly implement the disposable pattern.
        public void Dispose()
        {
            // Do not change this code. Put cleanup code in Dispose(bool disposing) above.
            Dispose(true);
            // TODO: uncomment the following line if the finalizer is overridden above.
            // GC.SuppressFinalize(this);
        }
        #endregion
    }
}
