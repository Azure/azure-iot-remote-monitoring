using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Azure.Devices.Applications.RemoteMonitoring.Common.Configurations;
using Microsoft.Azure.Devices.Applications.RemoteMonitoring.Common.Helpers;
using Microsoft.Azure.Devices.Applications.RemoteMonitoring.DeviceAdmin.Infrastructure.Models;
using Microsoft.WindowsAzure.Storage.Blob;

namespace Microsoft.Azure.Devices.Applications.RemoteMonitoring.DeviceAdmin.Infrastructure.Repository
{
    using StrDict = IDictionary<string, string>;

    /// <summary>
    /// A repository for Device telemetry data.
    /// </summary>
    public class DeviceTelemetryRepository : IDeviceTelemetryRepository
    {
        #region Instance Variables

        private readonly string _telemetryContainerName;
        private readonly string _telemetryDataPrefix;
        private readonly string _telemetryStoreConnectionString;
        private readonly string _telemetrySummaryPrefix;

        #endregion

        #region Constructors

        /// <summary>
        /// Initializes a new instance of the DeviceTelemetryRepository class.
        /// </summary>
        /// <param name="configProvider">
        /// The IConfigurationProvider implementation with which to initialize 
        /// the new instance.
        /// </param>
        public DeviceTelemetryRepository(
            IConfigurationProvider configProvider)
        {
            if (configProvider == null)
            {
                throw new ArgumentNullException("configProvider");
            }

            this._telemetryContainerName =
                configProvider.GetConfigurationSettingValue(
                "TelemetryStoreContainerName");

            this._telemetryDataPrefix =
                configProvider.GetConfigurationSettingValue(
                    "TelemetryDataPrefix");

            this._telemetryStoreConnectionString = 
                configProvider.GetConfigurationSettingValue(
                    "device.StorageConnectionString");

            this._telemetrySummaryPrefix =
                configProvider.GetConfigurationSettingValue(
                    "TelemetrySummaryPrefix");
        }

        #endregion

        #region Public Methods

        #region Instance Method: LoadLatestDeviceTelemetryAsync

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
            IEnumerable<DeviceTelemetryModel> blobModels;
            IEnumerable<IListBlobItem> blobs;
            CloudBlockBlob blockBlob;
            CloudBlobContainer container;
            int preFilterCount;
            IEnumerable<DeviceTelemetryModel> result;

            minTime = minTime.ToUniversalTime();
            result = new DeviceTelemetryModel[0];

            container =
                await BlobStorageHelper.BuildBlobContainerAsync(
                    this._telemetryStoreConnectionString,
                    _telemetryContainerName);

            blobs =
                await BlobStorageHelper.LoadBlobItemsAsync(
                    async (token) =>
                    {
                        return await container.ListBlobsSegmentedAsync(
                            _telemetryDataPrefix,
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

                try
                {
                    blobModels = await LoadBlobTelemetryModelsAsync(blockBlob);
                }
                catch
                {
                    continue;
                }

                if (blobModels == null)
                {
                    break;
                }

                preFilterCount = blobModels.Count();

                blobModels =
                    blobModels.Where(
                        t =>
                            (t != null) &&
                            t.Timestamp.HasValue &&
                            t.Timestamp.Value.ToUniversalTime() >= minTime);

                if (preFilterCount == 0)
                {
                    break;
                }

                result = result.Concat(blobModels);

                if (preFilterCount != blobModels.Count())
                {
                    break;
                }
            }

            if (!string.IsNullOrEmpty(deviceId))
            {
                result = result.Where(t => t.DeviceId == deviceId);
            }

            return result;
        }

        #endregion

        #region Instance Method: LoadLatestDeviceTelemetrySummaryAsync

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
        /// The most recent DeviceTelemetrySummaryModel for the Device, 
        /// specified by deviceId.
        /// </returns>
        public async Task<DeviceTelemetrySummaryModel> LoadLatestDeviceTelemetrySummaryAsync(
            string deviceId,
            DateTime? minTime)
        {
            IEnumerable<DeviceTelemetrySummaryModel> blobModels;
            IEnumerable<IListBlobItem> blobs;
            CloudBlockBlob blockBlob;
            CloudBlobContainer container;
            DeviceTelemetrySummaryModel summaryModel;

            if (minTime.HasValue)
            {
                minTime = minTime.Value.ToUniversalTime();
            }

            summaryModel = null;

            container =
                await BlobStorageHelper.BuildBlobContainerAsync(
                    this._telemetryStoreConnectionString,
                    _telemetryContainerName);

            blobs =
                await BlobStorageHelper.LoadBlobItemsAsync(
                    async (token) =>
                    {
                        return await container.ListBlobsSegmentedAsync(
                            _telemetrySummaryPrefix,
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

                if (minTime.HasValue &&
                    (blockBlob.Properties != null) &&
                    blockBlob.Properties.LastModified.HasValue &&
                    (blockBlob.Properties.LastModified.Value.UtcDateTime < minTime))
                {
                    break;
                }

                try
                {
                    blobModels = await LoadBlobTelemetrySummaryModelsAsync(blockBlob);
                }
                catch
                {
                    continue;
                }

                if (blobModels == null)
                {
                    break;
                }

                blobModels = blobModels.Where(t => t != null);

                if (!string.IsNullOrEmpty(deviceId))
                {
                    blobModels = blobModels.Where(t => t.DeviceId == deviceId);
                }

                summaryModel = blobModels.LastOrDefault();
                if (summaryModel != null)
                {
                    break;
                }
            }

            return summaryModel;
        }

        #endregion

        #endregion

        #region Private Methods

        #region Static Method: LoadBlobTelemetryModelsAsync

        private async static Task<List<DeviceTelemetryModel>> LoadBlobTelemetryModelsAsync(
            CloudBlockBlob blob)
        {
            DateTime date;
            IDisposable disp;
            DeviceTelemetryModel model;
            List<DeviceTelemetryModel> models;
            double number;
            TextReader reader;
            string str;
            IEnumerable<StrDict> strdicts;
            MemoryStream stream;

            Debug.Assert(blob != null, "blob is a null reference.");

            models = new List<DeviceTelemetryModel>();

            reader = null;
            stream = null;
            try
            {
                stream = new MemoryStream();
                await blob.DownloadToStreamAsync(stream);
                stream.Position = 0;
                reader = new StreamReader(stream);

                strdicts = ParsingHelper.ParseCsv(reader).ToDictionaries();
                foreach (StrDict strdict in strdicts)
                {
                    model = new DeviceTelemetryModel();

                    if (strdict.TryGetValue("DeviceId", out str))
                    {
                        model.DeviceId = str;
                    }

                    if (strdict.TryGetValue("ExternalTemperature", out str) &&
                        double.TryParse(str, out number))
                    {
                        model.ExternalTemperature = number;
                    }

                    if (strdict.TryGetValue("Humidity", out str) &&
                        double.TryParse(str, out number))
                    {
                        model.Humidity = number;
                    }

                    if (strdict.TryGetValue("Temperature", out str) &&
                        double.TryParse(str, out number))
                    {
                        model.Temperature = number;
                    }

                    if (strdict.TryGetValue("EventEnqueuedUtcTime", out str) &&
                        DateTime.TryParse(str, out date))
                    {
                        model.Timestamp = date;
                    }

                    models.Add(model);
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

        #region Static Method: LoadBlobTelemetrySummaryModelsAsync

        private async static Task<List<DeviceTelemetrySummaryModel>> LoadBlobTelemetrySummaryModelsAsync(
            CloudBlockBlob blob)
        {
            IDisposable disp;
            IEnumerable<StrDict> strdicts;
            DeviceTelemetrySummaryModel model;
            List<DeviceTelemetrySummaryModel> models;
            double number;
            TextReader reader;
            string str;
            MemoryStream stream;

            Debug.Assert(blob != null, "blob is a null reference.");

            models = new List<DeviceTelemetrySummaryModel>();

            reader = null;
            stream = null;
            try
            {
                stream = new MemoryStream();
                await blob.DownloadToStreamAsync(stream);
                stream.Position = 0;
                reader = new StreamReader(stream);

                strdicts = ParsingHelper.ParseCsv(reader).ToDictionaries();
                foreach (StrDict strdict in strdicts)
                {
                    model = new DeviceTelemetrySummaryModel();

                    if (strdict.TryGetValue("deviceid", out str))
                    {
                        model.DeviceId = str;
                    }

                    if (strdict.TryGetValue("averagehumidity", out str) &&
                       double.TryParse(str, out number))
                    {
                        model.AverageHumidity = number;
                    }

                    if (strdict.TryGetValue("maxhumidity", out str) &&
                       double.TryParse(str, out number))
                    {
                        model.MaximumHumidity = number;
                    }

                    if (strdict.TryGetValue("minimumhumidity", out str) &&
                       double.TryParse(str, out number))
                    {
                        model.MinimumHumidity = number;
                    }

                    if (strdict.TryGetValue("timeframeminutes", out str) &&
                       double.TryParse(str, out number))
                    {
                        model.TimeFrameMinutes = number;
                    }

                    if ((blob.Properties != null) &&
                        blob.Properties.LastModified.HasValue)
                    {
                        model.Timestamp =
                            blob.Properties.LastModified.Value.UtcDateTime;
                    }

                    models.Add(model);
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
