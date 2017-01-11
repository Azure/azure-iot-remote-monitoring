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
        public static async Task<Tuple<string, string, string, string>> GetJobDetailsAsync(DeviceJobModel job, Task<JobRepositoryModel> queryJobRepositoryTask)
        {
            try
            {
                JobRepositoryModel repositoryModel = await queryJobRepositoryTask;

                string filterId = repositoryModel.FilterId;
                string jobType = string.Empty;
                // The Extended JobType is missing in the table, use the origin JobType
                if (repositoryModel.JobType == ExtendJobType.Unknown)
                {
                    jobType = job.OperationType;
                }
                else
                {
                    jobType = repositoryModel.JobType.LocalizedString();
                }

                string filterName = repositoryModel.FilterName;
                if (string.IsNullOrEmpty(filterName))
                {
                    filterName = job.QueryCondition ?? Strings.NotApplicableValue;
                }
                if (filterName == "*" || DeviceListFilterRepository.DefaultDeviceListFilter.Id.Equals(filterId))
                {
                    filterName = Strings.AllDevices;
                }

                return Tuple.Create(repositoryModel.JobName ?? Strings.NotApplicableValue, filterId, filterName, jobType);
            }
            catch
            {
                string externalJobName = string.Format(Strings.ExternalJobNamePrefix, job.JobId);
                return Tuple.Create(externalJobName, string.Empty, job.QueryCondition ?? Strings.NotApplicableValue, job.OperationType);
            }
        }
    }
}