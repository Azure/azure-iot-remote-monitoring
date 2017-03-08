using System.Web.Optimization;

namespace Microsoft.Azure.Devices.Applications.RemoteMonitoring.DeviceAdmin.Web
{
    using System.Web;
    using Bundling;

    public sealed class BundleConfig
    {
        // For more information on bundling, visit http://go.microsoft.com/fwlink/?LinkId=301862
        public static void RegisterBundles(BundleCollection bundles)
        {
            // jquery is included in powerbi-visuals.all.min.js
            bundles.Add(new ScriptBundle("~/bundles/powerbi-visuals")
                .Include("~/Scripts/powerbi-visuals.all.min.js"));

            bundles.Add(new ScriptBundle("~/bundles/jqueryval")
                .Include(
                "~/Scripts/jquery.unobtrusive*",
                "~/Scripts/jquery.validate*"));

            bundles.Add(new ScriptBundle("~/bundles/jquerytable")
                .Include(
                "~/Scripts/jquery.dataTables.min.js"));

            bundles.Add(new ScriptBundle("~/bundles/jqueryui")
                .Include(
                "~/Scripts/jquery-ui-1.11.4.js",
                "~/Scripts/jquery-ui-i18n.min.js"));

            bundles.Add(new ScriptBundle("~/bundles/bootstrap")
               .Include(
                "~/Scripts/bootstrap.min.js"));
            bundles.Add(new ScriptBundle("~/bundles/bootstrapdatetime")
               .Include(
                "~/Scripts/bootstrap-datetimepicker.min.js"));

            bundles.Add(new ScriptBundle("~/bundles/knockout").Include(
                "~/Scripts/knockout-{version}.js",
                "~/Scripts/knockout.mapping-{version}.js"));

            bundles.Add(new ScriptBundle("~/bundles/papaparse")
               .Include(
                "~/Scripts/papaparse.min.js"));

            bundles.Add(new StyleBundle("~/content/css/vendor")
                .Include(
                "~/content/styles/datatables.css",
                "~/content/themes/base/core.css",
                "~/content/themes/base/dialog.css",
                "~/content/themes/base/resizable.css",
                "~/content/themes/base/menu.css",
                "~/content/themes/base/autocomplete.css",
                "~/content/styles/visuals.min.css"));

            //var lessBundle = new Bundle("~/content/css")
            //    .Include("~/Content/styles/main.less");

            //lessBundle.Transforms.Add(new LessTransform(HttpContext.Current.Server.MapPath("~/Content/styles")));
            //lessBundle.Transforms.Add(new CssMinify());

            //bundles.Add(lessBundle);

            bundles.Add(new StyleBundle("~/content/css")
                .Include("~/content/styles/main.css"));
        }
    }
}
