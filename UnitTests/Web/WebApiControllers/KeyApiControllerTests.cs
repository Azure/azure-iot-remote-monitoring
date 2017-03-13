using System;
using Microsoft.Azure.Devices.Applications.RemoteMonitoring.Common.Models;
using Microsoft.Azure.Devices.Applications.RemoteMonitoring.DeviceAdmin.Infrastructure.BusinessLogic;
using Microsoft.Azure.Devices.Applications.RemoteMonitoring.DeviceAdmin.Web.WebApiControllers;
using Xunit;

namespace Microsoft.Azure.Devices.Applications.RemoteMonitoring.UnitTests.Web.WebApiControllers
{
    public class KeyApiControllerTests : IDisposable
    {
        private readonly KeyApiController keyApiController;
        private readonly IKeyLogic keyLogic;
        private readonly ISecurityKeyGenerator securityKeyGenerator;

        public KeyApiControllerTests()
        {
            securityKeyGenerator = new SecurityKeyGenerator();
            keyLogic = new KeyLogic(securityKeyGenerator);
            keyApiController = new KeyApiController(keyLogic);
            keyApiController.InitializeRequest();
        }

        [Fact]
        public async void GetKeysAsyncTest()
        {
            var res = await keyApiController.GetKeysAsync();
            res.AssertOnError();
            var data = res.ExtractContentDataAs<SecurityKeys>();
            Assert.NotNull(data);
            Assert.NotNull(data.PrimaryKey);
            Assert.NotNull(data.SecondaryKey);
        }

        #region IDisposable Support
        private bool disposedValue = false; // To detect redundant calls

        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                    keyApiController.Dispose();
                }

                // TODO: free unmanaged resources (unmanaged objects) and override a finalizer below.
                // TODO: set large fields to null.

                disposedValue = true;
            }
        }

        // TODO: override a finalizer only if Dispose(bool disposing) above has code to free unmanaged resources.
        // ~KeyApiControllerTests() {
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