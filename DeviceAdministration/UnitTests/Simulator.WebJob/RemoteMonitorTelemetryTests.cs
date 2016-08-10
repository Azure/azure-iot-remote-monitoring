
using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Azure.Devices.Applications.RemoteMonitoring.Simulator.WebJob.SimulatorCore.Devices;
using Microsoft.Azure.Devices.Applications.RemoteMonitoring.Simulator.WebJob.SimulatorCore.Logging;
using Microsoft.Azure.Devices.Applications.RemoteMonitoring.Simulator.WebJob.SimulatorCore.Telemetry;
using Microsoft.Azure.Devices.Applications.RemoteMonitoring.Simulator.WebJob.Cooler.Telemetry;
using Microsoft.Azure.Devices.Applications.RemoteMonitoring.Simulator.WebJob.Cooler.Telemetry.Data;
using Xunit;
using Moq;
using Ploeh.AutoFixture;

namespace Microsoft.Azure.Devices.Applications.RemoteMonitoring.UnitTests.Simulator.WebJob
{
    class RemoteMonitorTelemetryTests
    {
        private Mock<ILogger> _loggerMock;
        private string testDeviceId;
        private RemoteMonitorTelemetry telemetry;

        public RemoteMonitorTelemetryTests()
        {
            this._loggerMock = new Mock<ILogger>();
            this.testDeviceId = "testDeviceId";

            this.telemetry = new RemoteMonitorTelemetry(this._loggerMock.Object, this.testDeviceId);
        }

        [Fact]
        public async void SendEventsAsyncPreCancelledTest()
        {
            CancellationTokenSource cancellationTokenSource = new CancellationTokenSource();

            cancellationTokenSource.Cancel();

            int count = 0;

            await this.telemetry.SendEventsAsync(cancellationTokenSource.Token, async (obj) =>
            {
                count++;
                await Task.Delay(0);
            });

            Assert.Equal(count, 0);
        }

        [Fact]
        public async void SendEventsAsyncCancelledTest()
        {
            CancellationTokenSource cancellationTokenSource = new CancellationTokenSource();

            int count = 0;

            await this.telemetry.SendEventsAsync(cancellationTokenSource.Token, async (obj) =>
            {
                count++;
                cancellationTokenSource.Cancel();
                await Task.Delay(0);
            });

            Assert.Equal(count, 1);
        }

        [Fact]
        public async void SendEventsAsyncInactiveTelemetryTest()
        {
            this.telemetry.TelemetryActive = false;

            CancellationTokenSource cancellationTokenSource = new CancellationTokenSource();

            cancellationTokenSource.CancelAfter(TimeSpan.FromMilliseconds(250));

            int count = 0;

            await this.telemetry.SendEventsAsync(cancellationTokenSource.Token, async (obj) =>
            {
                count++;
                await Task.Delay(0);
            });

            Assert.Equal(count, 0);
        }

        [Fact]
        public async void SendEventsAsyncActiveTelemetryTest()
        {
            CancellationTokenSource cancellationTokenSource = new CancellationTokenSource();

            await this.telemetry.SendEventsAsync(cancellationTokenSource.Token, async (obj) =>
            {
                Assert.IsType<RemoteMonitorTelemetryData>(obj);

                Assert.Null(((RemoteMonitorTelemetryData)obj).ExternalTemperature);

                Assert.NotNull(((RemoteMonitorTelemetryData)obj).DeviceId);
                Assert.NotNull(((RemoteMonitorTelemetryData)obj).Humidity);
                Assert.NotNull(((RemoteMonitorTelemetryData)obj).Temperature);

                await Task.Delay(0);

                cancellationTokenSource.Cancel();
            });
        }

        [Fact]
        public async void SendEventsAsyncActiveTempTelemetryTest()
        {
            this.telemetry.ActivateExternalTemperature = true;

            CancellationTokenSource cancellationTokenSource = new CancellationTokenSource();

            await this.telemetry.SendEventsAsync(cancellationTokenSource.Token, async (obj) =>
            {
                Assert.IsType<RemoteMonitorTelemetryData>(obj);
                Assert.NotNull(((RemoteMonitorTelemetryData)obj).ExternalTemperature);
                Assert.NotNull(((RemoteMonitorTelemetryData)obj).DeviceId);
                Assert.NotNull(((RemoteMonitorTelemetryData)obj).Humidity);
                Assert.NotNull(((RemoteMonitorTelemetryData)obj).Temperature);

                await Task.Delay(0);

                cancellationTokenSource.Cancel();
            });
        }
    }
}
