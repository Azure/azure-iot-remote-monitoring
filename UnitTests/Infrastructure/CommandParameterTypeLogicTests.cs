using System;
using Microsoft.Azure.Devices.Applications.RemoteMonitoring.DeviceAdmin.Infrastructure.BusinessLogic;
using Xunit;

namespace Microsoft.Azure.Devices.Applications.RemoteMonitoring.UnitTests.Infrastructure
{
    public class CommandParameterTypeLogicTests
    {
        [Fact]
        public void HandlesIntAsInt32()
        {
            Assert.Equal(CommandTypes.Types["int32"], CommandTypes.Types["int"]);
        }

        [Fact]
        public void IsValid()
        {
            Assert.True(CommandParameterTypeLogic.Instance.IsValid("string", null));
            Assert.True(CommandParameterTypeLogic.Instance.IsValid("binary", null));
            Assert.False(CommandParameterTypeLogic.Instance.IsValid("int", null));
            Assert.True(CommandParameterTypeLogic.Instance.IsValid("int", 3));
            Assert.True(CommandParameterTypeLogic.Instance.IsValid("double", 3.1));
            Assert.False(CommandParameterTypeLogic.Instance.IsValid("double", "invalid"));
            Assert.True(CommandParameterTypeLogic.Instance.IsValid("int64", 3));
            Assert.False(CommandParameterTypeLogic.Instance.IsValid("int64", "invalid"));
            Assert.True(CommandParameterTypeLogic.Instance.IsValid("decimal", 3));
            Assert.False(CommandParameterTypeLogic.Instance.IsValid("decimal", "invalid"));
            Assert.True(CommandParameterTypeLogic.Instance.IsValid("boolean", true));
            Assert.False(CommandParameterTypeLogic.Instance.IsValid("boolean", "invalid"));
            Assert.True(CommandParameterTypeLogic.Instance.IsValid("datetimeoffset", DateTimeOffset.Now));
            Assert.False(CommandParameterTypeLogic.Instance.IsValid("datetimeoffset", "invalid"));
            Assert.True(CommandParameterTypeLogic.Instance.IsValid("date", DateTime.Now));
            Assert.False(CommandParameterTypeLogic.Instance.IsValid("date", "invalid"));
            Assert.True(CommandParameterTypeLogic.Instance.IsValid("guid", Guid.NewGuid()));
            Assert.False(CommandParameterTypeLogic.Instance.IsValid("guid", "invalid"));
            Assert.True(CommandParameterTypeLogic.Instance.IsValid("binary", "fbsIV6w7gfVUyoRIQFSVgw =="));
            Assert.False(CommandParameterTypeLogic.Instance.IsValid("binary", "invalid"));
            Assert.False(CommandParameterTypeLogic.Instance.IsValid(null, null));
        }

        [Fact]
        public void Get()
        {
            Assert.Null(CommandParameterTypeLogic.Instance.Get("string", null));
        }
    }
}