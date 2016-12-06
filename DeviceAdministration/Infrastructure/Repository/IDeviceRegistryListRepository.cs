using System.Threading.Tasks;
using Microsoft.Azure.Devices.Applications.RemoteMonitoring.DeviceAdmin.Infrastructure.Models;

namespace Microsoft.Azure.Devices.Applications.RemoteMonitoring.DeviceAdmin.Infrastructure.Repository
{
    public interface IDeviceRegistryListRepository
    {
        /// <summary>
        /// Gets a list of type Device depending on search parameters, sort column, sort direction,
        /// starting point, page size, and filters.
        /// </summary>
        /// <param name="filter">The device filter.</param>
        /// <returns></returns>
        Task<DeviceListFilterResult> GetDeviceList(DeviceListFilter filter);
    }
}