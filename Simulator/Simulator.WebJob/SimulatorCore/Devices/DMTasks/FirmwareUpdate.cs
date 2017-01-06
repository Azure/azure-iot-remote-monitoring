using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.Azure.Devices.Applications.RemoteMonitoring.Common.Extensions;
using Microsoft.Azure.Devices.Applications.RemoteMonitoring.Simulator.WebJob.SimulatorCore.Transport;
using Microsoft.Azure.Devices.Client;
using Microsoft.Azure.Devices.Shared;
using Newtonsoft.Json;

namespace Microsoft.Azure.Devices.Applications.RemoteMonitoring.Simulator.WebJob.SimulatorCore.Devices.DMTasks
{
    class FirmwareUpdate : DMTaskBase
    {
        static private string StatusPath = "iothubDM.firmwareUpdate.status";
        static private string LogPath = "iothubDM.firmwareUpdate.log";

        private Stopwatch _watch;
        private LogBuilder _logBuilder = new LogBuilder();

        public string Uri { get; private set; }
        public string FirmwareVersion { get; private set; }

        public FirmwareUpdate(MethodRequest request)
        {
            var payload = JsonConvert.DeserializeObject<dynamic>(request.DataAsJson);

            var uri = (string)payload.FwPackageUri;
            if (string.IsNullOrWhiteSpace(uri))
            {
                throw new ArgumentException("Missing FwPackageUri");
            }

            Uri = uri;

            // State switch graph: pending -> downloading -> applying -> rebooting -> idle
            _steps = new List<DMTaskStep>
            {
                new DMTaskStep { CurrentState = DMTaskState.FU_PENDING, ExecuteTime = TimeSpan.Zero, NextState = DMTaskState.FU_DOWNLOADING },
                new DMTaskStep { CurrentState = DMTaskState.FU_DOWNLOADING, ExecuteTime = TimeSpan.Zero, NextState = DMTaskState.FU_APPLYING },
                new DMTaskStep { CurrentState = DMTaskState.FU_APPLYING, ExecuteTime = TimeSpan.FromSeconds(10), NextState = DMTaskState.FU_REBOOTING },
                new DMTaskStep { CurrentState = DMTaskState.FU_REBOOTING, ExecuteTime = TimeSpan.FromSeconds(10), NextState = DMTaskState.DM_IDLE }
            };
        }

        protected override async Task<bool> OnEnterStateProc(DMTaskState state, ITransport transport)
        {
            bool succeed = true;
            var report = new TwinCollection();
            string status;

            switch (state)
            {
                case DMTaskState.FU_PENDING:
                    report = null;  // No report for entering pending state
                    break;

                case DMTaskState.FU_DOWNLOADING:
                    _watch = Stopwatch.StartNew();

                    try
                    {
                        using (var client = new HttpClient())
                        {
                            FirmwareVersion = (await client.GetStringAsync(Uri)).Trim();
                        }

                        status = "downloading";
                    }
                    catch (Exception ex)
                    {
                        succeed = false;
                        status = $"download failed: {ex.Message}";
                    }

                    report.Set(StatusPath, status);
                    report.Set(LogPath, _logBuilder.Append(status, succeed));
                    break;

                case DMTaskState.FU_APPLYING:
                    _watch = Stopwatch.StartNew();
                    succeed = FirmwareVersion != "applyFail";
                    status = succeed ? "applying" : "apply failed";
                    report.Set(StatusPath, status);
                    report.Set(LogPath, _logBuilder.Append(status, succeed));
                    break;

                case DMTaskState.FU_REBOOTING:
                    _watch = Stopwatch.StartNew();
                    succeed = FirmwareVersion != "rebootFail";
                    status = succeed ? "rebooting" : "reboot failed";
                    report.Set(StatusPath, status);
                    report.Set(LogPath, _logBuilder.Append(status, succeed));
                    break;

                case DMTaskState.DM_IDLE:
                    report.Set("iothubDM.firmwareUpdate.status", $"updated to {FirmwareVersion}");
                    report.Set("FirmwareVersion", FirmwareVersion);
                    break;

                default:
                    throw new ArgumentOutOfRangeException(nameof(state));
            }

            if (report != null)
            {
                await transport.UpdateReportedPropertiesAsync(report);
                Trace.TraceInformation($"Sent report {report.ToJson(Formatting.Indented)}");
            }

            return succeed;
        }

        protected override async Task<bool> OnLeaveStateProc(DMTaskState state, ITransport transport)
        {
            var report = new TwinCollection();

            switch (state)
            {
                case DMTaskState.DM_IDLE:
                    report = null;  // No report for leaving idle state
                    break;

                case DMTaskState.FU_PENDING:
                    report = null;  // No report for leaving pending state
                    break;

                case DMTaskState.FU_DOWNLOADING:
                    report.Set(StatusPath, "download completed");
                    report.Set(LogPath, _logBuilder.Append($"downloaded({(int)_watch.Elapsed.TotalSeconds}s)"));
                    break;

                case DMTaskState.FU_APPLYING:
                    report.Set(StatusPath, "apply completed");
                    report.Set(LogPath, _logBuilder.Append($"applied({(int)_watch.Elapsed.TotalSeconds}s)"));
                    break;

                case DMTaskState.FU_REBOOTING:
                    report.Set(StatusPath, "reboot completed");
                    report.Set(LogPath, _logBuilder.Append($"rebooted({(int)_watch.Elapsed.TotalSeconds}s)"));
                    break;

                default:
                    throw new ArgumentOutOfRangeException(nameof(state));
            }

            if (report != null)
            {
                await transport.UpdateReportedPropertiesAsync(report);
                Trace.TraceInformation($"Sent report {report.ToJson(Formatting.Indented)}");
            }

            return true;
        }
    }
}