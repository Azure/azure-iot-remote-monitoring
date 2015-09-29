using System.Threading.Tasks;
using Microsoft.Azure.Devices.Applications.RemoteMonitoring.Common.Models;

namespace Microsoft.Azure.Devices.Applications.RemoteMonitoring.DeviceAdmin.Infrastructure.BusinessLogic
{
    public class KeyLogic : IKeyLogic
    {
        private readonly ISecurityKeyGenerator _keyGenerator;

        public KeyLogic(ISecurityKeyGenerator keyGenerator)
        {
            _keyGenerator = keyGenerator;
        }

        public async Task<SecurityKeys> GetKeysAsync()
        {
            return await Task.Run(() => { return _keyGenerator.CreateRandomKeys(); });
        }
    }
}
