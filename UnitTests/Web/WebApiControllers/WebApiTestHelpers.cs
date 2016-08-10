using System.Net.Http;
using System.Web.Http;
using Xunit;

namespace Microsoft.Azure.Devices.Applications.RemoteMonitoring.UnitTests.Web.
    WebApiControllers
{
    public static class WebApiTestHelpers
    {
        public static void InitializeRequest(this ApiController contoller)
        {
            contoller.Request = new HttpRequestMessage();
            contoller.Request.SetConfiguration(new HttpConfiguration());
        }

        public static void AssertOnError(this HttpResponseMessage result)
        {
            Assert.True(result.IsSuccessStatusCode);
            Assert.Equal(result.ReasonPhrase, "OK");
        }

        public static T ExtractContentAs<T>(this HttpResponseMessage response)
        {
            T content;
            var contentExists = response.TryGetContentValue(out content);
            Assert.True(contentExists);
            return content;
        }

        public static T ExtractContentDataAs<T>(this HttpResponseMessage response)
        {
            dynamic content;
            var contentExists = response.TryGetContentValue(out content);
            Assert.True(contentExists);
            return (T) content.Data;
        }
    }
}