using System.Collections.Generic;
using System.Linq;
using System.Web.Http;

namespace Microsoft.Azure.Devices.Applications.RemoteMonitoring.DeviceAdmin.Web.Security
{
    /// <summary>
    /// Attribute for permission-based security for Web API controllers. 
    /// 
    /// Usages:
    /// [WebApiRequirePermission(Permission.ViewDevices)]
    /// [WebApiRequirePermission(Permission.ViewDevices, Permission.AddDevices)]
    /// </summary>
    public class WebApiRequirePermissionAttribute : AuthorizeAttribute
    {
        public List<Permission> Permissions { get; set; }

        public WebApiRequirePermissionAttribute(params Permission[] values)
        {
            if (values != null)
                this.Permissions = values.ToList();
        }

        /// <summary>
        /// Validates that the current user has permisson
        /// </summary>
        /// <param name="actionContext"></param>
        /// <returns></returns>
        protected override bool IsAuthorized(System.Web.Http.Controllers.HttpActionContext actionContext)
        {
            return PermsChecker.HasPermission(this.Permissions);
        }
    }
}