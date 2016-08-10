using System.Collections.Generic;
using System.Linq;
using Microsoft.Azure.Devices.Applications.RemoteMonitoring.Common.Configurations;
using Microsoft.Azure.Devices.Applications.RemoteMonitoring.Common.Helpers;
using Microsoft.Azure.Devices.Applications.RemoteMonitoring.Common.Models;
using Microsoft.Azure.Devices.Applications.RemoteMonitoring.DeviceAdmin.Infrastructure.Repository;
using Microsoft.WindowsAzure.Storage.Table;
using Moq;
using Newtonsoft.Json.Linq;
using Ploeh.AutoFixture;
using Ploeh.AutoFixture.AutoMoq;
using Xunit;

namespace Microsoft.Azure.Devices.Applications.RemoteMonitoring.UnitTests.Infrastructure
{
    public class ApiRegistrationRepositoryTest
    {
        private readonly Mock<IAzureTableStorageClient> _tableStorageClientMock;
        private readonly ApiRegistrationRepository apiRegistrationRepository;
        private readonly IFixture fixture;

        public ApiRegistrationRepositoryTest()
        {
            fixture = new Fixture();
            var configProviderMock = new Mock<IConfigurationProvider>();
            configProviderMock.Setup(x => x.GetConfigurationSettingValue(It.IsNotNull<string>()))
                .ReturnsUsingFixture(fixture);
            _tableStorageClientMock = new Mock<IAzureTableStorageClient>();
            var tableStorageClientFactory = new AzureTableStorageClientFactory(_tableStorageClientMock.Object);
            apiRegistrationRepository = new ApiRegistrationRepository(configProviderMock.Object,
                tableStorageClientFactory);
        }

        [Fact]
        public void AmendRegistrationTest()
        {
            var apiRegistrationModel = fixture.Create<ApiRegistrationModel>();
            TableOperation savedOp = null;
            _tableStorageClientMock.Setup(x => x.Execute(It.IsNotNull<TableOperation>()))
                .Callback<TableOperation>(op => savedOp = op)
                .Returns(new TableResult());
            var ret = apiRegistrationRepository.AmendRegistration(apiRegistrationModel);
            Assert.True(ret);
            Assert.NotNull(savedOp);
        }

        [Fact]
        public void RecieveDetailsTest()
        {
            var tableEntities = fixture.Create<List<ApiRegistrationTableEntity>>();
            TableQuery<ApiRegistrationTableEntity> savedOp = null;
            _tableStorageClientMock.Setup(x => x.ExecuteQuery(It.IsNotNull<TableQuery<ApiRegistrationTableEntity>>()))
                .Callback<TableQuery<ApiRegistrationTableEntity>>(op => savedOp = op)
                .Returns(tableEntities);
            var ret = apiRegistrationRepository.RecieveDetails();
            Assert.NotNull(ret);
            Assert.NotNull(savedOp);
            Assert.Equal(ret.Username, tableEntities.First().Username);
            Assert.Equal(ret.BaseUrl, tableEntities.First().BaseUrl);
            Assert.Equal(ret.LicenceKey, tableEntities.First().LicenceKey);
            Assert.Equal(ret.Password, tableEntities.First().Password);
        }

        [Fact]
        public void IsApiRegisteredInAzureTest()
        {
            TableOperation savedOp = null;
            _tableStorageClientMock.Setup(x => x.Execute(It.IsNotNull<TableOperation>()))
                .Callback<TableOperation>(op => savedOp = op)
                .Returns(new TableResult {Result = new JObject()});
            var ret = apiRegistrationRepository.IsApiRegisteredInAzure();
            Assert.True(ret);
            Assert.NotNull(savedOp);
        }

        [Fact]
        public void DeleteApiDetailsTest()
        {
            TableOperation savedOp = null;
            _tableStorageClientMock.Setup(x => x.Execute(It.IsNotNull<TableOperation>()))
                .Callback<TableOperation>(op => savedOp = op)
                .Returns(new TableResult());
            var ret = apiRegistrationRepository.DeleteApiDetails();
            Assert.True(ret);
            Assert.NotNull(savedOp);
        }
    }
}