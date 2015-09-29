using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Microsoft.Azure.Devices.Applications.RemoteMonitoring.DeviceAdmin.Web.Security
{
    /// <summary>
    /// Helper class for checking permissions in code.
    /// </summary>
    public static class PermsChecker
    {
        private static readonly RolePermissions _rolePermissions;

        static PermsChecker()
        {
            _rolePermissions = new RolePermissions();
        }

        /// <summary>
        /// Call this method in code to determine if a user has a given permission
        /// </summary>
        /// <param name="permission">Permission to check for</param>
        /// <returns>True if they have it</returns>
        public static bool HasPermission(Permission permission)
        {
            return _rolePermissions.HasPermission(permission, new HttpContextWrapper(HttpContext.Current));
        }

        /// <summary>
        /// Call this method in code to determine if a user has a given permissions
        /// </summary>
        /// <param name="permissions"></param>
        /// <returns></returns>
        public static bool HasPermission(List<Permission> permissions) 
        {
            var httpContext = new HttpContextWrapper(HttpContext.Current);

            if (permissions == null || !permissions.Any())
            {
                return true;
            }

            // return true only if the user has ALL permissions
            return permissions
                    .Select(p => _rolePermissions.HasPermission(p, httpContext))
                    .All(val => val == true);
        }
    }
}