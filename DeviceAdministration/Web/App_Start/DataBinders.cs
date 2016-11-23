using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace Microsoft.Azure.Devices.Applications.RemoteMonitoring.DeviceAdmin.Web.App_Start
{
    public class DateTimeBinder : IModelBinder
    {
        public String DateFormat { get; set; }
        public String TimeFormat { get; set; }

        public object BindModel(ControllerContext controllerContext, ModelBindingContext bindingContext)
        {
            var value = bindingContext.ValueProvider.GetValue(bindingContext.ModelName);
            bindingContext.ModelState.SetModelValue(bindingContext.ModelName, value);

            DateTime result;
            if (DateTime.TryParseExact(value.AttemptedValue, DateFormat, CultureInfo.InvariantCulture, DateTimeStyles.None,out result))
            {
                return result;
            }
            else if(DateTime.TryParseExact(value.AttemptedValue, TimeFormat, CultureInfo.InvariantCulture, DateTimeStyles.None, out result))
            {
                return result;
            }
            else if(DateTime.TryParse(value.AttemptedValue, out result))
            {
                return result;
            }
            return null;
        }
    }

    public class NullableDateTimeBinder : IModelBinder
    {
        public String DateFormat { get; set; }
        public String TimeFormat { get; set; }

        public object BindModel(ControllerContext controllerContext, ModelBindingContext bindingContext)
        {
            var value = bindingContext.ValueProvider.GetValue(bindingContext.ModelName);
            bindingContext.ModelState.SetModelValue(bindingContext.ModelName, value);

            DateTime result;
            if (value !=null && DateTime.TryParseExact(value.AttemptedValue, DateFormat, CultureInfo.InvariantCulture, DateTimeStyles.None, out result))
            {
                return result;
            }
            else if (value != null && DateTime.TryParseExact(value.AttemptedValue, TimeFormat, CultureInfo.InvariantCulture, DateTimeStyles.None, out result))
            {
                return result;
            }
            else if (DateTime.TryParse(value.AttemptedValue, out result))
            {
                return result;
            }
            return null;
        }
    }

}