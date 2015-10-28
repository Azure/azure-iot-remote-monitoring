using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Azure.Devices.Applications.RemoteMonitoring.DeviceAdmin.Infrastructure.Models;

namespace Microsoft.Azure.Devices.Applications.RemoteMonitoring.DeviceAdmin.Infrastructure.Repository
{
    public interface IAlertsRepository
    {
        Task<IEnumerable<AlertHistoryItemModel>> LoadLatestAlertHistoryAsync(DateTime minTime, int minResults);
    }
}