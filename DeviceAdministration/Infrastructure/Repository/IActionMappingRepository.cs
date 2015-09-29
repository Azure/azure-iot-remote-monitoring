using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Azure.Devices.Applications.RemoteMonitoring.DeviceAdmin.Infrastructure.Models;

namespace Microsoft.Azure.Devices.Applications.RemoteMonitoring.DeviceAdmin.Infrastructure.Repository
{
    /// <summary>
    /// Defines the interface to the actions repository, which stores the 
    /// mappings of ActionId values to logic app actions.
    /// </summary>
    public interface IActionMappingRepository
    {
        Task<List<ActionMapping>> GetAllMappingsAsync();
        Task SaveMappingAsync(ActionMapping m);
    }
}
