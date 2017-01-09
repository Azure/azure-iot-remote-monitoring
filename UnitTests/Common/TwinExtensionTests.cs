using System;
using System.Linq;
using Microsoft.Azure.Devices.Applications.RemoteMonitoring.Common.Extensions;
using Microsoft.Azure.Devices.Shared;
using Xunit;

namespace Microsoft.Azure.Devices.Applications.RemoteMonitoring.UnitTests.Common
{
    public class TwinExtensionTests
    {
        [Fact]
        public void GetTest()
        {
            string deviceId = Guid.NewGuid().ToString();
            var now = DateTime.Now;

            var twin = new Twin(deviceId);
            twin.Tags["x"] = "one";
            twin.Properties.Desired["y"] = 1;
            twin.Properties.Reported["z"] = now;

            Assert.Equal(twin.Get("deviceId"), deviceId);
            Assert.Equal(twin.Get("twin.deviceId"), deviceId);
            Assert.Equal(twin.Get("tags.x").ToString(), "one");
            Assert.Equal(twin.Get("twin.tags.x").ToString(), "one");
            Assert.Equal((int)twin.Get("properties.desired.y"), 1);
            Assert.Equal((int)twin.Get("twin.properties.desired.y"), 1);
            Assert.Equal((DateTime)twin.Get("properties.reported.z"), now);
            Assert.Equal((DateTime)twin.Get("twin.properties.reported.z"), now);
        }

        [Fact]
        public void GetShouldReturnNullIfFullNameIsInvalidate()
        {
            var twin = new Twin();

            Assert.Null(twin.Get("prefix.x"));
        }

        [Fact]
        public void GetShouldReturnNullIfTagIsNotDefined()
        {
            var twin = new Twin();

            Assert.Null(twin.Get("tags.x"));
        }

        [Fact]
        public void SetTest()
        {
            string deviceId = Guid.NewGuid().ToString();

            var twin = new Twin();

            twin.Set("deviceId", deviceId);
            twin.Set("tags.x", "one");
            twin.Set("twin.tags.X", "ONE");
            twin.Set("properties.desired.y", 1);
            twin.Set("twin.properties.desired.Y", 2);

            Assert.Equal(twin.DeviceId.ToString(), deviceId);
            Assert.Equal(twin.Tags["x"].ToString(), "one");
            Assert.Equal(twin.Tags["X"].ToString(), "ONE");
            Assert.Equal((int)twin.Properties.Desired["y"], 1);
            Assert.Equal((int)twin.Properties.Desired["Y"], 2);
        }

        [Fact]
        public void SetShouldNotThrowIfFullNameIsInvalidate()
        {
            var twin = new Twin();

            twin.Set("prefix.x", "one");
        }

        [Fact]
        public void UpdateRequiredTest()
        {
            var current = new Twin();
            current.Tags["x"] = 1;
            current.Properties.Desired["y"] = "one";

            var existing = new Twin();
            existing.Tags["x"] = 1;
            existing.Properties.Desired["y"] = "one";

            Assert.False(current.UpdateRequired(existing));

            existing.Properties.Reported["z"] = DateTime.Now;
            Assert.False(current.UpdateRequired(existing));

            existing.Tags["x"] = 2;
            Assert.True(current.UpdateRequired(existing));

            existing.Tags["x"] = current.Tags["x"];
            existing.Properties.Desired["y"] = "two";
            Assert.True(current.UpdateRequired(existing));

            existing.Properties.Desired["y"] = current.Properties.Desired["y"];
            existing.Tags["xx"] = 3;
            Assert.True(current.UpdateRequired(existing));
        }

        [Fact]
        public void GetNameListTest()
        {
            var twinA = new Twin();
            twinA.Tags.Set("Tag0", "value");
            twinA.Tags.Set("Tag1.Sub0", "value");
            twinA.Tags.Set("Tag1.Sub1", "value");

            var twinB = new Twin();
            twinA.Tags.Set("Tag0", "value");
            twinA.Tags.Set("Tag2.Sub0", "value");
            twinA.Tags.Set("Tag2.Sub1", "value");

            var names = (new Twin[] { twinA, twinB }).GetNameList(twin => twin.Tags);
            Assert.True(names.SequenceEqual(new string[] { "Tag0", "Tag1.Sub0", "Tag1.Sub1", "Tag2.Sub0", "Tag2.Sub1" }));

            Assert.False((new Twin[] { }).GetNameList(twin => twin.Tags).Any());
        }
    }
}
