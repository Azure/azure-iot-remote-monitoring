using System.Collections.Generic;
using System.Threading.Tasks;
using System.Web.Http;
using Microsoft.Azure.Devices.Applications.RemoteMonitoring.Common.Models;
using Microsoft.Azure.Devices.Applications.RemoteMonitoring.Common.Models.Commands;
using Microsoft.Azure.Devices.Applications.RemoteMonitoring.DeviceAdmin.Infrastructure.BusinessLogic;
using Microsoft.Azure.Devices.Applications.RemoteMonitoring.DeviceAdmin.Infrastructure.Models;
using Microsoft.Azure.Devices.Applications.RemoteMonitoring.DeviceAdmin.Web.DataTables;
using Microsoft.Azure.Devices.Applications.RemoteMonitoring.DeviceAdmin.Web.WebApiControllers;
using Moq;
using Newtonsoft.Json.Linq;
using Ploeh.AutoFixture;
using Xunit;
using Microsoft.Azure.Devices.Applications.RemoteMonitoring.DeviceAdmin.Infrastructure.Repository;
using Microsoft.WindowsAzure.Storage.Blob;
using Ploeh.AutoFixture.Kernel;
using System.Linq;
using System;

namespace Microsoft.Azure.Devices.Applications.RemoteMonitoring.UnitTests.Web.WebApiControllers
{
    public class DeviceIconApiControllerTests : IDisposable
    {
        private readonly DeviceIconApiController deviceIconApiController;
        private readonly Mock<IIoTHubDeviceManager> deviceManager;
        private readonly Mock<IDeviceIconRepository> deviceIconRepository;
        private readonly IFixture fixture;

        public DeviceIconApiControllerTests()
        {
            deviceManager = new Mock<IIoTHubDeviceManager>();
            deviceIconRepository = new Mock<IDeviceIconRepository>();
            deviceIconApiController = new DeviceIconApiController(deviceManager.Object, deviceIconRepository.Object);
            fixture = new Fixture();
        }

        #region IDisposable Support
        private bool disposedValue = false; // To detect redundant calls

        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                    deviceIconApiController.Dispose();
                }

                // TODO: free unmanaged resources (unmanaged objects) and override a finalizer below.
                // TODO: set large fields to null.

                disposedValue = true;
            }
        }

        // TODO: override a finalizer only if Dispose(bool disposing) above has code to free unmanaged resources.
        // ~DeviceIconApiControllerTests() {
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
