using Microsoft.Azure.Devices.Applications.RemoteMonitoring.Common.Models;

namespace Microsoft.Azure.Devices.Applications.RemoteMonitoring.DeviceAdmin.Infrastructure.BusinessLogic
{
    public interface ISecurityKeyGenerator
    {
        /// <summary>
        /// Creates a random security key pair
        /// </summary>
        /// <returns>Populated SecurityKeys object</returns>
        SecurityKeys CreateRandomKeys();
    }
}