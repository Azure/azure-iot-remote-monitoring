using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Microsoft.Azure.Devices.Applications.RemoteMonitoring.Common.Extensions;
using Microsoft.Azure.Devices.Applications.RemoteMonitoring.Simulator.WebJob.SimulatorCore.Transport;
using Microsoft.Azure.Devices.Client;
using Microsoft.Azure.Devices.Shared;
using Newtonsoft.Json;

namespace Microsoft.Azure.Devices.Applications.RemoteMonitoring.Simulator.WebJob.SimulatorCore.Devices.DMTasks
{
    class ConfigurationUpdate : DMTaskBase
    {
        static private string LogPath = "Method.ConfigurationUpdate.Log";

        private Stopwatch _watch;
        private LogBuilder _logBuilder = new LogBuilder();

        public string Uri { get; private set; }
        public string Version { get; private set; }

        public ConfigurationUpdate(MethodRequest request)
        {
            var payload = JsonConvert.DeserializeObject<dynamic>(request.DataAsJson);

            var uri = (string)payload.ConfigUri;
            if (string.IsNullOrWhiteSpace(uri))
            {
                throw new ArgumentException("Missing ConfigUri");
            }

            // [WORKAROUND] Directly pick the version from the URI
            var match = new Regex("/configuration/(?<version>.*)$").Match(uri);
            if (!match.Success)
            {
                throw new ArgumentException("Bad format of ConfigUri");
            }

            Uri = uri;
            Version = match.Groups["version"].Value;

            // State switch graph: pending -> downloading -> applying -> idle
            _steps = new List<DMTaskStep>
            {
                new DMTaskStep { CurrentState = DMTaskState.CU_PENDING, ExecuteTime = TimeSpan.Zero, NextState = DMTaskState.CU_DOWNLOADING },
                new DMTaskStep { CurrentState = DMTaskState.CU_DOWNLOADING, ExecuteTime = TimeSpan.FromSeconds(10), NextState = DMTaskState.CU_APPLYING },
                new DMTaskStep { CurrentState = DMTaskState.CU_APPLYING, ExecuteTime = TimeSpan.FromSeconds(10), NextState = DMTaskState.DM_IDLE }
            };
        }

        protected override async Task<bool> OnEnterStateProc(DMTaskState state, ITransport transport)
        {
            bool succeed = true;
            var report = new TwinCollection();
            string status;

            switch (state)
            {
                case DMTaskState.CU_PENDING:
                    report = null;  // No report for entering pending state
                    break;

                case DMTaskState.CU_DOWNLOADING:
                    _watch = Stopwatch.StartNew();
                    succeed = Version != "downloadFail";
                    status = succeed ? "Downloading" : "Dowload failed";
                    report.Set(LogPath, _logBuilder.Append(status, succeed));
                    break;

                case DMTaskState.CU_APPLYING:
                    _watch = Stopwatch.StartNew();
                    succeed = Version != "applyFail";
                    status = succeed ? "Applying" : "Apply failed";
                    report.Set(LogPath, _logBuilder.Append(status, succeed));
                    break;

                case DMTaskState.DM_IDLE:
                    report.Set(DeviceBase.ConfigurationVersionPropertyName, Version);
                    break;

                default:
                    throw new ArgumentOutOfRangeException(nameof(state));
            }

            if (report != null)
            {
                await transport.UpdateReportedPropertiesAsync(report);
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

                case DMTaskState.CU_PENDING:
                    report = null;  // No report for leaving pending state
                    break;

                case DMTaskState.CU_DOWNLOADING:
                    report.Set(LogPath, _logBuilder.Append($"Downloaded({(int)_watch.Elapsed.TotalSeconds}s)"));
                    break;

                case DMTaskState.CU_APPLYING:
                    report.Set(LogPath, _logBuilder.Append($"Applied({(int)_watch.Elapsed.TotalSeconds}s)"));
                    break;

                default:
                    throw new ArgumentOutOfRangeException(nameof(state));
            }

            if (report != null)
            {
                await transport.UpdateReportedPropertiesAsync(report);
            }

            return true;
        }
    }
}