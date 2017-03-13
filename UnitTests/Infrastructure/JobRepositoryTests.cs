using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Azure.Devices.Applications.RemoteMonitoring.Common.Configurations;
using Microsoft.Azure.Devices.Applications.RemoteMonitoring.Common.Helpers;
using Microsoft.Azure.Devices.Applications.RemoteMonitoring.Common.Models;
using Microsoft.Azure.Devices.Applications.RemoteMonitoring.DeviceAdmin.Infrastructure.Exceptions;
using Microsoft.Azure.Devices.Applications.RemoteMonitoring.DeviceAdmin.Infrastructure.Models;
using Microsoft.Azure.Devices.Applications.RemoteMonitoring.DeviceAdmin.Infrastructure.Repository;
using Microsoft.WindowsAzure.Storage.Table;
using Moq;
using Ploeh.AutoFixture;
using Ploeh.AutoFixture.AutoMoq;
using Xunit;

namespace Microsoft.Azure.Devices.Applications.RemoteMonitoring.UnitTests.Infrastructure
{
    public class JobRepositoryTests
    {
        private readonly IFixture _fixture;
        private readonly Mock<IConfigurationProvider> _configurationProviderMock;
        private readonly Mock<IAzureTableStorageClient> _tableStorageClientMock;
        private readonly JobRepository _repository;

        public JobRepositoryTests()
        {
            _fixture = new Fixture();
            _fixture.Customize(new AutoConfiguredMoqCustomization());

            _configurationProviderMock = new Mock<IConfigurationProvider>();
            _configurationProviderMock.Setup(x => x.GetConfigurationSettingValue(It.IsNotNull<string>()))
                .ReturnsUsingFixture(_fixture);

            _tableStorageClientMock = new Mock<IAzureTableStorageClient>();
            var tableStorageClientFactory = new AzureTableStorageClientFactory(_tableStorageClientMock.Object);

            _repository = new JobRepository(_configurationProviderMock.Object, tableStorageClientFactory);
        }

        [Fact]
        public async Task AddAsyncTest()
        {
            var job = _fixture.Create<JobRepositoryModel>();

            _tableStorageClientMock
                .Setup(x => x.DoTableInsertOrReplaceAsync(
                    It.IsAny<JobTableEntity>(),
                    It.IsAny<Func<JobTableEntity, object>>()))
                .ReturnsAsync(new TableStorageResponse<object> { Status = TableStorageResponseStatus.Successful });

            await _repository.AddAsync(job);

            _tableStorageClientMock
                .Verify(x => x.DoTableInsertOrReplaceAsync(
                    It.Is<JobTableEntity>(e => e.JobId == job.JobId
                        && e.FilterName == job.FilterName
                        && e.JobName == job.JobName
                        && e.FilterId == job.FilterId
                        && e.PartitionKey == job.JobId
                        && e.RowKey == job.FilterId),
                    It.IsAny<Func<JobTableEntity, object>>()));
        }

        [Fact]
        public async Task AddAsyncShouldThrowJobRepositorySaveExceptionIfFailed()
        {
            var job = _fixture.Create<JobRepositoryModel>();

            _tableStorageClientMock
                .Setup(x => x.DoTableInsertOrReplaceAsync(
                    It.IsAny<JobTableEntity>(),
                    It.IsAny<Func<JobTableEntity, object>>()))
                .ReturnsAsync(new TableStorageResponse<object> { Status = TableStorageResponseStatus.UnknownError });

            await Assert.ThrowsAsync<JobRepositorySaveException>(() => _repository.AddAsync(job));
        }

        [Fact]
        public async Task DeleteAsyncTest()
        {
            var jobId = _fixture.Create<string>();
            var entity = _fixture.Create<JobTableEntity>();

            _tableStorageClientMock
                .Setup(x => x.ExecuteQueryAsync(It.Is<TableQuery<JobTableEntity>>(q => IsCorrectQueryForJobId(q, jobId))))
                .ReturnsAsync(new JobTableEntity[] { entity });

            _tableStorageClientMock
                .Setup(x => x.DoDeleteAsync(entity, It.IsAny<Func<JobTableEntity, object>>()))
                .ReturnsAsync(new TableStorageResponse<object> { Status = TableStorageResponseStatus.Successful });

            await _repository.DeleteAsync(jobId);

            _tableStorageClientMock
                .Verify(x => x.DoDeleteAsync(entity, It.IsAny<Func<JobTableEntity, object>>()));
        }

        [Fact]
        public async Task DeleteAsyncShouldThrowJobRepositoryRemoveExceptionIfFailed()
        {
            var jobId = _fixture.Create<string>();
            var entity = _fixture.Create<JobTableEntity>();

            _tableStorageClientMock
                .Setup(x => x.ExecuteQueryAsync(It.Is<TableQuery<JobTableEntity>>(q => IsCorrectQueryForJobId(q, jobId))))
                .ReturnsAsync(new JobTableEntity[] { entity });

            _tableStorageClientMock
                .Setup(x => x.DoDeleteAsync(entity, It.IsAny<Func<JobTableEntity, object>>()))
                .ReturnsAsync(new TableStorageResponse<object> { Status = TableStorageResponseStatus.UnknownError });

            await Assert.ThrowsAsync<JobRepositoryRemoveException>(() => _repository.DeleteAsync(jobId));
        }

        [Fact]
        public async Task QueryByJobIDAsyncTest()
        {
            var jobId = _fixture.Create<string>();
            var entity = _fixture.Create<JobTableEntity>();

            _tableStorageClientMock
                .Setup(x => x.ExecuteQueryAsync(It.Is<TableQuery<JobTableEntity>>(q => IsCorrectQueryForJobId(q, jobId))))
                .ReturnsAsync(new JobTableEntity[] { entity });

            var model = await _repository.QueryByJobIDAsync(jobId);

            Assert.True(IsModelCreatedByEntity(model, entity));
        }

        [Fact]
        public async Task UpdateAssociatedFilterNameAsyncTest()
        {
            var job = _fixture.Create<JobTableEntity>();
            var jobs = new List<JobRepositoryModel>()
            {
                new JobRepositoryModel(job)
            };
            _tableStorageClientMock.Setup(x => x.ExecuteAsync(It.IsAny<TableOperation>())).ReturnsAsync(new TableResult() { Result = job });
            var ret = await _repository.UpdateAssociatedFilterNameAsync(jobs);
            Assert.NotNull(ret);
            Assert.Equal(jobs.Select(j => j.FilterName), ret.Select(r => r.FilterName));
        }

        [Fact]
        public async Task QueryByJobIDAsyncShouldThrowArgumentNullExceptionIfNullProvidedAsParameter()
        {
            await Assert.ThrowsAsync<ArgumentNullException>(() => _repository.QueryByJobIDAsync(null));
        }

        [Fact]
        public async Task QueryByJobIDAsyncShouldThrowJobNotFoundExceptionIfNoJobFound()
        {
            var jobId = _fixture.Create<string>();

            _tableStorageClientMock
                .Setup(x => x.ExecuteQueryAsync(It.Is<TableQuery<JobTableEntity>>(q => IsCorrectQueryForJobId(q, jobId))))
                .ReturnsAsync(new JobTableEntity[] { });

            await Assert.ThrowsAsync<JobNotFoundException>(() => _repository.QueryByJobIDAsync(jobId));
        }

        [Fact]
        public async Task QueryByJobIDAsyncShouldThrowDuplicatedJobFoundExceptionIfMultipleJobsFound()
        {
            var jobId = _fixture.Create<string>();
            var entities = _fixture.CreateMany<JobTableEntity>();

            _tableStorageClientMock
                .Setup(x => x.ExecuteQueryAsync(It.Is<TableQuery<JobTableEntity>>(q => IsCorrectQueryForJobId(q, jobId))))
                .ReturnsAsync(entities);

            await Assert.ThrowsAsync<DuplicatedJobFoundException>(() => _repository.QueryByJobIDAsync(jobId));
        }

        [Fact]
        public async Task QueryByQueryNameAsyncTest()
        {
            var queryName = _fixture.Create<string>();
            var entities = _fixture.CreateMany<JobTableEntity>();

            _tableStorageClientMock
                .Setup(x => x.ExecuteQueryAsync(It.Is<TableQuery<JobTableEntity>>(q => IsCorrectQueryForQueryName(q, queryName))))
                .ReturnsAsync(entities);

            var models = await _repository.QueryByFilterIdAsync(queryName);

            Assert.Equal(models.Count(), entities.Count());
            Assert.True(models.All(m => entities.Any(e => IsModelCreatedByEntity(m, e))));
        }

        [Fact]
        public async Task QueryByQueryNameAsyncShouldThrowArgumentNullExceptionIfNullProvidedAsParameter()
        {
            await Assert.ThrowsAsync<ArgumentNullException>(() => _repository.QueryByFilterIdAsync(null));
        }

        private bool IsCorrectQueryForJobId(TableQuery<JobTableEntity> query, string jobId)
        {
            return query.FilterString == FormattableString.Invariant($"PartitionKey eq '{jobId}'");
        }

        private bool IsCorrectQueryForQueryName(TableQuery<JobTableEntity> query, string queryName)
        {
            return query.FilterString == FormattableString.Invariant($"RowKey eq '{queryName}'");
        }

        private bool IsModelCreatedByEntity(JobRepositoryModel model, JobTableEntity entity)
        {
            return model.JobId == entity.JobId
                && model.FilterName == entity.FilterName
                && model.JobName == entity.JobName;
        }
    }
}
