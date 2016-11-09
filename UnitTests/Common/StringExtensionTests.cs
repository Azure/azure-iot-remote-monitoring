using Microsoft.Azure.Devices.Applications.RemoteMonitoring.Common.Extensions;
using System;
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

        [Fact]
        public void AllowedTableKeyTest()
        {
            Assert.False(string.Empty.IsAllowedTableKey());
            Assert.False("".IsAllowedTableKey());
            Assert.False(" ".IsAllowedTableKey());
            Assert.False("  ".IsAllowedTableKey());

            Assert.False("ab#c".IsAllowedTableKey());
            Assert.False("ab?c".IsAllowedTableKey());
            Assert.False("ab/c".IsAllowedTableKey());
            Assert.False(@"ab\c".IsAllowedTableKey());

            Assert.False("ab\t".IsAllowedTableKey());
            Assert.False("ab\n".IsAllowedTableKey());
            Assert.False("ab\r".IsAllowedTableKey());
            Assert.False((new string('a', 1025)).IsAllowedTableKey());

            Assert.True("ab中".IsAllowedTableKey());
            Assert.True(@"ABCabc123""''`!@$%^&*()-_+=[]|{}<>,.:;".IsAllowedTableKey());
        }
    }
}
