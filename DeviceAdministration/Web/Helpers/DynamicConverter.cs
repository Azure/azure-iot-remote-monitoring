using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using GlobalResources;
using Microsoft.Azure.Devices.Applications.RemoteMonitoring.DeviceAdmin.Web.Models;

namespace Microsoft.Azure.Devices.Applications.RemoteMonitoring.DeviceAdmin.Web.Helpers
{
    public class DynamicConverter
    {
        public static T ValidateAndConvert<T>(dynamic dynamicObj)
        {
            string dynamicObjStr = Newtonsoft.Json.JsonConvert.SerializeObject(dynamicObj);
            T strongObj = Newtonsoft.Json.JsonConvert.DeserializeObject<T>(dynamicObjStr);
            var strongObjStr = Newtonsoft.Json.JsonConvert.SerializeObject(strongObj);
            if (strongObjStr != dynamicObjStr)
                throw new Exception(string.Format("Conversion failed for type: {0}", typeof(T)));
            return strongObj;
        }
    }
}