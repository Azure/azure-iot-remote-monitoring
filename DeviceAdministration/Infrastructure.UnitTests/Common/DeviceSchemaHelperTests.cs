using System;
using System.Globalization;
using Microsoft.Azure.Devices.Applications.RemoteMonitoring.Common.DeviceSchema;
using Microsoft.Azure.Devices.Applications.RemoteMonitoring.Common.Exceptions;
using Microsoft.Azure.Devices.Applications.RemoteMonitoring.Common.Models;
using Newtonsoft.Json.Linq;
using NUnit.Framework;

namespace Microsoft.Azure.Devices.Applications.RemoteMonitoring.DeviceAdmin.Infrastructure.UnitTests
{
    [TestFixture]
    public class DeviceSchemaHelperTests
    {
        #region DeviceProperty tests

        [Test]
        public void GetDevicePropertiesShouldReturnDeviceProperties()
        {
            Device d = GetValidDevice();

            DeviceProperties props = DeviceSchemaHelper.GetDeviceProperties(d);

            Assert.NotNull(props);
            Assert.AreEqual("test", props.DeviceID.ToString());
        }

        [Test]
        public void GetDevicePropertiesShouldThrowIfMissingDeviceProperties()
        {
            Device d = GetDeviceWithMissingDeviceProperties();

            Assert.Throws<DeviceRequiredPropertyNotFoundException>(() => DeviceSchemaHelper.GetDeviceProperties(d));
        }

        #endregion

        #region DeviceID tests

        [Test]
        public void GetDeviceIDShouldReturnDeviceID()
        {
            Device d = GetValidDevice();

            string deviceID = DeviceSchemaHelper.GetDeviceID(d);

            Assert.AreEqual("test", deviceID);
        }

        [Test]
        public void GetDeviceIDShouldThrowIfMissingDeviceProperties()
        {
            Device d = GetDeviceWithMissingDeviceProperties();

            Assert.Throws<DeviceRequiredPropertyNotFoundException>(() => DeviceSchemaHelper.GetDeviceID(d));
        }

        [Test]
        public void GetDeviceIDShouldThrowIfMissingDeviceID()
        {
            Device d = GetDeviceWithMissingDeviceID();

            Assert.Throws<DeviceRequiredPropertyNotFoundException>(() => DeviceSchemaHelper.GetDeviceID(d));
        }

        #endregion

        #region CreatedTime tests

        [Test]
        public void GetCreatedTimeShouldReturnCreatedTime()
        {
            Device d = GetValidDevice();

            var createdTime = DeviceSchemaHelper.GetCreatedTime(d);

            // Need to include RoundtripKind to get a UTC value
            var expectedTime = DateTime.Parse("2015-08-01T01:02:03.0000Z", null, DateTimeStyles.RoundtripKind);
            Assert.AreEqual(expectedTime, createdTime);
        }

        [Test]
        public void GetCreatedTimeShouldThrowIfMissingDeviceProperties()
        {
            Device d = GetDeviceWithMissingDeviceProperties();

            Assert.Throws<DeviceRequiredPropertyNotFoundException>(() => DeviceSchemaHelper.GetCreatedTime(d));
        }

        [Test]
        public void GetCreatedTimeShouldThrowIfMissingCreatedTime()
        {
            Device d = GetDeviceWithMissingCreatedTime();

            Assert.Throws<DeviceRequiredPropertyNotFoundException>(() => DeviceSchemaHelper.GetCreatedTime(d));
        }

        #endregion

        #region GetUpdatedTime tests

        [Test]
        public void GetUpdatedTimeShouldReturnUpdatedTime()
        {
            Device d = GetValidDevice();

            var updatedTime = DeviceSchemaHelper.GetUpdatedTime(d);

            // Need to include RoundtripKind to get a UTC value
            var expectedTime = DateTime.Parse("2015-09-01T01:02:03.0000Z", null, DateTimeStyles.RoundtripKind);
            Assert.AreEqual(expectedTime, updatedTime);
        }

        [Test]
        public void GetUpdatedTimeShouldThrowIfMissingDeviceProperties()
        {
            Device d = GetDeviceWithMissingDeviceProperties();

            Assert.Throws<DeviceRequiredPropertyNotFoundException>(() => DeviceSchemaHelper.GetUpdatedTime(d));
        }


        [Test]
        public void GetUpdatedTimeShouldReturnNullButNotThrowIfMissingUpdatedTime()
        {
            Device d = GetDeviceWithMissingUpdatedTime();

            var updatedTime = DeviceSchemaHelper.GetUpdatedTime(d);

            Assert.AreEqual(null, updatedTime);
        }

        #endregion

        #region HubEnabledState tests

        [Test]
        public void GetHubEnabledStateShouldReturnState()
        {
            var d = GetValidDevice();

            var hubEnabledState = DeviceSchemaHelper.GetHubEnabledState(d);

            Assert.AreEqual(true, hubEnabledState);
        }

        [Test]
        public void GetHubEnabledStateShouldThrowIfMissingDeviceProperties()
        {
            var d = GetDeviceWithMissingDeviceProperties();

            Assert.Throws<DeviceRequiredPropertyNotFoundException>(() => DeviceSchemaHelper.GetHubEnabledState(d));
        }

        [Test]
        public void GetHubEnabledStateShouldReturnNullButNotThrowIfMissingState()
        {
            var d = GetDeviceWithMissingHubEnabledState();

            var hubEnabledState = DeviceSchemaHelper.GetHubEnabledState(d);

            Assert.AreEqual(null, hubEnabledState);
        }

        #endregion

        private Device GetValidDevice()
        {
            string d = @"{ ""DeviceProperties"": 
                            { 
                                ""DeviceID"": ""test"", 
                                ""CreatedTime"": ""2015-08-01T01:02:03.0000Z"",
                                ""UpdatedTime"": ""2015-09-01T01:02:03.0000Z"",
                                ""HubEnabledState"": true
                            }
                        }";

            return ParseDeviceFromJson(d);
        }

        private Device GetDeviceWithMissingDeviceProperties()
        {
            string d = @"{ ""DeviceXXXProperties"": { ""DeviceID"": ""test"" } }";

            return ParseDeviceFromJson(d);
        }

        private Device GetDeviceWithMissingDeviceID()
        {
            string d = @"{ ""DeviceProperties"": { ""DeviXXXceID"": ""test"" } }";

            return ParseDeviceFromJson(d);
        }

        private Device GetDeviceWithMissingCreatedTime()
        {
            string d = @"{ ""DeviceProperties"": 
                            { 
                                ""DeviceID"": ""test"", 
                                ""CreatXXXedTime"": ""2015-08-01T01:02:03.0000Z"" 
                            }
                        }";

            return ParseDeviceFromJson(d);
        }

        private Device GetDeviceWithMissingUpdatedTime()
        {
            string d = @"{ ""DeviceProperties"": 
                            { 
                                ""DeviceID"": ""test"", 
                                ""CreatedTime"": ""2015-08-01T01:02:03.0000Z"",
                                ""UpdatXXXedTime"": ""2015-09-01T01:02:03.0000Z""
                            }
                        }";

            return ParseDeviceFromJson(d);
        }

        private Device GetDeviceWithMissingHubEnabledState()
        {
            string d = @"{ ""DeviceProperties"": 
                            { 
                                ""DeviceID"": ""test"", 
                                ""CreatedTime"": ""2015-08-01T01:02:03.0000Z"",
                                ""UpdatedTime"": ""2015-09-01T01:02:03.0000Z"",
                                ""HubEnaXXXbledState"": true
                            }
                        }";

            return ParseDeviceFromJson(d);
        }

        private Device ParseDeviceFromJson(string deviceAsJson)
        {
            return Newtonsoft.Json.JsonConvert.DeserializeObject<Device>(deviceAsJson);
        }
    }
}
