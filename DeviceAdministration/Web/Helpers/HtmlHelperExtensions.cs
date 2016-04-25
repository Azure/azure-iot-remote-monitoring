namespace Microsoft.Azure.Devices.Applications.RemoteMonitoring.DeviceAdmin.Web.Helpers
{
    using System.Web;
    using System.Web.Mvc;

    public static class HtmlHelperExtensions
    {
        public static IHtmlString JavaScriptString(this HtmlHelper htmlHelper, string message)
        {
            return htmlHelper.Raw(HttpUtility.JavaScriptStringEncode(message));
        }
    }
}