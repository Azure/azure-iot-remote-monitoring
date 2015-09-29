using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Azure.Devices.Applications.RemoteMonitoring.DeviceAdmin.Infrastructure.Models;
using Microsoft.Azure.Devices.Applications.RemoteMonitoring.DeviceAdmin.Infrastructure.Repository;

namespace Microsoft.Azure.Devices.Applications.RemoteMonitoring.DeviceAdmin.Infrastructure.BusinessLogic
{
    /// <summary>
    /// An IDeviceTelemetryLogic implementation that has business logic class 
    /// for Device telemetry-related functionality.
    /// </summary>
    public class DeviceTelemetryLogic : IDeviceTelemetryLogic
    {
        #region Instance Variables

        private readonly IDeviceTelemetryRepository _deviceTelemetryRepository;

        #endregion

        #region Constructors

        /// <summary>
        /// Initializes a new instance of the DeviceTelemetryLogic class.
        /// </summary>
        /// <param name="deviceTelemetryRepository">
        /// The IDeviceTelemetryRepository implementation that the new 
        /// instance will use.
        /// </param>
        public DeviceTelemetryLogic(
            IDeviceTelemetryRepository deviceTelemetryRepository)
        {
            if (deviceTelemetryRepository == null)
            {
                throw new ArgumentNullException("deviceTelemetryRepository");
            }

            _deviceTelemetryRepository = deviceTelemetryRepository;
        }

        #endregion

        #region Public Methods

        /// <summary>
        /// Loads the most recent Device telemetry.
        /// </summary>
        /// <param name="deviceId">
        /// The ID of the Device for which telemetry should be returned.
        /// </param>
        /// <param name="minTime">
        /// The minimum time of record of the telemetry that should be returned.
        /// </param>
        /// <returns>
        /// Telemetry for the Device specified by deviceId, inclusively since 
        /// minTime.
        /// </returns>
        public async Task<IEnumerable<DeviceTelemetryModel>> LoadLatestDeviceTelemetryAsync(
            string deviceId, 
            DateTime minTime)
        {
            return await _deviceTelemetryRepository.LoadLatestDeviceTelemetryAsync(
                deviceId,
                minTime);
        }

        /// <summary>
        /// Loads the most recent DeviceTelemetrySummaryModel for a specified Device.
        /// </summary>
        /// <param name="deviceId">
        /// The ID of the Device for which a telemetry summary model should be 
        /// returned.
        /// </param>
        /// <param name="minTime">
        /// If provided the the minimum time stamp of the summary data that should 
        /// be loaded. 
        /// </param>
        /// <returns>
        /// The most recent DeviceTElemetrySummaryModel for the Device, 
        /// specified by deviceId.
        /// </returns>
        public async Task<DeviceTelemetrySummaryModel> LoadLatestDeviceTelemetrySummaryAsync(
            string deviceId,
            DateTime? minTime)
        {
            return await _deviceTelemetryRepository.LoadLatestDeviceTelemetrySummaryAsync(
                deviceId,
                minTime);
        }

        #endregion
    }
}
