using Microsoft.Azure.Devices.Applications.RemoteMonitoring.Common.Configurations;
using Microsoft.Azure.Devices.Applications.RemoteMonitoring.Common.Helpers;
using Microsoft.Azure.Devices.Applications.RemoteMonitoring.DeviceAdmin.Infrastructure.Models;
using Microsoft.WindowsAzure.Storage.Blob;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Dynamic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

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

        private readonly string alertsContainerConnectionString;
        private readonly string alertsStoreContainerName;
        private readonly string deviceAlertsDataPrefix;

        /// <summary>
        /// Initializes a new instance of the AlertsRepository class.
        /// </summary>
        /// <param name="configProvider">
        /// The IConfigurationProvider implementation with which the new 
        /// instance will be initialized.
        /// </param>
        public AlertsRepository(IConfigurationProvider configProvider)
        {
            if (configProvider == null)
            {
                throw new ArgumentNullException("configProvider");
            }

            this.alertsContainerConnectionString = configProvider.GetConfigurationSettingValue("device.StorageConnectionString");
            this.alertsStoreContainerName = configProvider.GetConfigurationSettingValue("AlertsStoreContainerName");
            this.deviceAlertsDataPrefix =configProvider.GetConfigurationSettingValue("DeviceAlertsDataPrefix");
        }

        /// <summary>
        /// Loads the latest Device Alert History items.
        /// </summary>
        /// <param name="maxItems">
        /// The maximum number of Device Alert History items to return.
        /// </param>
        /// <returns>
        /// The latest Device Alert History items.
        /// </returns>
        public async Task<IEnumerable<AlertHistoryItemModel>> LoadLatestAlertHistoryAsync(int maxItems)
        {
            IEnumerable<IListBlobItem> blobs;
            CloudBlockBlob blockBlob;
            CloudBlobContainer container;
            List<AlertHistoryItemModel> result;
            IEnumerable<AlertHistoryItemModel> segment;

            if (maxItems <= 0)
            {
                throw new ArgumentOutOfRangeException("maxItems", "maxItems is not a positive integer.");
            }

            result = new List<AlertHistoryItemModel>();

            container =
                await BlobStorageHelper.BuildBlobContainerAsync(
                    this.alertsContainerConnectionString,
                    this.alertsStoreContainerName);

            blobs =
                await BlobStorageHelper.LoadBlobItemsAsync(
                    async (token) =>
                    {
                        return await container.ListBlobsSegmentedAsync(
                            this.deviceAlertsDataPrefix,
                            true,
                            BlobListingDetails.None,
                            null,
                            token,
                            null,
                            null);
                    });

            blobs =
                blobs.OrderByDescending(
                    t => BlobStorageHelper.ExtractBlobItemDate(t));

            foreach (IListBlobItem blob in blobs)
            {
                if ((blockBlob = blob as CloudBlockBlob) == null)
                {
                    continue;
                }

                segment = await ProduceAlertHistoryItemsAsync(blockBlob);
                segment = segment.OrderByDescending(t => t.Timestamp);

                result.AddRange(segment);

                if (result.Count >= maxItems)
                {
                    return result.Take(maxItems);
                }
            }

            return result;
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

            var thresholdValue = ReflectionHelper.GetNamedPropertyValue(
                    expandoObject,
                    THRESHOLD_VALUE_COLUMN_NAME,
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

            return BuildModelForItem(ruleOutput, deviceId, readingValue, thresholdValue, time);
        }

        private static AlertHistoryItemModel BuildModelForItem(string ruleOutput, string deviceId, string value, string threshold, string time)
        {
            double valDouble;
            double threshDouble;
            DateTime timeAsDateTime;

            if (!string.IsNullOrWhiteSpace(value) &&
                !string.IsNullOrWhiteSpace(threshold) &&
                !string.IsNullOrWhiteSpace(deviceId) &&
                double.TryParse(value, NumberStyles.Float, CultureInfo.InvariantCulture, out valDouble) &&
                double.TryParse(threshold, NumberStyles.Float, CultureInfo.InvariantCulture, out threshDouble) &&
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

        private async static Task<List<AlertHistoryItemModel>> ProduceAlertHistoryItemsAsync(
            CloudBlockBlob blob)
        {
            IDisposable disp;
            IEnumerable<ExpandoObject> expandos;
            AlertHistoryItemModel model;

            Debug.Assert(blob != null, "blob is a null reference.");

            var models = new List<AlertHistoryItemModel>();

            TextReader reader = null;
            MemoryStream stream = null;
            try
            {
                stream = new MemoryStream();
                await blob.DownloadToStreamAsync(stream);
                stream.Position = 0;
                reader = new StreamReader(stream);

                expandos = ParsingHelper.ParseCsv(reader).ToExpandoObjects();
                foreach (ExpandoObject expando in expandos)
                {
                    model = ProduceAlertHistoryItem(expando);

                    if (model != null)
                    {
                        models.Add(model);
                    }
                }
            }
            finally
            {
                if ((disp = stream) != null)
                {
                    disp.Dispose();
                }

                if ((disp = reader) != null)
                {
                    disp.Dispose();
                }
            }

            return models;
        }
    }
}