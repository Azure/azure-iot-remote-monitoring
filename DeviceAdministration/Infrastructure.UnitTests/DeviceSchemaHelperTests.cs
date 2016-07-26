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
            DeviceND d = GetValidDevice();

            DeviceProperties props = DeviceSchemaHelperND.GetDeviceProperties(d);

            Assert.NotNull(props);
            Assert.AreEqual("test", props.DeviceID.ToString());
        }

        [Test]
        public void GetDevicePropertiesShouldThrowIfMissingDeviceProperties()
        {
            DeviceND d = GetDeviceWithMissingDeviceProperties();

            Assert.Throws<DeviceRequiredPropertyNotFoundException>(() => DeviceSchemaHelperND.GetDeviceProperties(d));
        }

        #endregion

        #region DeviceID tests

        [Test]
        public void GetDeviceIDShouldReturnDeviceID()
        {
            DeviceND d = GetValidDevice();

            string deviceID = DeviceSchemaHelperND.GetDeviceID(d);

            Assert.AreEqual("test", deviceID);
        }

        [Test]
        public void GetDeviceIDShouldThrowIfMissingDeviceProperties()
        {
            DeviceND d = GetDeviceWithMissingDeviceProperties();

            Assert.Throws<DeviceRequiredPropertyNotFoundException>(() => DeviceSchemaHelperND.GetDeviceID(d));
        }

        [Test]
        public void GetDeviceIDShouldThrowIfMissingDeviceID()
        {
            DeviceND d = GetDeviceWithMissingDeviceID();

            Assert.Throws<DeviceRequiredPropertyNotFoundException>(() => DeviceSchemaHelperND.GetDeviceID(d));
        }

        #endregion

        #region CreatedTime tests

        [Test]
        public void GetCreatedTimeShouldReturnCreatedTime()
        {
            DeviceND d = GetValidDevice();

            var createdTime = DeviceSchemaHelperND.GetCreatedTime(d);

            // Need to include RoundtripKind to get a UTC value
            var expectedTime = DateTime.Parse("2015-08-01T01:02:03.0000Z", null, DateTimeStyles.RoundtripKind);
            Assert.AreEqual(expectedTime, createdTime);
        }

        [Test]
        public void GetCreatedTimeShouldThrowIfMissingDeviceProperties()
        {
            DeviceND d = GetDeviceWithMissingDeviceProperties();

            Assert.Throws<DeviceRequiredPropertyNotFoundException>(() => DeviceSchemaHelperND.GetCreatedTime(d));
        }

        [Test]
        public void GetCreatedTimeShouldThrowIfMissingCreatedTime()
        {
            DeviceND d = GetDeviceWithMissingCreatedTime();

            Assert.Throws<DeviceRequiredPropertyNotFoundException>(() => DeviceSchemaHelperND.GetCreatedTime(d));
        }

        #endregion

        #region GetUpdatedTime tests

        [Test]
        public void GetUpdatedTimeShouldReturnUpdatedTime()
        {
            DeviceND d = GetValidDevice();

            var updatedTime = DeviceSchemaHelperND.GetUpdatedTime(d);

            // Need to include RoundtripKind to get a UTC value
            var expectedTime = DateTime.Parse("2015-09-01T01:02:03.0000Z", null, DateTimeStyles.RoundtripKind);
            Assert.AreEqual(expectedTime, updatedTime);
        }

        [Test]
        public void GetUpdatedTimeShouldThrowIfMissingDeviceProperties()
        {
            DeviceND d = GetDeviceWithMissingDeviceProperties();

            Assert.Throws<DeviceRequiredPropertyNotFoundException>(() => DeviceSchemaHelperND.GetUpdatedTime(d));
        }


        [Test]
        public void GetUpdatedTimeShouldReturnNullButNotThrowIfMissingUpdatedTime()
        {
            DeviceND d = GetDeviceWithMissingUpdatedTime();

            var updatedTime = DeviceSchemaHelperND.GetUpdatedTime(d);

            Assert.AreEqual(null, updatedTime);
        }

        #endregion

        #region HubEnabledState tests

        [Test]
        public void GetHubEnabledStateShouldReturnState()
        {
            var d = GetValidDevice();

            var hubEnabledState = DeviceSchemaHelperND.GetHubEnabledState(d);

            Assert.AreEqual(true, hubEnabledState);
        }

        [Test]
        public void GetHubEnabledStateShouldThrowIfMissingDeviceProperties()
        {
            var d = GetDeviceWithMissingDeviceProperties();

            Assert.Throws<DeviceRequiredPropertyNotFoundException>(() => DeviceSchemaHelperND.GetHubEnabledState(d));
        }

        [Test]
        public void GetHubEnabledStateShouldReturnNullButNotThrowIfMissingState()
        {
            var d = GetDeviceWithMissingHubEnabledState();

            var hubEnabledState = DeviceSchemaHelperND.GetHubEnabledState(d);

            Assert.AreEqual(null, hubEnabledState);
        }

        #endregion

        private DeviceND GetValidDevice()
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

        private DeviceND GetDeviceWithMissingDeviceProperties()
        {
            string d = @"{ ""DeviceXXXProperties"": { ""DeviceID"": ""test"" } }";

            return ParseDeviceFromJson(d);
        }

        private DeviceND GetDeviceWithMissingDeviceID()
        {
            string d = @"{ ""DeviceProperties"": { ""DeviXXXceID"": ""test"" } }";

            return ParseDeviceFromJson(d);
        }

        private DeviceND GetDeviceWithMissingCreatedTime()
        {
            string d = @"{ ""DeviceProperties"": 
                            { 
                                ""DeviceID"": ""test"", 
                                ""CreatXXXedTime"": ""2015-08-01T01:02:03.0000Z"" 
                            }
                        }";

            return ParseDeviceFromJson(d);
        }

        private DeviceND GetDeviceWithMissingUpdatedTime()
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

        private DeviceND GetDeviceWithMissingHubEnabledState()
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

        private DeviceND ParseDeviceFromJson(string deviceAsJson)
        {
            return Newtonsoft.Json.JsonConvert.DeserializeObject<DeviceND>(deviceAsJson);
        }
    }
}
