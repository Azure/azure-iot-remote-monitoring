using System;
using System.Diagnostics;
using System.Globalization;
using System.IdentityModel.Claims;
using System.Threading;
using System.Web;
using System.Web.Helpers;
using System.Web.Mvc;
using System.Web.Optimization;
using System.Web.Routing;

namespace Microsoft.Azure.Devices.Applications.RemoteMonitoring.DeviceAdmin.Web
{
    using App_Start;
    using Helpers;

    public class MvcApplication : System.Web.HttpApplication
    {
        protected void Application_Start()
        {
            AreaRegistration.RegisterAllAreas();
            FilterConfig.RegisterGlobalFilters(GlobalFilters.Filters);
            RouteConfig.RegisterRoutes(RouteTable.Routes);
            BundleConfig.RegisterBundles(BundleTable.Bundles);
            GlobalFilters.Filters.Add(new HandleErrorAttribute());
            AntiForgeryConfig.UniqueClaimTypeIdentifier = ClaimTypes.NameIdentifier;
            ControllerBuilder.Current.DefaultNamespaces.Add("Microsoft.Azure.Devices.Applications.RemoteMonitoring.DeviceAdmin.Web.WebApiControllers");

            // Reset JobList and QueryList via actual current job history for debugging
            //RepositoryInitializer.SeedTablesAsync().Wait();
            ModelBinders.Binders.Add(typeof(DateTime), new DateTimeBinder() { TimeFormat = "h:m:ss tt", DateFormat= "MMMM d, yyyy h:mm tt" });
            ModelBinders.Binders.Add(typeof(DateTime?), new NullableDateTimeBinder() { TimeFormat = "h:m:ss tt", DateFormat = "MMMM d, yyyy h:mm tt" });
        }

        // Require HTTPS for all requests processed by ASP.NET
        protected void Application_BeginRequest(Object sender, EventArgs e)
        {
            Thread.CurrentThread.CurrentCulture = Thread.CurrentThread.CurrentUICulture = GetSelectedCulture();

            if (Context.Request.IsSecureConnection)
            {
                // HSTS blocks access to sites with invalid certs
                bool usingValidTlsCert = false;

                // tell the browser that this site is ALWAYS https (but only if the cert is valid!)
                if (usingValidTlsCert)
                {
                    // note: to clear this from a browser, set the header with "max-age=0"
                    Response.AddHeader("Strict-Transport-Security", "max-age=3600");
                }
            }
            else
            {
                // (if we are serving HTTP) redirect users to HTTPS
                Response.RedirectPermanent(Context.Request.Url.ToString().Replace("http://", "https://"), false);
                CompleteRequest();
            }
        }

        protected void Application_EndRequest()
        {
            // This implementation is a work around for the HTTP 302 issue
            // with MVC and ajax requests. The code below modifies the status code
            // to 401 if the status is 302 and the request is ajax based.
            var context = new HttpContextWrapper(Context);
            if (context.Response.StatusCode == 302 && GetIsServiceCall(context))
            {
                context.Response.Clear();
                context.Response.StatusCode = 401;
            }
        }

        private CultureInfo GetSelectedCulture()
        {
            string cultureName;

            // Attempt to read the culture cookie from Request
            HttpCookie cultureCookie = this.Request.Cookies["_culture"];

            if (cultureCookie != null)
            {
                cultureName = cultureCookie.Value;
            }
            else
            {
                // Obtain it from HTTP header AcceptLanguages
                cultureName = this.Request.UserLanguages != null && this.Request.UserLanguages.Length > 0 ? this.Request.UserLanguages[0] : null;
            }

            // Validate culture name
            var culture = CultureHelper.GetClosestCulture(cultureName);

            // Modify current thread's cultures            
            return culture;
        }

        private bool GetIsServiceCall(HttpContextWrapper contextWrapper)
        {
            Debug.Assert(contextWrapper != null, "contextWrapper is a null reference.");

            if (contextWrapper.Request.IsAjaxRequest())
            {
                return true;
            }

            string apiPath = VirtualPathUtility.ToAbsolute("~/api/");
            return contextWrapper.Request.Url.LocalPath.StartsWith(apiPath, StringComparison.Ordinal);
        }
    }
}
