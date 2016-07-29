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
            DeviceModel d = GetValidDevice();

            DeviceProperties props = DeviceSchemaHelper.GetDeviceProperties(d);

            Assert.NotNull(props);
            Assert.Equal("test", props.DeviceID.ToString());
        }

        [Fact]
        public void GetDevicePropertiesShouldThrowIfMissingDeviceProperties()
        {
            DeviceModel d = GetDeviceWithMissingDeviceProperties();

            Assert.Throws<DeviceRequiredPropertyNotFoundException>(() => DeviceSchemaHelper.GetDeviceProperties(d));
        }

        #endregion

        #region DeviceID tests

        [Fact]
        public void GetDeviceIDShouldReturnDeviceID()
        {
            DeviceModel d = GetValidDevice();

            string deviceID = DeviceSchemaHelper.GetDeviceID(d);

            Assert.Equal("test", deviceID);
        }

        [Fact]
        public void GetDeviceIDShouldThrowIfMissingDeviceProperties()
        {
            DeviceModel d = GetDeviceWithMissingDeviceProperties();

            Assert.Throws<DeviceRequiredPropertyNotFoundException>(() => DeviceSchemaHelper.GetDeviceID(d));
        }

        [Fact]
        public void GetDeviceIDShouldThrowIfMissingDeviceID()
        {
            DeviceModel d = GetDeviceWithMissingDeviceID();

            Assert.Throws<DeviceRequiredPropertyNotFoundException>(() => DeviceSchemaHelper.GetDeviceID(d));
        }

        #endregion

        #region CreatedTime tests

        [Fact]
        public void GetCreatedTimeShouldReturnCreatedTime()
        {
            DeviceModel d = GetValidDevice();

            var createdTime = DeviceSchemaHelper.GetCreatedTime(d);

            // Need to include RoundtripKind to get a UTC value
            var expectedTime = DateTime.Parse("2015-08-01T01:02:03.0000Z", null, DateTimeStyles.RoundtripKind);
            Assert.Equal(expectedTime, createdTime);
        }

        [Fact]
        public void GetCreatedTimeShouldThrowIfMissingDeviceProperties()
        {
            DeviceModel d = GetDeviceWithMissingDeviceProperties();

            Assert.Throws<DeviceRequiredPropertyNotFoundException>(() => DeviceSchemaHelper.GetCreatedTime(d));
        }

        [Fact]
        public void GetCreatedTimeShouldThrowIfMissingCreatedTime()
        {
            DeviceModel d = GetDeviceWithMissingCreatedTime();

            Assert.Throws<DeviceRequiredPropertyNotFoundException>(() => DeviceSchemaHelper.GetCreatedTime(d));
        }

        #endregion

        #region GetUpdatedTime tests

        [Fact]
        public void GetUpdatedTimeShouldReturnUpdatedTime()
        {
            DeviceModel d = GetValidDevice();

            var updatedTime = DeviceSchemaHelper.GetUpdatedTime(d);

            // Need to include RoundtripKind to get a UTC value
            var expectedTime = DateTime.Parse("2015-09-01T01:02:03.0000Z", null, DateTimeStyles.RoundtripKind);
            Assert.Equal(expectedTime, updatedTime);
        }

        [Fact]
        public void GetUpdatedTimeShouldThrowIfMissingDeviceProperties()
        {
            DeviceModel d = GetDeviceWithMissingDeviceProperties();

            Assert.Throws<DeviceRequiredPropertyNotFoundException>(() => DeviceSchemaHelper.GetUpdatedTime(d));
        }


        [Fact]
        public void GetUpdatedTimeShouldReturnNullButNotThrowIfMissingUpdatedTime()
        {
            DeviceModel d = GetDeviceWithMissingUpdatedTime();

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

        private DeviceModel GetValidDevice()
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

        private DeviceModel GetDeviceWithMissingDeviceProperties()
        {
            string d = @"{ ""DeviceXXXProperties"": { ""DeviceID"": ""test"" } }";

            return ParseDeviceFromJson(d);
        }

        private DeviceModel GetDeviceWithMissingDeviceID()
        {
            string d = @"{ ""DeviceProperties"": { ""DeviXXXceID"": ""test"" } }";

            return ParseDeviceFromJson(d);
        }

        private DeviceModel GetDeviceWithMissingCreatedTime()
        {
            string d = @"{ ""DeviceProperties"": 
                            { 
                                ""DeviceID"": ""test"", 
                                ""CreatXXXedTime"": ""2015-08-01T01:02:03.0000Z"" 
                            }
                        }";

            return ParseDeviceFromJson(d);
        }

        private DeviceModel GetDeviceWithMissingUpdatedTime()
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

        private DeviceModel GetDeviceWithMissingHubEnabledState()
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

        private DeviceModel ParseDeviceFromJson(string deviceAsJson)
        {
            return Newtonsoft.Json.JsonConvert.DeserializeObject<DeviceModel>(deviceAsJson);
        }
    }
}
