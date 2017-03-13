using System.Text;
using Xunit;
using Microsoft.Azure.Devices.Applications.RemoteMonitoring.Simulator.WebJob.SimulatorCore.Transport;
using System;

namespace Microsoft.Azure.Devices.Applications.RemoteMonitoring.UnitTests.Simulator.WebJob
{
    public class DeserializableCommandTest : IDisposable
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

        #region IDisposable Support
        private bool disposedValue = false; // To detect redundant calls

        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                    _message.Dispose();
                }

                // TODO: free unmanaged resources (unmanaged objects) and override a finalizer below.
                // TODO: set large fields to null.

                disposedValue = true;
            }
        }

        // TODO: override a finalizer only if Dispose(bool disposing) above has code to free unmanaged resources.
        // ~DeserializableCommandTest() {
        //   // Do not change this code. Put cleanup code in Dispose(bool disposing) above.
        //   Dispose(false);
        // }

        // This code added to correctly implement the disposable pattern.
        public void Dispose()
        {
            // Do not change this code. Put cleanup code in Dispose(bool disposing) above.
            Dispose(true);
            // TODO: uncomment the following line if the finalizer is overridden above.
            // GC.SuppressFinalize(this);
        }
        #endregion
    }
}
