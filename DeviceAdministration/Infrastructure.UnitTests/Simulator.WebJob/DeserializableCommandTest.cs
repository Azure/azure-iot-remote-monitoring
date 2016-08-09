using System.Text;
using Xunit;
using Microsoft.Azure.Devices.Applications.RemoteMonitoring.Simulator.WebJob.SimulatorCore.Transport;
using System;

namespace Microsoft.Azure.Devices.Applications.RemoteMonitoring.UnitTests.Simulator.WebJob
{
    public class DeserializableCommandTest
    {
        private readonly Client.Message _message;

        public DeserializableCommandTest()
        {
            _message = new Client.Message(Encoding.UTF8.GetBytes("{}"));
        }

        [Fact]
        public void CreationFailureTest()
        {
            Assert.Throws<ArgumentNullException>(() => new DeserializableCommand(null));
        }
    }
}
