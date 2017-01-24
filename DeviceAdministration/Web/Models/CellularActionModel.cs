using System;

namespace Microsoft.Azure.Devices.Applications.RemoteMonitoring.DeviceAdmin.Web.Models
{
    public class CellularActionModel
    {
        public CellularActionType Type { get; set; }
        public string PreviousValue { get; set; }
        public string Value { get; set; }
        public Exception Exception { get; set; }
    }

    public enum CellularActionType
    {
        UpdateStatus = 1,
        UpdateSubscriptionPackage = 2,
        ReconnectDevice = 3,
        SendSms = 4,
        UpdateLocale = 5
    }
}