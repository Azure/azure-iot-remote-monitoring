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
        #region Instance Variables

        private readonly IAlertsRepository alertsRepository;

        #endregion

        #region Constructors

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

        #endregion

        #region Public Methods

        /// <summary>
        /// Loads the latest Device Alert History items.
        /// </summary>
        /// <param name="maxItems">
        /// The maximum number of Device Alert History items to return.
        /// </param>
        /// <returns>
        /// The latest Device Alert History items.
        /// </returns>
        public async Task<IEnumerable<AlertHistoryItemModel>> LoadLatestAlertHistoryAsync(
            int maxItems)
        {
            if (maxItems <= 0)
            {
                throw new ArgumentOutOfRangeException(
                    "maxItems",
                    "maxItems is not a positive integer.");
            }

            return await this.alertsRepository.LoadLatestAlertHistoryAsync(maxItems);
        }

        #endregion
    }
}
