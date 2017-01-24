using GlobalResources;
using System;
using System.Threading.Tasks;
using Microsoft.Azure.Devices.Applications.RemoteMonitoring.DeviceAdmin.Infrastructure.Models;
using Microsoft.Azure.Devices.Applications.RemoteMonitoring.DeviceAdmin.Web.Extensions;
using Microsoft.Azure.Devices.Applications.RemoteMonitoring.DeviceAdmin.Web.Helpers;
using Microsoft.Azure.Devices.Applications.RemoteMonitoring.DeviceAdmin.Web.Models;
using Ploeh.AutoFixture;
using Xunit;

namespace Microsoft.Azure.Devices.Applications.RemoteMonitoring.UnitTests.Web.Helpers
{
    public class DeviceJobHelperTests
    {
        private IFixture _fixture;
        private DeviceJobModel _job;
        Task<JobRepositoryModel> _queryJobTask;

        public DeviceJobHelperTests()
        {
            _fixture = new Fixture();
            _job = _fixture.Create<DeviceJobModel>();
        }

        [Fact]
        public async Task GetJobDetailsAsyncTest()
        {
            _job = _fixture.Create<DeviceJobModel>();

            _queryJobTask = Task.Run(() =>
                new JobRepositoryModel("jobId", "filterId", "jobName", "filterName", ExtendJobType.ScheduleUpdateTwin, null));

            await DeviceJobHelper.AddMoreDetailsToJobAsync(_job, _queryJobTask);
            Assert.Equal("jobName", _job.JobName);
            Assert.Equal("filterId", _job.FilterId);
            Assert.Equal("filterName", _job.FilterName);
            Assert.Equal(ExtendJobType.ScheduleUpdateTwin.LocalizedString(), _job.OperationType);

            _queryJobTask = Task.Run(() =>
                new JobRepositoryModel("jobId", "filterId", "jobName", "", ExtendJobType.ScheduleUpdateIcon, null));
            await DeviceJobHelper.AddMoreDetailsToJobAsync(_job, _queryJobTask);
            Assert.Equal("jobName", _job.JobName);
            Assert.Equal("filterId", _job.FilterId);
            Assert.Equal(_job.QueryCondition, _job.FilterName);
            Assert.Equal(ExtendJobType.ScheduleUpdateIcon.LocalizedString(), _job.OperationType);

            _queryJobTask = Task.Run(() =>
                new JobRepositoryModel("jobId", "filterId", "jobName", "*", ExtendJobType.ScheduleUpdateIcon, null));
            await DeviceJobHelper.AddMoreDetailsToJobAsync(_job, _queryJobTask);
            Assert.Equal("jobName", _job.JobName);
            Assert.Equal("filterId", _job.FilterId);
            Assert.Equal(Strings.AllDevices, _job.FilterName);
            Assert.Equal(ExtendJobType.ScheduleUpdateIcon.LocalizedString(), _job.OperationType);

            _job.QueryCondition = null;
            _queryJobTask = Task.Run(() =>
                new JobRepositoryModel("jobId", "filterId", "jobName", "", ExtendJobType.ScheduleRemoveIcon, null));
            await DeviceJobHelper.AddMoreDetailsToJobAsync(_job, _queryJobTask);
            Assert.Equal("jobName", _job.JobName);
            Assert.Equal("filterId", _job.FilterId);
            Assert.Equal(Strings.NotApplicableValue, _job.FilterName);
            Assert.Equal(ExtendJobType.ScheduleRemoveIcon.LocalizedString(), _job.OperationType);

            _queryJobTask = Task.Run(() =>
                new JobRepositoryModel("jobId", "filterId", null, "", ExtendJobType.ScheduleRemoveIcon, null));
            await DeviceJobHelper.AddMoreDetailsToJobAsync(_job, _queryJobTask);
            Assert.Equal(Strings.NotApplicableValue, _job.JobName);
            Assert.Equal("filterId", _job.FilterId);
            Assert.Equal(Strings.NotApplicableValue, _job.FilterName);
            Assert.Equal(ExtendJobType.ScheduleRemoveIcon.LocalizedString(), _job.OperationType);
        }
    }
}
