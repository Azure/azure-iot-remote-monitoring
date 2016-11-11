using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Azure.Devices.Applications.RemoteMonitoring.Common.Models.Commands;
using Microsoft.Azure.Devices.Applications.RemoteMonitoring.DeviceAdmin.Infrastructure.Models;
using Microsoft.Azure.Devices.Applications.RemoteMonitoring.DeviceAdmin.Infrastructure.Repository;

namespace Microsoft.Azure.Devices.Applications.RemoteMonitoring.DeviceAdmin.Infrastructure.BusinessLogic
{
    /// <summary>
    /// Business logic around different types of devices
    /// </summary>
    public class NameCacheLogic : INameCacheLogic
    {
        private readonly INameCacheRepository _nameCacheRepository;
        private const string PREFIX_REPORTED = "reported.";
        private const string PREFIX_DESIRED = "desired.";
        private const string PREFIX_TAGS = "tags.";

        public NameCacheLogic(INameCacheRepository nameCacheRepository)
        {
            _nameCacheRepository = nameCacheRepository;
        }

        public async Task<IEnumerable<NameCacheEntity>> GetNameListAsync(NameCacheEntityType type)
        {
            return await _nameCacheRepository.GetNameListAsync(type);
        }

        public async Task<bool> AddNameAsync(string name)
        {
            var type = this.GetEntityType(name);
            var entity = new NameCacheEntity() {
                Name = name
            };

            return await _nameCacheRepository.AddNameAsync(type, entity);
        }

        public async Task<bool> AddMethodAsync(Command method)
        {
            var entity = new NameCacheEntity()
            {
                Name = method.Name,
                Description = method.Description,
                Parameters = method.Parameters
            };

            return await _nameCacheRepository.AddNameAsync(NameCacheEntityType.Method, entity);
        }

        public async Task<bool> DeleteNameAsync(string name)
        {
            var type = this.GetEntityType(name);
            return await _nameCacheRepository.DeleteNameAsync(type, name);
        }

        public async Task<bool> DeleteMethodAsync(string name)
        {
            return await _nameCacheRepository.DeleteNameAsync(NameCacheEntityType.Method, name);
        }

        private NameCacheEntityType GetEntityType(string name)
        {
            NameCacheEntityType type;

            if (name.StartsWith(PREFIX_REPORTED))
            {
                type = NameCacheEntityType.ReportedProperty;
            }
            else if (name.StartsWith(PREFIX_DESIRED))
            {
                type = NameCacheEntityType.DesiredProperty;
            }
            else if (name.StartsWith(PREFIX_TAGS))
            {
                type = NameCacheEntityType.Tag;
            }
            else 
            {
                type = NameCacheEntityType.DeviceInfo;
            }

            return type;
        }
    }
}
