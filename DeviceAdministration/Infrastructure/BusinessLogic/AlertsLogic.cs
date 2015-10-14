using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Azure.Devices.Applications.RemoteMonitoring.DeviceAdmin.Infrastructure.Models;
using Microsoft.Azure.Devices.Applications.RemoteMonitoring.DeviceAdmin.Infrastructure.Repository;

namespace Microsoft.Azure.Devices.Applications.RemoteMonitoring.DeviceAdmin.Infrastructure.BusinessLogic
{
    /// <summary>
    /// An IAlertsLogic implementation that holds business logic, related to 
    /// Alerts.
    /// </summary>
    public class AlertsLogic : IAlertsLogic
    {
        private readonly IAlertsRepository alertsRepository;

        /// <summary>
        /// Initializes a new instance of the AlertsLogic class.
        /// </summary>
        /// <param name="alertsRepository">
        /// The IAlertsRepository implementation that the new instance will use.
        /// </param>
        public AlertsLogic(IAlertsRepository alertsRepository)
        {
            if (alertsRepository == null)
            {
                throw new ArgumentNullException("alertsRepository");
            }

            this.alertsRepository = alertsRepository;
        }

        /// <summary>
        /// Loads the latest Device Alert History items.
        /// </summary>
        /// <param name="minTime">
        /// The cutoff time for Device Alert History items that should be returned.
        /// </param>
        /// <returns>
        /// The latest Device Alert History items.
        /// </returns>
        public async Task<IEnumerable<AlertHistoryItemModel>> LoadLatestAlertHistoryAsync(DateTime minTime)
        {

            return await this.alertsRepository.LoadLatestAlertHistoryAsync(minTime);
        }
    }
}
