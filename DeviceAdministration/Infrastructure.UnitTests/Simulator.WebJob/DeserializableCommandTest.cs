using Microsoft.Azure.Devices.Applications.RemoteMonitoring.Simulator.WebJob.SimulatorCore.Serialization;
using Microsoft.Azure.Devices.Applications.RemoteMonitoring.Common.Models.Commands;
using Moq;
using System.Text;
using Xunit;
using Microsoft.Azure.Devices.Applications.RemoteMonitoring.Simulator.WebJob.SimulatorCore.Transport;
using System;

namespace Microsoft.Azure.Devices.Applications.RemoteMonitoring.UnitTests.Simulator.WebJob
{
    public class DeserializableCommandTest
    {
        private readonly Mock<ISerialize> _serializerMock;
        private readonly Client.Message _message;

        public DeserializableCommandTest()
        {
            _serializerMock = new Mock<ISerialize>();
            _message = new Client.Message(Encoding.UTF8.GetBytes("{}"));
        }

        [Fact]
        public void CreationFailureTest()
        {
            Assert.Throws<ArgumentNullException>(() => new DeserializableCommand(null, _serializerMock.Object));
            Assert.Throws<ArgumentNullException>(() => new DeserializableCommand(_message, null));
        }
    }
}
