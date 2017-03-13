using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Azure.Devices.Applications.RemoteMonitoring.Common.Extensions;
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

        public string PREFIX_REPORTED => "reported.";
        public string PREFIX_DESIRED => "desired.";
        public string PREFIX_TAGS => "tags.";

        public NameCacheLogic(INameCacheRepository nameCacheRepository)
        {
            _nameCacheRepository = nameCacheRepository;
        }

        public async Task<IEnumerable<NameCacheEntity>> GetNameListAsync(NameCacheEntityType type)
        {
            var namelist = await _nameCacheRepository.GetNameListAsync(type);
            return namelist.Where(n => !n.Name.IsReservedTwinName());
        }

        public async Task<bool> AddNameAsync(string name)
        {
            var type = this.GetEntityType(name);
            var entity = new NameCacheEntity()
            {
                Name = name
            };

            return await _nameCacheRepository.AddNameAsync(type, entity);
        }

        public async Task AddShortNamesAsync(NameCacheEntityType type, IEnumerable<string> shortNames)
        {
            var names = shortNames.Select(s =>
            {
                switch (type)
                {
                    case NameCacheEntityType.Tag: return PREFIX_TAGS + s;
                    case NameCacheEntityType.DesiredProperty: return PREFIX_DESIRED + s;
                    case NameCacheEntityType.ReportedProperty: return PREFIX_REPORTED + s;
                    default: throw new ArgumentOutOfRangeException();
                }
            }).ToList();

            await _nameCacheRepository.AddNamesAsync(type, names);
        }

        public async Task<bool> AddMethodAsync(Command method)
        {
            var parameterTypes = method.Parameters.Select(p => p.Type).ToList();
            string normalizedMethodName = string.Format("{0}({1})", method.Name, string.Join(",", parameterTypes));
            var entity = new NameCacheEntity()
            {
                Name = normalizedMethodName,
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

            if (name.StartsWith(PREFIX_REPORTED, StringComparison.Ordinal))
            {
                type = NameCacheEntityType.ReportedProperty;
            }
            else if (name.StartsWith(PREFIX_DESIRED, StringComparison.Ordinal))
            {
                type = NameCacheEntityType.DesiredProperty;
            }
            else if (name.StartsWith(PREFIX_TAGS, StringComparison.Ordinal))
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
