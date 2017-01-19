using System.Collections.Generic;
using System.Web;

namespace Microsoft.Azure.Devices.Applications.RemoteMonitoring.DeviceAdmin.Web.Security
{
    /// <summary>
    /// Class to manage the association of roles and permissions.
    /// </summary>
    public class RolePermissions
    {
        private Dictionary<Permission, HashSet<string>> _rolePermissions;
        private List<string> _allRoles;

        // can only view data (and not auth keys!)
        private const string READ_ONLY_ROLE_NAME = "ReadOnly";

        // can do all system functions
        private const string ADMIN_ROLE_NAME = "Admin";

        // default OAuth role name in AAD
        private const string NATIVE_CLIENT_ROLE_NAME = "user_impersonation";

        public RolePermissions()
        {
            _allRoles = new List<string> 
                {
                    ADMIN_ROLE_NAME,
                    READ_ONLY_ROLE_NAME,
                    NATIVE_CLIENT_ROLE_NAME
                };

            _rolePermissions = new Dictionary<Permission, HashSet<string>>();
            DefineRoles();
        }

        public bool HasPermission(Permission permission, HttpContextBase httpContext)
        {
            // get the list of roles that the user must have some overlap with to have the permission
            HashSet<string> rolesRequired = _rolePermissions[permission];

            foreach(var role in _allRoles)
            {
                if (httpContext.User.IsInRole(role) && rolesRequired.Contains(role))
                {
                    return true;
                }
            }

            // fallback for no roles -- give them at least Read Only status

            bool userHasAtLeastOneRole = false;
            foreach(var role in _allRoles)
            {
                if (httpContext.User.IsInRole(role))
                {
                    userHasAtLeastOneRole = true;
                    break;
                }
            }

            if (!userHasAtLeastOneRole)
            {
                if (rolesRequired.Contains(READ_ONLY_ROLE_NAME))
                {
                    return true;
                }
            }

            return false;
        }


        /// <summary>
        /// This method builds the associations between permissions
        /// and roles.
        ///
        /// It goes from permissions to roles, not vice versa.
        /// </summary>
        private void DefineRoles()
        {
            AssignRolesToPermission(Permission.ViewDevices,
                NATIVE_CLIENT_ROLE_NAME,
                READ_ONLY_ROLE_NAME,
                ADMIN_ROLE_NAME);

            AssignRolesToPermission(Permission.ViewActions,
                READ_ONLY_ROLE_NAME,
                ADMIN_ROLE_NAME);
            
            AssignRolesToPermission(Permission.AssignAction,
                ADMIN_ROLE_NAME);

            AssignRolesToPermission(Permission.DisableEnableDevices,
                ADMIN_ROLE_NAME);

            AssignRolesToPermission(Permission.EditDeviceMetadata,
                ADMIN_ROLE_NAME);

            AssignRolesToPermission(Permission.AddDevices,
                ADMIN_ROLE_NAME);

            AssignRolesToPermission(Permission.RemoveDevices,
                ADMIN_ROLE_NAME);

            AssignRolesToPermission(Permission.SendCommandToDevices,
                ADMIN_ROLE_NAME);

            AssignRolesToPermission(Permission.ViewDeviceSecurityKeys,
                ADMIN_ROLE_NAME);

            AssignRolesToPermission(Permission.ViewRules,
                READ_ONLY_ROLE_NAME,
                ADMIN_ROLE_NAME);

            AssignRolesToPermission(Permission.ViewTelemetry,
                NATIVE_CLIENT_ROLE_NAME,
                READ_ONLY_ROLE_NAME,
                ADMIN_ROLE_NAME);

            AssignRolesToPermission(Permission.EditRules,
                ADMIN_ROLE_NAME);

            AssignRolesToPermission(Permission.DeleteRules,
                ADMIN_ROLE_NAME);

            AssignRolesToPermission(Permission.HealthBeat,
                ADMIN_ROLE_NAME);

            AssignRolesToPermission(Permission.LogicApps,
                ADMIN_ROLE_NAME);

            AssignRolesToPermission(Permission.CellularConn,
                ADMIN_ROLE_NAME);

            AssignRolesToPermission(Permission.ViewJobs,
                NATIVE_CLIENT_ROLE_NAME,
                READ_ONLY_ROLE_NAME,
                ADMIN_ROLE_NAME);

            AssignRolesToPermission(Permission.ManageJobs,
                ADMIN_ROLE_NAME);

            AssignRolesToPermission(Permission.SaveDeviceListColumnsAsGlobal,
                ADMIN_ROLE_NAME);

            AssignRolesToPermission(Permission.DeleteSuggestedClauses,
                ADMIN_ROLE_NAME);
        }

        /// <summary>
        /// Helper method to assign a permission to a set of roles
        /// </summary>
        /// <param name="permission">Permission to assign</param>
        /// <param name="roles">Roles to assign the permission to</param>
        private void AssignRolesToPermission(Permission permission, params string[] roles)
        {
            var rolesHashSet = new HashSet<string>();

            // add each role that grants this permission to the set of granting permissions
            foreach(string r in roles)
            {
                rolesHashSet.Add(r);
            }

            // add the permission and granting roles to the data structure
            _rolePermissions.Add(permission, rolesHashSet);
        }
    }
}
