using System;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.Azure.Devices.Applications.RemoteMonitoring.Common.Helpers;
using Microsoft.Azure.Devices.Common.Exceptions;
using Moq;
using Xunit;

namespace Microsoft.Azure.Devices.Applications.RemoteMonitoring.UnitTests.Common
{
    public class AzureRetryHelperTests
    {
        private static int count;

        [Fact]
        public async void OperationWithBasicRetryAsyncTest_HttpRequestException()
        {
            count = 0;
            await AzureRetryHelper.OperationWithBasicRetryAsync(async () => await this.Function(new HttpRequestException()));
            Assert.Equal(count, 2);
        }

        [Fact]
        public async void OperationWithBasicRetryAsyncRetryTest_IotHubException()
        {
            count = 0;
            await AzureRetryHelper.OperationWithBasicRetryAsync(async () => await this.Function(new IotHubException("MSG", true)));
            Assert.Equal(count, 2);
        }

        [Fact]
        public async void OperationWithBasicRetryAsyncRetryTest_WebException()
        {
            count = 0;
            var response = new Mock<HttpWebResponse>();
            var ex = new WebException("message",
                                      new Exception(),
                                      WebExceptionStatus.Timeout,
                                      response.Object);
            await AzureRetryHelper.OperationWithBasicRetryAsync(async () => await this.Function(ex));
            Assert.Equal(count, 2);
        }

        [Fact]
        public async void OperationWithBasicRetryAsyncRetryTest_Exception()
        {
            count = 0;
            await
                Assert.ThrowsAsync<Exception>(
                                              async () =>
                                              await AzureRetryHelper.OperationWithBasicRetryAsync(async () => await this.Function(new Exception())));
        }

        private async Task Function(Exception ex)
        {
            if (count == 0)
            {
                count++;
                throw ex;
            }
            if (count == 1)
            {
                count++;
                await Task.FromResult(true);
                return;
            }
            count++;
        }
    }
}
