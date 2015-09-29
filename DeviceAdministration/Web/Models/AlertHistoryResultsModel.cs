using System.Collections.Generic;
using Microsoft.Azure.Devices.Applications.RemoteMonitoring.DeviceAdmin.Infrastructure.Models;

namespace Microsoft.Azure.Devices.Applications.RemoteMonitoring.DeviceAdmin.Web.Models
{
    public class AlertHistoryResultsModel
    {
        public int TotalAlertCount { get; set; }

        public int TotalFilteredCount { get; set; }

        public List<AlertHistoryItemModel> Data{ get; set; }
    }
}