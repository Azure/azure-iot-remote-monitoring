using System;
using System.Security.Cryptography;
using Microsoft.Azure.Devices.Applications.RemoteMonitoring.Common.Models;

namespace Microsoft.Azure.Devices.Applications.RemoteMonitoring.DeviceAdmin.Infrastructure.BusinessLogic
{
    /// <summary>
    /// Service to generate a security key pair for a device
    /// </summary>
    public class SecurityKeyGenerator : ISecurityKeyGenerator
    {
        // string will be about 33% longer than this
        private const int _lengthInBytes = 32;

        /// <summary>
        /// Creates a random security key pair
        /// </summary>
        /// <returns>Populated SecurityKeys object</returns>
        public SecurityKeys CreateRandomKeys()
        {
            byte[] primaryRawRandomBytes = new byte[_lengthInBytes];
            byte[] secondaryRawRandomBytes = new byte[_lengthInBytes];

            using (var rngCsp = new RNGCryptoServiceProvider())
            {
                rngCsp.GetBytes(primaryRawRandomBytes);
                rngCsp.GetBytes(secondaryRawRandomBytes);
            }

            string s1 = Convert.ToBase64String(primaryRawRandomBytes);
            string s2 = Convert.ToBase64String(secondaryRawRandomBytes);

            return new SecurityKeys(s1, s2);
        }
    }
}
