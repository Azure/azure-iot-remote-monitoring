using System;
using Microsoft.Azure.Devices.Applications.RemoteMonitoring.DeviceAdmin.Web.Helpers;
using Microsoft.Azure.Devices.Applications.RemoteMonitoring.DeviceAdmin.Web.Models;
using Ploeh.AutoFixture;
using Xunit;

namespace Microsoft.Azure.Devices.Applications.RemoteMonitoring.UnitTests.Web.Helpers
{
    public class DeviceDisplayHelperTests
    {
        private readonly IFixture fixture = new Fixture();

        [Fact]
        public void GetCommandResultClassNameTest()
        {
            var res = DeviceDisplayHelper.GetCommandResultClassName("Success");
            Assert.Equal(res, "Success");

            res = DeviceDisplayHelper.GetCommandResultClassName("Expired");
            Assert.Equal(res, "Error");

            res = DeviceDisplayHelper.GetCommandResultClassName("DeliveryCountExceeded");
            Assert.Equal(res, "Error");

            res = DeviceDisplayHelper.GetCommandResultClassName("Rejected");
            Assert.Equal(res, "Error");

            res = DeviceDisplayHelper.GetCommandResultClassName("Purged");
            Assert.Equal(res, "Purged");

            res = DeviceDisplayHelper.GetCommandResultClassName(null);
            Assert.Equal(res, "pending");
        }

        [Fact]
        public void BuildAvailableCommandNameSetTest()
        {
            Assert.Throws<ArgumentNullException>(() => DeviceDisplayHelper.BuildAvailableCommandNameSet(null));

            var model = fixture.Create<DeviceCommandModel>();
            model.SendCommandModel = null;
            var res = DeviceDisplayHelper.BuildAvailableCommandNameSet(model);
            Assert.Equal(res.Count, 0);

            model = fixture.Create<DeviceCommandModel>();
            model.SendCommandModel.CommandSelectList = null;
            res = DeviceDisplayHelper.BuildAvailableCommandNameSet(model);
            Assert.Equal(res.Count, 0);

            model = fixture.Create<DeviceCommandModel>();
            model.SendCommandModel.CommandSelectList.Add(null);
            res = DeviceDisplayHelper.BuildAvailableCommandNameSet(model);
            Assert.Equal(res.Count, model.SendCommandModel.CommandSelectList.Count - 1);
        }

        [Fact]
        public void GetIsCopyControlPropertyNameTest()
        {
            var res = DeviceDisplayHelper.GetIsCopyControlPropertyName("deviceId");
            Assert.Equal(res, true);
            res = DeviceDisplayHelper.GetIsCopyControlPropertyName("hostname");
            Assert.Equal(res, true);
            res = DeviceDisplayHelper.GetIsCopyControlPropertyName(null);
            Assert.Equal(res, false);
            res = DeviceDisplayHelper.GetIsCopyControlPropertyName("abc");
            Assert.Equal(res, false);
        }

        [Fact]
        public void GetLocalizedCommandResultTextTest()
        {
            var res = DeviceDisplayHelper.GetLocalizedCommandResultText("", "");
            Assert.Equal(res.Key, "Pending");
            Assert.Equal(res.Value, "");

            res = DeviceDisplayHelper.GetLocalizedCommandResultText("Success", "");
            Assert.Equal(res.Value, "");
            Assert.Equal(res.Key, "Success");

            res = DeviceDisplayHelper.GetLocalizedCommandResultText("Expired", "");
            Assert.Equal(res.Key, "Error");
            Assert.Equal(res.Value, "Expired");

            res = DeviceDisplayHelper.GetLocalizedCommandResultText("DeliveryCountExceeded", "");
            Assert.Equal(res.Key, "Error");
            Assert.Equal(res.Value, "Delivery Count Exceeded");

            res = DeviceDisplayHelper.GetLocalizedCommandResultText("Rejected", "");
            Assert.Equal(res.Key, "Error");
            Assert.Equal(res.Value, "Rejected");
        }

        [Fact]
        public void GetDevicePropertyFieldLocalNameTest()
        {
            var localName = DeviceDisplayHelper.GetDevicePropertyFieldLocalName("");
            Assert.Equal(localName, "");

            localName = DeviceDisplayHelper.GetDevicePropertyFieldLocalName(null);
            Assert.Equal(localName, null);

            localName = DeviceDisplayHelper.GetDevicePropertyFieldLocalName("deviceId");
            Assert.Equal(localName, "deviceId");

            // reads from resx
            localName = DeviceDisplayHelper.GetDevicePropertyFieldLocalName("UpdatedTime");
            Assert.Equal(localName, "UpdatedTime (UTC)");
        }
    }
}