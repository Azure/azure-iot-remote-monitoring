using System.Collections.Generic;
using Microsoft.Azure.Devices.Applications.RemoteMonitoring.DeviceAdmin.Infrastructure.Models;

namespace Microsoft.Azure.Devices.Applications.RemoteMonitoring.DeviceAdmin.Web.Models
{
    public class AlertHistoryResultsModel
    {
        public int TotalAlertCount { get; set; }

        public int TotalFilteredCount { get; set; }

        public List<AlertHistoryItemModel> Data { get; set; }

        public List<AlertHistoryDeviceModel> Devices
        {
            get;
            set;
        }

        public double MaxLatitude
        {
            get;
            set;
        }

        public double MaxLongitude
        {
            get;
            set;
        }

        public double MinLatitude
        {
            get;
            set;
        }

        public double MinLongitude
        {
            get;
            set;
        }
    }
}