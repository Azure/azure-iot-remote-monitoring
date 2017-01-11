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

            var result = await DeviceJobHelper.GetJobDetailsAsync(_job, _queryJobTask);
            Assert.Equal("jobName", result.Item1);
            Assert.Equal("filterId", result.Item2);
            Assert.Equal("filterName", result.Item3);
            Assert.Equal(ExtendJobType.ScheduleUpdateTwin.LocalizedString(), result.Item4);

            _queryJobTask = Task.Run(() =>
                new JobRepositoryModel("jobId", "filterId", "jobName", "", ExtendJobType.ScheduleUpdateIcon, null));
            result = await DeviceJobHelper.GetJobDetailsAsync(_job, _queryJobTask);
            Assert.Equal("jobName", result.Item1);
            Assert.Equal("filterId", result.Item2);
            Assert.Equal(_job.QueryCondition, result.Item3);
            Assert.Equal(ExtendJobType.ScheduleUpdateIcon.LocalizedString(), result.Item4);

            _queryJobTask = Task.Run(() =>
                new JobRepositoryModel("jobId", "filterId", "jobName", "*", ExtendJobType.ScheduleUpdateIcon, null));
            result = await DeviceJobHelper.GetJobDetailsAsync(_job, _queryJobTask);
            Assert.Equal("jobName", result.Item1);
            Assert.Equal("filterId", result.Item2);
            Assert.Equal(Strings.AllDevices, result.Item3);
            Assert.Equal(ExtendJobType.ScheduleUpdateIcon.LocalizedString(), result.Item4);

            _job.QueryCondition = null;
            _queryJobTask = Task.Run(() =>
                new JobRepositoryModel("jobId", "filterId", "jobName", "", ExtendJobType.ScheduleRemoveIcon, null));
            result = await DeviceJobHelper.GetJobDetailsAsync(_job, _queryJobTask);
            Assert.Equal("jobName", result.Item1);
            Assert.Equal("filterId", result.Item2);
            Assert.Equal(Strings.NotApplicableValue, result.Item3);
            Assert.Equal(ExtendJobType.ScheduleRemoveIcon.LocalizedString(), result.Item4);

            _queryJobTask = Task.Run(() =>
                new JobRepositoryModel("jobId", "filterId", null, "", ExtendJobType.ScheduleRemoveIcon, null));
            result = await DeviceJobHelper.GetJobDetailsAsync(_job, _queryJobTask);
            Assert.Equal(Strings.NotApplicableValue, result.Item1);
            Assert.Equal("filterId", result.Item2);
            Assert.Equal(Strings.NotApplicableValue, result.Item3);
            Assert.Equal(ExtendJobType.ScheduleRemoveIcon.LocalizedString(), result.Item4);
        }
    }
}
