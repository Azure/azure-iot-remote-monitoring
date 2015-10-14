/* Uncomment the line below to turn off
 * CSRF protection. This is typically done
 * when testing the Web API with a browser
 * extension
 */
 //#define SUPPRESS_CSRF_PROTECTION

using System;
using System.Globalization;
using System.Web.Http;

namespace Microsoft.Azure.Devices.Applications.RemoteMonitoring.DeviceAdmin.Web.Security
{   
    /// <summary>
    /// The purpose of this attribute is to handle CSRF for a Web API controller
    /// </summary>
    public class WebApiCSRFValidationAttribute : AuthorizeAttribute
    {

        /// <summary>
        /// Validates if the origin or referrer match the host
        /// </summary>
        /// <param name="actionContext"></param>
        /// <returns>Returns true if the origin or referrer match the host. If both are null then the request is considered valid and true is returned.</returns>
        protected override bool IsAuthorized(System.Web.Http.Controllers.HttpActionContext actionContext)
        {
#if SUPPRESS_CSRF_PROTECTION
            return true;
#warning CSRF Validation is turned off!

#if !DEBUG
#error CSRF Validation is turned off!
#endif
#else
     
            var request = System.Web.HttpContext.Current.Request;

            var origin = request.Headers["origin"];
            var referer = request.Headers["referer"];
            var host = request.Url.Host;
            var scheme = request.Url.Scheme;

            var baseUrl = 
                string.Format(
                    CultureInfo.InvariantCulture, 
                    "{0}://{1}", 
                    scheme, 
                    host);

            var valid = true;

            if (origin != null)
            {
                valid =
                    origin.StartsWith(
                        baseUrl, 
                        StringComparison.OrdinalIgnoreCase);
            }
            else if (referer != null)
            {
                valid =
                    referer.StartsWith(
                        baseUrl,
                        StringComparison.OrdinalIgnoreCase);
            }

            return valid;
            
#endif
        }
    }
}