using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
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
        static internal string ReportPrefix = "Method.UpdateFirmware";
        static private string Status = "Status";
        static private string LastUpdate = "LastUpdate";
        static private string Duration = "Duration-s";
        static private string Running = "Running";
        static private string Failed = "Failed";
        static private string Complete = "Complete";

        private Stopwatch _masterWatch;
        private Stopwatch _stepWatch;

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
                new DMTaskStep { CurrentState = DMTaskState.FU_DOWNLOADING, ExecuteTime = TimeSpan.FromSeconds(20), NextState = DMTaskState.FU_APPLYING },
                new DMTaskStep { CurrentState = DMTaskState.FU_APPLYING, ExecuteTime = TimeSpan.FromSeconds(20), NextState = DMTaskState.FU_REBOOTING },
                new DMTaskStep { CurrentState = DMTaskState.FU_REBOOTING, ExecuteTime = TimeSpan.FromSeconds(20), NextState = DMTaskState.DM_IDLE }
            };
        }

        protected override async Task<bool> OnEnterStateProc(DMTaskState state, ITransport transport)
        {
            bool succeed = true;
            var report = new TwinCollection();

            switch (state)
            {
                case DMTaskState.FU_PENDING:
                    var clear = new TwinCollection();
                    clear.Set(ReportPrefix, null);
                    await transport.UpdateReportedPropertiesAsync(clear);

                    _masterWatch = Stopwatch.StartNew();
                    _stepWatch = Stopwatch.StartNew();
                    BuildReport(report, Running);
                    break;

                case DMTaskState.FU_DOWNLOADING:
                    _stepWatch = Stopwatch.StartNew();

                    try
                    {
                        using (var client = new HttpClient())
                        {
                            FirmwareVersion = (await client.GetStringAsync(Uri)).Trim();
                        }
                    }
                    catch
                    {
                        succeed = false;
                    }

                    BuildReport(report, "Download", succeed ? Running : Failed);
                    break;

                case DMTaskState.FU_APPLYING:
                    _stepWatch = Stopwatch.StartNew();
                    succeed = FirmwareVersion != "applyFail";
                    BuildReport(report, "Applied", succeed ? Running : Failed);
                    break;

                case DMTaskState.FU_REBOOTING:
                    _stepWatch = Stopwatch.StartNew();
                    succeed = FirmwareVersion != "rebootFail";
                    BuildReport(report, "Reboot", succeed ? Running : Failed);
                    break;

                case DMTaskState.DM_IDLE:
                    BuildReport(report, Complete);

                    report.Set(DeviceBase.StartupTimePropertyName, DateTime.UtcNow.ToString());
                    report.Set(DeviceBase.FirmwareVersionPropertyName, FirmwareVersion);
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

                case DMTaskState.FU_PENDING:
                    report = null;  // No report for leaving pending state
                    break;

                case DMTaskState.FU_DOWNLOADING:
                    BuildReport(report, "Download", Complete);
                    break;

                case DMTaskState.FU_APPLYING:
                    BuildReport(report, "Applied", Complete);
                    break;

                case DMTaskState.FU_REBOOTING:
                    BuildReport(report, "Reboot", Complete);
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

        private void BuildReport(TwinCollection report, string status)
        {
            report.Set(FormattableString.Invariant($"{ReportPrefix}.{Status}"), status);
            report.Set(FormattableString.Invariant($"{ReportPrefix}.{LastUpdate}"), DateTime.UtcNow.ToString(CultureInfo.CurrentCulture));
            report.Set(FormattableString.Invariant($"{ReportPrefix}.{Duration}"), (int)_masterWatch.Elapsed.TotalSeconds);
        }

        private void BuildReport(TwinCollection report, string stepName, string status)
        {
            report.Set(FormattableString.Invariant($"{ReportPrefix}.{stepName}.{Status}"), status);
            report.Set(FormattableString.Invariant($"{ReportPrefix}.{stepName}.{LastUpdate}"), DateTime.UtcNow.ToString(CultureInfo.CurrentCulture));
            report.Set(FormattableString.Invariant($"{ReportPrefix}.{stepName}.{Duration}"), (int)_stepWatch.Elapsed.TotalSeconds);

            if (status == Failed)
            {
                BuildReport(report, Failed);
            }
            else
            {
                BuildReport(report, Running);
            }
        }
    }
}