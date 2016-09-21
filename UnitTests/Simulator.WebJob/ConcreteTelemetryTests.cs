using Microsoft.Azure.Devices.Applications.RemoteMonitoring.Common.Models;
using Microsoft.Azure.Devices.Applications.RemoteMonitoring.Simulator.WebJob.SimulatorCore.Logging;
using Microsoft.Azure.Devices.Applications.RemoteMonitoring.Simulator.WebJob.SimulatorCore.Telemetry;
using Moq;
using System;
using System.Threading;
using System.Threading.Tasks;
using Xunit;

namespace Microsoft.Azure.Devices.Applications.RemoteMonitoring.UnitTests.Simulator.WebJob
{
    public class ConcreteTelemetryTests
    {
        private readonly Mock<ILogger> _loggerMock;
        private ConcreteTelemetry telemetry;
        private string testMessage;

        public ConcreteTelemetryTests()
        {
            _loggerMock = new Mock<ILogger>();
            this.telemetry = new ConcreteTelemetry(this._loggerMock.Object);
            this.testMessage = "test";
            this.telemetry.MessageBody = this.testMessage;
            this.telemetry.DelayBefore = new System.TimeSpan(100);
        }

        [Fact]
        public async void SendEventsAsyncRepeatForeverTests()
        {
            this.telemetry.RepeatForever = true;
            this.telemetry.RepeatCount = 1;

            var foreverCount = 3; 
            var i = 0;

            Func<object, Task> onSend = async (msg) =>
            {
                Assert.Equal(msg, this.testMessage);

                i++;

                if (i == foreverCount)
                {
                    this.telemetry.RepeatForever = false;
                }

                await Task.Delay(0);
            };

            await this.telemetry.SendEventsAsync(new CancellationToken(), onSend);
            Assert.Equal(i, foreverCount);
        }

        [Fact]
        public async void SendEventsAsyncRepeatCountTests()
        {
            var count = 3;
            var i = 0;

            this.telemetry.RepeatForever = false;
            this.telemetry.RepeatCount = count;

            Func<object, Task> onSend = async (msg) =>
            {
                Assert.Equal(msg, this.testMessage);

                i++;

                await Task.Delay(0);
            };

            await this.telemetry.SendEventsAsync(new CancellationToken(), onSend);
            Assert.Equal(i, count);
        }

        [Fact]
        public async void SendEventsAsyncCancelationTokenTests()
        {
            this.telemetry.RepeatCount = 10;

            var cancellationCount = 3;
            var i = 0;

            CancellationTokenSource cancellationTokenSource = new CancellationTokenSource();

            Func<object, Task> onSend = async (msg) =>
            {
                Assert.Equal(msg, this.testMessage);

                i++;

                if (i == cancellationCount)
                {
                    cancellationTokenSource.Cancel();
                }

                await Task.Delay(0);
            };

            await Assert.ThrowsAsync<OperationCanceledException>(async () => await this.telemetry.SendEventsAsync(cancellationTokenSource.Token, onSend));
            Assert.Equal(i, cancellationCount);
        }
    }
}