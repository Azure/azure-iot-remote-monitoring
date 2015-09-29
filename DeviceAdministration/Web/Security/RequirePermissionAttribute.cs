using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Mvc;

namespace Microsoft.Azure.Devices.Applications.RemoteMonitoring.DeviceAdmin.Web.Security
{
    /// <summary>
    /// Attribute for permission-based security. 
    /// 
    /// Usages:
    /// [RequirePermission(Permission.ViewDevices)]
    /// [RequirePermission(Permission.ViewDevices, Permission.AddDevices)]
    /// </summary>
    public class RequirePermissionAttribute : AuthorizeAttribute 
    {
        public List<Permission> Permissions { get; set; }

        public RequirePermissionAttribute(params Permission[] values) 
        {
            if (values != null)
                this.Permissions = values.ToList();
        }

        /// <summary>
        /// Core logic for permissions-based security checks
        /// </summary>
        /// <param name="httpContext">Current HttpContextBase instance</param>
        /// <returns>True if they have the permission, false otherwise (or if the permission doesn't exist)</returns>
        protected override bool AuthorizeCore(HttpContextBase httpContext)
        {
            var isAuthorized = base.AuthorizeCore(httpContext);
            if (!isAuthorized)
            {
                return false;
            }

            return PermsChecker.HasPermission(this.Permissions);
        }

        /// <summary>
        /// If the user is already logged in, do NOT send them to the login
        /// page. (For Azure AD, that would just create an endless
        /// redirect loop.)
        ///
        /// See http://stackoverflow.com/questions/238437/why-does-authorizeattribute-redirect-to-the-login-page-for-authentication-and-au
        /// </summary>
        /// <param name="filterContext">filterContext</param>
        protected override void HandleUnauthorizedRequest(AuthorizationContext filterContext)
        {
            if (filterContext.HttpContext.Request.IsAuthenticated)
            {
                // return a 403 here (they are logged in and forbidden, so don't send them to login view!)
                filterContext.Result = new HttpStatusCodeResult((int)HttpStatusCode.Forbidden);
            }
            else
            {
                // return a 401 here, which will send them to login
                base.HandleUnauthorizedRequest(filterContext);
            }
        }
    }
}