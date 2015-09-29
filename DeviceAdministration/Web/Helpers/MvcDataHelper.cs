using System.Collections.Generic;
using System.Web.Mvc;

namespace Microsoft.Azure.Devices.Applications.RemoteMonitoring.DeviceAdmin.Web.Helpers
{
    public static class MvcDataHelper
    {
        public static List<SelectListItem> ConvertStringListToSelectList(List<string> stringList)
        {
            List<SelectListItem> result = new List<SelectListItem>();
            foreach(string item in stringList)
            {
                SelectListItem selectItem = new SelectListItem();
                selectItem.Value = item;
                selectItem.Text = item;
                result.Add(selectItem);
            }

            return result;
        }
    }
}