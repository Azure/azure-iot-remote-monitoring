using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Dynamic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Azure.Devices.Applications.RemoteMonitoring.Common.Configurations;
using Microsoft.Azure.Devices.Applications.RemoteMonitoring.Common.Helpers;
using Microsoft.Azure.Devices.Applications.RemoteMonitoring.DeviceAdmin.Infrastructure.Models;

namespace Microsoft.Azure.Devices.Applications.RemoteMonitoring.DeviceAdmin.Infrastructure.Repository
{
    /// <summary>
    /// An IAlertsRepository implementation with functionality for accessing 
    /// Alerts-related data.
    /// </summary>
    public class AlertsRepository : IAlertsRepository
    {
        // column names in ASA job output
        private const string DEVICE_ID_COLUMN_NAME = "deviceid";
        private const string READING_TYPE_COLUMN_NAME = "readingtype";
        private const string READING_VALUE_COLUMN_NAME = "reading";
        private const string THRESHOLD_VALUE_COLUMN_NAME = "threshold";
        private const string RULE_OUTPUT_COLUMN_NAME = "ruleoutput";
        private const string TIME_COLUMN_NAME = "time";

        private readonly IBlobStorageClient _blobStorageManager;
        private readonly string deviceAlertsDataPrefix;

        /// <summary>
        /// Initializes a new instance of the AlertsRepository class.
        /// </summary>
        /// <param name="configProvider">
        /// The IConfigurationProvider implementation with which the new 
        /// instance will be initialized.
        /// </param>
        public AlertsRepository(IConfigurationProvider configProvider, IBlobStorageClientFactory blobStorageClientFactory)
        {
            if (configProvider == null)
            {
                throw new ArgumentNullException("configProvider");
            }

            string alertsContainerConnectionString = configProvider.GetConfigurationSettingValue("device.StorageConnectionString");
            string alertsStoreContainerName = configProvider.GetConfigurationSettingValue("AlertsStoreContainerName");
            this._blobStorageManager = blobStorageClientFactory.CreateClient(alertsContainerConnectionString, alertsStoreContainerName);
            this.deviceAlertsDataPrefix = configProvider.GetConfigurationSettingValue("DeviceAlertsDataPrefix");
        }

        /// <summary>
        /// Loads the latest Device Alert History items.
        /// </summary>
        /// <param name="minTime">
        /// The cutoff time for Device Alert History items that should be returned.
        /// </param>
        /// <param name="minResults">
        /// The minimum number of items that should be returned, if possible, 
        /// after <paramref name="minTime"/> or otherwise.
        /// </param>
        /// <returns>
        /// The latest Device Alert History items.
        /// </returns>
        public async Task<IEnumerable<AlertHistoryItemModel>> LoadLatestAlertHistoryAsync(
            DateTime minTime,
            int minResults)
        {
            if (minResults <= 0)
            {
                throw new ArgumentOutOfRangeException("minResults", minResults, "minResults must be a positive integer.");
            }

            var filteredResult = new List<AlertHistoryItemModel>();
            var unfilteredResult = new List<AlertHistoryItemModel>();
            var alertBlobReader = await _blobStorageManager.GetReader(deviceAlertsDataPrefix);
            foreach (var alertStream in alertBlobReader)
            {
                var segment = ProduceAlertHistoryItemsAsync(alertStream.Data);
                IEnumerable<AlertHistoryItemModel> filteredSegment = segment.Where(t => t?.Timestamp != null && (t.Timestamp.Value > minTime));

                var unfilteredCount = segment.Count();
                var filteredCount = filteredSegment.Count();

                unfilteredResult.AddRange(segment.OrderByDescending(t => t.Timestamp));
                filteredResult.AddRange(filteredSegment.OrderByDescending(t => t.Timestamp));

                // Anything filtered and min entries?
                if ((filteredCount != unfilteredCount) && (filteredResult.Count >= minResults))
                {
                    // already into items older than minTime
                    break;
                }

                // No more filtered entries and enough otherwise?
                if ((filteredCount == 0) && (unfilteredResult.Count >= minResults))
                {
                    // we are past minTime and we have enough unfiltered results
                    break;
                }
            }

            if (filteredResult.Count >= minResults)
            {
                return filteredResult;
            }
            else
            {
                return unfilteredResult.Take(minResults);
            }
        }

        private static AlertHistoryItemModel ProduceAlertHistoryItem(ExpandoObject expandoObject)
        {
            Debug.Assert(expandoObject != null, "expandoObject is a null reference.");

            var deviceId = ReflectionHelper.GetNamedPropertyValue(
                        expandoObject,
                        DEVICE_ID_COLUMN_NAME,
                        true,
                        false) as string;

            var readingValue = ReflectionHelper.GetNamedPropertyValue(
                        expandoObject,
                        READING_VALUE_COLUMN_NAME,
                        true,
                        false) as string;

            var ruleOutput = ReflectionHelper.GetNamedPropertyValue(
                        expandoObject,
                        RULE_OUTPUT_COLUMN_NAME,
                        true,
                        false) as string;

            var time = ReflectionHelper.GetNamedPropertyValue(
                        expandoObject,
                        TIME_COLUMN_NAME,
                        true,
                        false) as string;

            return BuildModelForItem(ruleOutput, deviceId, readingValue, time);
        }

        private static AlertHistoryItemModel BuildModelForItem(string ruleOutput, string deviceId, string value, string time)
        {
            double valDouble;
            DateTime timeAsDateTime;

            if (!string.IsNullOrWhiteSpace(value) &&
                !string.IsNullOrWhiteSpace(deviceId) &&
                double.TryParse(value, NumberStyles.Float, CultureInfo.InvariantCulture, out valDouble) &&
                DateTime.TryParse(time, CultureInfo.InvariantCulture, DateTimeStyles.AllowWhiteSpaces, out timeAsDateTime))
            {
                return new AlertHistoryItemModel()
                {
                    RuleOutput = ruleOutput,
                    Value = value,
                    DeviceId = deviceId,
                    Timestamp = timeAsDateTime
                };
            }

            return null;
        }

        private static List<AlertHistoryItemModel> ProduceAlertHistoryItemsAsync(Stream stream)
        {
            Debug.Assert(stream != null, "stream is a null reference.");

            var models = new List<AlertHistoryItemModel>();

            try
            {
                stream.Position = 0;
                using (var reader = new StreamReader(stream))
                {
                    IEnumerable<ExpandoObject> expandos = ParsingHelper.ParseCsv(reader).ToExpandoObjects();
                    foreach (ExpandoObject expando in expandos)
                    {
                        AlertHistoryItemModel model = ProduceAlertHistoryItem(expando);

                        if (model != null)
                        {
                            models.Add(model);
                        }
                    }
                }
            }
            finally
            {
            }

            return models;
        }
    }
}