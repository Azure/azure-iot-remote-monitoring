using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Azure.Devices.Applications.RemoteMonitoring.UnitTests.Infrastructure;

namespace Microsoft.Azure.Devices.Applications.RemoteMonitoring.UnitTests.TestStubs
{
    public class HttpMessageHandlerStub : HttpMessageHandler
    {
        protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request,
            CancellationToken cancellationToken)
        {
            if (Equals(request.RequestUri, ActionLogicTest.ENDPOINT))
            {
                var responseMessage = new HttpResponseMessage(HttpStatusCode.OK)
                {
                    Content = new StringContent("Successful action")
                };
                return await Task.FromResult(responseMessage);
            }
            else
            {
                var responseMessage = new HttpResponseMessage(HttpStatusCode.BadRequest)
                {
                    Content = new StringContent("Abort")
                };
                return await Task.FromResult(responseMessage);
            }
        }
    }
}