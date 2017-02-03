using GlobalResources;
using System;
using System.Threading.Tasks;
using Microsoft.Azure.Devices.Applications.RemoteMonitoring.DeviceAdmin.Infrastructure.Models;
using Microsoft.Azure.Devices.Applications.RemoteMonitoring.DeviceAdmin.Infrastructure.Repository;
using Microsoft.Azure.Devices.Applications.RemoteMonitoring.DeviceAdmin.Web.Extensions;
using Microsoft.Azure.Devices.Applications.RemoteMonitoring.DeviceAdmin.Web.Models;


namespace Microsoft.Azure.Devices.Applications.RemoteMonitoring.DeviceAdmin.Web.Helpers
{
    public class DeviceJobHelper
    {
        public static async Task AddMoreDetailsToJobAsync(DeviceJobModel job, Task<JobRepositoryModel> queryJobRepositoryTask)
        {
            try
            {
                JobRepositoryModel repositoryModel = await queryJobRepositoryTask;

                job.JobName = repositoryModel.JobName ?? Strings.NotApplicableValue;
                job.FilterId = repositoryModel.FilterId;

                string filterName = repositoryModel.FilterName;
                if (string.IsNullOrEmpty(filterName))
                {
                    filterName = job.QueryCondition ?? Strings.NotApplicableValue;
                }
                if (filterName == "*" || DeviceListFilterRepository.DefaultDeviceListFilter.Id.Equals(job.FilterId))
                {
                    filterName = Strings.AllDevices;
                }
                job.FilterName = filterName;

                if (repositoryModel.JobType != ExtendJobType.Unknown)
                {
                    job.OperationType = repositoryModel.JobType.LocalizedString();
                }
            }
            catch
            {
                job.JobName = string.Format(Strings.ExternalJobNamePrefix, job.JobId);
                job.FilterId = string.Empty;
                job.FilterName = job.QueryCondition ?? Strings.NotApplicableValue;
            }
        }
    }
}