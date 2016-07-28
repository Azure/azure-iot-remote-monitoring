using System;
using System.Globalization;
using Microsoft.Azure.Devices.Applications.RemoteMonitoring.Common.DeviceSchema;
using Microsoft.Azure.Devices.Applications.RemoteMonitoring.Common.Exceptions;
using Microsoft.Azure.Devices.Applications.RemoteMonitoring.Common.Models;
using Newtonsoft.Json.Linq;
using Xunit;

namespace Microsoft.Azure.Devices.Applications.RemoteMonitoring.DeviceAdmin.Infrastructure.UnitTests
{
    public class DeviceSchemaHelperTests
    {
        #region DeviceProperty tests

        [Fact]
        public void GetDevicePropertiesShouldReturnDeviceProperties()
        {
            Device d = GetValidDevice();

            DeviceProperties props = DeviceSchemaHelper.GetDeviceProperties(d);

            Assert.NotNull(props);
            Assert.Equal("test", props.DeviceID.ToString());
        }

        [Fact]
        public void GetDevicePropertiesShouldThrowIfMissingDeviceProperties()
        {
            Device d = GetDeviceWithMissingDeviceProperties();

            Assert.Throws<DeviceRequiredPropertyNotFoundException>(() => DeviceSchemaHelper.GetDeviceProperties(d));
        }

        #endregion

        #region DeviceID tests

        [Fact]
        public void GetDeviceIDShouldReturnDeviceID()
        {
            Device d = GetValidDevice();

            string deviceID = DeviceSchemaHelper.GetDeviceID(d);

            Assert.Equal("test", deviceID);
        }

        [Fact]
        public void GetDeviceIDShouldThrowIfMissingDeviceProperties()
        {
            Device d = GetDeviceWithMissingDeviceProperties();

            Assert.Throws<DeviceRequiredPropertyNotFoundException>(() => DeviceSchemaHelper.GetDeviceID(d));
        }

        [Fact]
        public void GetDeviceIDShouldThrowIfMissingDeviceID()
        {
            Device d = GetDeviceWithMissingDeviceID();

            Assert.Throws<DeviceRequiredPropertyNotFoundException>(() => DeviceSchemaHelper.GetDeviceID(d));
        }

        #endregion

        #region CreatedTime tests

        [Fact]
        public void GetCreatedTimeShouldReturnCreatedTime()
        {
            Device d = GetValidDevice();

            var createdTime = DeviceSchemaHelper.GetCreatedTime(d);

            // Need to include RoundtripKind to get a UTC value
            var expectedTime = DateTime.Parse("2015-08-01T01:02:03.0000Z", null, DateTimeStyles.RoundtripKind);
            Assert.Equal(expectedTime, createdTime);
        }

        [Fact]
        public void GetCreatedTimeShouldThrowIfMissingDeviceProperties()
        {
            Device d = GetDeviceWithMissingDeviceProperties();

            Assert.Throws<DeviceRequiredPropertyNotFoundException>(() => DeviceSchemaHelper.GetCreatedTime(d));
        }

        [Fact]
        public void GetCreatedTimeShouldThrowIfMissingCreatedTime()
        {
            Device d = GetDeviceWithMissingCreatedTime();

            Assert.Throws<DeviceRequiredPropertyNotFoundException>(() => DeviceSchemaHelper.GetCreatedTime(d));
        }

        #endregion

        #region GetUpdatedTime tests

        [Fact]
        public void GetUpdatedTimeShouldReturnUpdatedTime()
        {
            Device d = GetValidDevice();

            var updatedTime = DeviceSchemaHelper.GetUpdatedTime(d);

            // Need to include RoundtripKind to get a UTC value
            var expectedTime = DateTime.Parse("2015-09-01T01:02:03.0000Z", null, DateTimeStyles.RoundtripKind);
            Assert.Equal(expectedTime, updatedTime);
        }

        [Fact]
        public void GetUpdatedTimeShouldThrowIfMissingDeviceProperties()
        {
            Device d = GetDeviceWithMissingDeviceProperties();

            Assert.Throws<DeviceRequiredPropertyNotFoundException>(() => DeviceSchemaHelper.GetUpdatedTime(d));
        }


        [Fact]
        public void GetUpdatedTimeShouldReturnNullButNotThrowIfMissingUpdatedTime()
        {
            Device d = GetDeviceWithMissingUpdatedTime();

            var updatedTime = DeviceSchemaHelper.GetUpdatedTime(d);

            Assert.Equal(null, updatedTime);
        }

        #endregion

        #region HubEnabledState tests

        [Fact]
        public void GetHubEnabledStateShouldReturnState()
        {
            var d = GetValidDevice();

            var hubEnabledState = DeviceSchemaHelper.GetHubEnabledState(d);

            Assert.Equal(true, hubEnabledState);
        }

        [Fact]
        public void GetHubEnabledStateShouldThrowIfMissingDeviceProperties()
        {
            var d = GetDeviceWithMissingDeviceProperties();

            Assert.Throws<DeviceRequiredPropertyNotFoundException>(() => DeviceSchemaHelper.GetHubEnabledState(d));
        }

        [Fact]
        public void GetHubEnabledStateShouldReturnNullButNotThrowIfMissingState()
        {
            var d = GetDeviceWithMissingHubEnabledState();

            var hubEnabledState = DeviceSchemaHelper.GetHubEnabledState(d);

            Assert.Equal(null, hubEnabledState);
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
