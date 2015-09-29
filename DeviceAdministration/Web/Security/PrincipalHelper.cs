using System.IdentityModel.Claims;
using System.Linq;
using System.Security.Principal;

namespace Microsoft.Azure.Devices.Applications.RemoteMonitoring.DeviceAdmin.Web.Security
{
    public static class PrincipalHelper
    {
        /// <summary>
        /// Helper method to get the email address of the current user for display
        /// </summary>
        /// <param name="principal">Current user IPrincipal object</param>
        /// <returns>Email address as string (if available) or empty string</returns>
        public static string GetEmailAddress(IPrincipal principal)
        {
            // for some account types, this is the email
            if (principal.Identity.Name != null)
            {
                return principal.Identity.Name;
            }

            // if that didn't work, try to cast into a ClaimsPrincipal
            var claimsPrincipal = principal as System.Security.Claims.ClaimsPrincipal;

            if (claimsPrincipal == null || claimsPrincipal.Claims == null)
            {
                // no email available
                return "";
            }

            // try to fish out the email claim
            var emailAddressClaim = claimsPrincipal.Claims.SingleOrDefault(c => c.Type == ClaimTypes.Email);

            if (emailAddressClaim == null)
            {
                return "";
            }

            return emailAddressClaim.Value;
        }
    }
}