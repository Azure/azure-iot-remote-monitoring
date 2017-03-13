using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Azure.Devices.Applications.RemoteMonitoring.Common.Configurations;
using Microsoft.Azure.Devices.Applications.RemoteMonitoring.Common.Helpers;
using Microsoft.Azure.Devices.Applications.RemoteMonitoring.DeviceAdmin.Infrastructure.Models;

namespace Microsoft.Azure.Devices.Applications.RemoteMonitoring.DeviceAdmin.Infrastructure.Repository
{
    using StrDict = IDictionary<string, string>;

    /// <summary>
    /// A repository for Device telemetry data.
    /// </summary>
    public class DeviceTelemetryRepository : IDeviceTelemetryRepository
    {
        private readonly string _telemetryDataPrefix;
        private readonly string _telemetrySummaryPrefix;
        private readonly IBlobStorageClient _blobStorageManager;

        /// <summary>
        /// Initializes a new instance of the DeviceTelemetryRepository class.
        /// </summary>
        /// <param name="configProvider">
        /// The IConfigurationProvider implementation with which to initialize 
        /// the new instance.
        /// </param>
        public DeviceTelemetryRepository(IConfigurationProvider configProvider, IBlobStorageClientFactory blobStorageClientFactory)
        {
            if (configProvider == null)
            {
                throw new ArgumentNullException("configProvider");
            }

            string telemetryContainerName = configProvider.GetConfigurationSettingValue("TelemetryStoreContainerName");
            _telemetryDataPrefix = configProvider.GetConfigurationSettingValue("TelemetryDataPrefix");
            string telemetryStoreConnectionString = configProvider.GetConfigurationSettingValue("device.StorageConnectionString");
            _telemetrySummaryPrefix = configProvider.GetConfigurationSettingValue("TelemetrySummaryPrefix");
            _blobStorageManager = blobStorageClientFactory.CreateClient(telemetryStoreConnectionString, telemetryContainerName);
        }

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
            IList<DeviceTelemetryFieldModel> telemetryFields,
            DateTime minTime)
        {
            IEnumerable<DeviceTelemetryModel> result = new DeviceTelemetryModel[0];
            IEnumerable<DeviceTelemetryModel> blobModels;

            var telemetryBlobReader = await _blobStorageManager.GetReader(_telemetryDataPrefix, minTime);
            foreach (var telemetryStream in telemetryBlobReader)
            {
                try
                {
                    blobModels = LoadBlobTelemetryModels(telemetryStream.Data, telemetryFields);
                }
                catch
                {
                    continue;
                }

                if (blobModels == null)
                {
                    break;
                }

                int preFilterCount = blobModels.Count();

                blobModels =
                    blobModels.Where(
                        t =>
                            (t != null) &&
                            t.Timestamp.HasValue &&
                            t.Timestamp.Value >= minTime);

                if (preFilterCount == 0)
                {
                    break;
                }

                result = result.Concat(blobModels);
            }

            if (!string.IsNullOrEmpty(deviceId))
            {
                result = result.Where(t => t.DeviceId == deviceId);
            }

            return result;
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
        /// The most recent DeviceTelemetrySummaryModel for the Device, 
        /// specified by deviceId.
        /// </returns>
        public async Task<DeviceTelemetrySummaryModel> LoadLatestDeviceTelemetrySummaryAsync(
            string deviceId,
            DateTime? minTime)
        {
            DeviceTelemetrySummaryModel summaryModel = null;
            IEnumerable<DeviceTelemetrySummaryModel> blobModels;
            var telemetryBlobReader = await _blobStorageManager.GetReader(_telemetrySummaryPrefix, minTime);
            foreach (var telemetryStream in telemetryBlobReader)
            {
                try
                {
                    blobModels = LoadBlobTelemetrySummaryModels(telemetryStream.Data, telemetryStream.LastModifiedTime);
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

        private static List<DeviceTelemetryModel> LoadBlobTelemetryModels(Stream stream, IList<DeviceTelemetryFieldModel> telemetryFields)
        {
            Debug.Assert(stream != null, "stream is a null reference.");

            List<DeviceTelemetryModel> models = new List<DeviceTelemetryModel>();

            try
            {
                stream.Position = 0;
                using (var reader = new StreamReader(stream))
                {
                    IEnumerable<StrDict> strdicts = ParsingHelper.ParseCsv(reader).ToDictionaries();
                    DeviceTelemetryModel model;
                    string str;
                    foreach (StrDict strdict in strdicts)
                    {
                        model = new DeviceTelemetryModel();

                        if (strdict.TryGetValue("deviceid", out str))
                        {
                            model.DeviceId = str;
                        }

                        model.Timestamp = DateTime.Parse(
                            strdict["eventenqueuedutctime"],
                            CultureInfo.InvariantCulture,
                            DateTimeStyles.AllowWhiteSpaces);

                        IEnumerable<DeviceTelemetryFieldModel> fields;

                        if (telemetryFields != null && telemetryFields.Count > 0)
                        {
                            fields = telemetryFields;
                        }
                        else
                        {
                            List<string> reservedColumns = new List<string>
                        {
                            "DeviceId",
                            "EventEnqueuedUtcTime",
                            "EventProcessedUtcTime",
                            "IoTHub",
                            "PartitionId"
                        };

                            fields = strdict.Keys
                                .Where((key) => !reservedColumns.Contains(key))
                                .Select((name) => new DeviceTelemetryFieldModel
                                {
                                    Name = name,
                                    Type = "double"
                                });
                        }

                        foreach (var field in fields)
                        {
                            if (strdict.TryGetValue(field.Name, out str))
                            {
                                switch (field.Type.ToUpperInvariant())
                                {
                                    case "INT":
                                    case "INT16":
                                    case "INT32":
                                    case "INT64":
                                    case "SBYTE":
                                    case "BYTE":
                                        int intValue;
                                        if (
                                            int.TryParse(
                                                str,
                                                NumberStyles.Integer,
                                                CultureInfo.InvariantCulture,
                                                out intValue) &&
                                            !model.Values.ContainsKey(field.Name))
                                        {
                                            model.Values.Add(field.Name, intValue);
                                        }
                                        break;

                                    case "DOUBLE":
                                    case "DECIMAL":
                                    case "SINGLE":
                                        double dblValue;
                                        if (
                                            double.TryParse(
                                                str,
                                                NumberStyles.Float,
                                                CultureInfo.InvariantCulture,
                                                out dblValue) &&
                                            !model.Values.ContainsKey(field.Name))
                                        {
                                            model.Values.Add(field.Name, dblValue);
                                        }
                                        break;
                                }
                            }
                        }

                        models.Add(model);
                    }
                }
            }
            finally
            {
            }

            return models;
        }

        private static List<DeviceTelemetrySummaryModel> LoadBlobTelemetrySummaryModels(Stream stream, DateTime? lastModifiedTime)
        {
            Debug.Assert(stream != null, "stream is a null reference.");

            var models = new List<DeviceTelemetrySummaryModel>();

            try
            {
                stream.Position = 0;
                using (var reader = new StreamReader(stream))
                {
                    IEnumerable<StrDict> strdicts = ParsingHelper.ParseCsv(reader).ToDictionaries();
                    DeviceTelemetrySummaryModel model;
                    double number;
                    string str;
                    foreach (StrDict strdict in strdicts)
                    {
                        model = new DeviceTelemetrySummaryModel();

                        if (strdict.TryGetValue("deviceid", out str))
                        {
                            model.DeviceId = str;
                        }

                        if (strdict.TryGetValue("averagehumidity", out str) &&
                           double.TryParse(
                                str,
                                NumberStyles.Float,
                                CultureInfo.InvariantCulture,
                                out number))
                        {
                            model.AverageHumidity = number;
                        }

                        if (strdict.TryGetValue("maxhumidity", out str) &&
                           double.TryParse(
                                str,
                                NumberStyles.Float,
                                CultureInfo.InvariantCulture,
                                out number))
                        {
                            model.MaximumHumidity = number;
                        }

                        if (strdict.TryGetValue("minimumhumidity", out str) &&
                           double.TryParse(
                                str,
                                NumberStyles.Float,
                                CultureInfo.InvariantCulture,
                                out number))
                        {
                            model.MinimumHumidity = number;
                        }

                        if (strdict.TryGetValue("timeframeminutes", out str) &&
                           double.TryParse(
                                str,
                                NumberStyles.Float,
                                CultureInfo.InvariantCulture,
                                out number))
                        {
                            model.TimeFrameMinutes = number;
                        }

                        model.Timestamp = lastModifiedTime;

                        models.Add(model);
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
