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
using Microsoft.WindowsAzure.Storage.Blob;

namespace Microsoft.Azure.Devices.Applications.RemoteMonitoring.DeviceAdmin.Infrastructure.Repository
{
    /// <summary>
    /// An IAlertsRepository implementation with functionality for accessing 
    /// Alerts-related data.
    /// </summary>
    public class AlertsRepository : IAlertsRepository
    {
        #region Instance Variables

        private readonly string alertsContainerConnectionString;
        private readonly string alertsStoreContainerName;
        private readonly string deviceAlertsDataPrefix;

        #endregion

        #region Constructors

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

            this.alertsContainerConnectionString =
                configProvider.GetConfigurationSettingValue(
                    "device.StorageConnectionString");

            this.alertsStoreContainerName =
                configProvider.GetConfigurationSettingValue(
                    "AlertsStoreContainerName");

            this.deviceAlertsDataPrefix =
                configProvider.GetConfigurationSettingValue(
                    "DeviceAlertsDataPrefix");
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
            IEnumerable<IListBlobItem> blobs;
            CloudBlockBlob blockBlob;
            CloudBlobContainer container;
            List<AlertHistoryItemModel> result;
            IEnumerable<AlertHistoryItemModel> segment;

            if (maxItems <= 0)
            {
                throw new ArgumentOutOfRangeException(
                    "maxItems",
                    "maxItems is not a positive integer.");
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

        #endregion

        #region Private Methods

        #region Static Method: AttemptNumericFormatting

        private static string AttemptNumericFormatting(string str)
        {
            double dbl;

            if (double.TryParse(
                    str,
                    NumberStyles.Float, 
                    CultureInfo.InvariantCulture, 
                    out dbl))
            {
                str = dbl.ToString("F3", CultureInfo.CurrentCulture);
            }

            return str;
        }

        #endregion

        #region Static Method: ProduceAlertHistoryItem

        private static AlertHistoryItemModel ProduceAlertHistoryItem(
            ExpandoObject expandoObject,
            string sourceField)
        {
            DateTime date;
            AlertHistoryItemModel model;
            string str;

            Debug.Assert(
                expandoObject != null,
                "expandoObject is a null reference.");

            Debug.Assert(
                !string.IsNullOrEmpty(sourceField),
                "sourceField is a null reference or empty string.");

            model = null;

            str =
                ReflectionHelper.GetNamedPropertyValue(
                    expandoObject,
                    sourceField,
                    true,
                    false) as string;

            if (string.Equals(
                    str,
                    "AlarmTemp",
                    StringComparison.OrdinalIgnoreCase))
            {
                model = new AlertHistoryItemModel()
                {
                    RuleOutput = str
                };

                model.DeviceId =
                    ReflectionHelper.GetNamedPropertyValue(
                        expandoObject,
                        "deviceid",
                        true,
                        false) as string;

                model.Value =
                    ReflectionHelper.GetNamedPropertyValue(
                        expandoObject,
                        "tempreading",
                        true,
                        false) as string;
            }
            else if (string.Equals(
                    str,
                    "AlarmHumidity",
                    StringComparison.OrdinalIgnoreCase))
            {
                model = new AlertHistoryItemModel()
                {
                    RuleOutput = str
                };

                model.DeviceId =
                    ReflectionHelper.GetNamedPropertyValue(
                        expandoObject,
                        "deviceid",
                        true,
                        false) as string;

                model.Value =
                    ReflectionHelper.GetNamedPropertyValue(
                        expandoObject,
                        "humidityreading",
                        true,
                        false) as string;
            }

            if (model != null)
            {
                str =
                    ReflectionHelper.GetNamedPropertyValue(
                        expandoObject,
                        "time",
                        true,
                        false) as string;

                if (DateTime.TryParse(
                        str, 
                        CultureInfo.InvariantCulture,
                        DateTimeStyles.AllowWhiteSpaces,
                        out date))
                {
                    model.Timestamp = date;
                }

                model.Value = AttemptNumericFormatting(model.Value);
            }

            return model;
        }

        #endregion

        #region Static Method: ProduceAlertHistoryItemsAsync

        private async static Task<List<AlertHistoryItemModel>> ProduceAlertHistoryItemsAsync(
            CloudBlockBlob blob)
        {
            IDisposable disp;
            IEnumerable<ExpandoObject> expandos;
            AlertHistoryItemModel model;
            List<AlertHistoryItemModel> models;
            TextReader reader;
            MemoryStream stream;

            Debug.Assert(blob != null, "blob is a null reference.");

            models = new List<AlertHistoryItemModel>();

            reader = null;
            stream = null;
            try
            {
                stream = new MemoryStream();
                await blob.DownloadToStreamAsync(stream);
                stream.Position = 0;
                reader = new StreamReader(stream);

                expandos = ParsingHelper.ParseCsv(reader).ToExpandoObjects();
                foreach (ExpandoObject expando in expandos)
                {
                    model =
                        ProduceAlertHistoryItem(
                            expando,
                            "temperatureruleoutput");

                    if (model != null)
                    {
                        models.Add(model);
                    }

                    model =
                        ProduceAlertHistoryItem(expando, "humidityruleoutput");

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

        #endregion

        #endregion
    }
}