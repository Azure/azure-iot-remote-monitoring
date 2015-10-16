using System.Web.Optimization;

namespace Microsoft.Azure.Devices.Applications.RemoteMonitoring.DeviceAdmin.Web
{
    public class BundleConfig
    {
        // For more information on bundling, visit http://go.microsoft.com/fwlink/?LinkId=301862
        public static void RegisterBundles(BundleCollection bundles)
        {

            // jquery is included in powerbi-visuals.all.min.js
            bundles.Add(new ScriptBundle("~/bundles/powerbi-visuals").Include(
                        "~/Scripts/powerbi-visuals.all.min.js"));

            bundles.Add(new ScriptBundle("~/bundles/jqueryval").Include(
                        "~/Scripts/jquery.unobtrusive*",
                        "~/Scripts/jquery.validate*"));

            bundles.Add(new ScriptBundle("~/bundles/jquerytable").Include(
                        "~/Scripts/jquery.dataTables.min.js"));

            bundles.Add(new ScriptBundle("~/bundles/jqueryui").Include("~/Scripts/jquery-ui-1.11.4.js", "~/Scripts/jquery-ui-i18n.min.js"));

            bundles.Add(new StyleBundle("~/Content/css")
                .Include("~/Content/datatables.css", "~/Content/themes/base/core.css", "~/Content/themes/base/dialog.css", "~/Content/visuals.min.css", "~/Content/screen.css"));
        }
    }
}
