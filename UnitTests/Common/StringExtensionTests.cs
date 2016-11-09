using Microsoft.Azure.Devices.Applications.RemoteMonitoring.Common.Extensions;
using Xunit;

namespace Microsoft.Azure.Devices.Applications.RemoteMonitoring.UnitTests.Common
{
    public class StringExtensionTests
    {
        [Fact]
        public void TryTrimPrefixTest()
        {
            const string prefix = "prefix.";
            const string prefix2 = "prefix2.";
            const string content = "content";

            string input = prefix + content;
            Assert.Equal(input.TryTrimPrefix(prefix), content);
            Assert.Equal(input.TryTrimPrefix(prefix2), input);

            string output;
            Assert.True(input.TryTrimPrefix(prefix, out output));
            Assert.Equal(output, content);

            Assert.False(input.TryTrimPrefix(prefix2, out output));
            Assert.Equal(output, input);
        }
    }
}
