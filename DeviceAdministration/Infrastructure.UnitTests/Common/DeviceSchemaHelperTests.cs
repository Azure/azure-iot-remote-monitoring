using System;
using System.Globalization;
using Microsoft.Azure.Devices.Applications.RemoteMonitoring.Common.DeviceSchema;
using Microsoft.Azure.Devices.Applications.RemoteMonitoring.Common.Exceptions;
using Microsoft.Azure.Devices.Applications.RemoteMonitoring.Common.Models;
using Newtonsoft.Json;
using Xunit;

namespace Microsoft.Azure.Devices.Applications.RemoteMonitoring.DeviceAdmin.Infrastructure.UnitTests
{
    public class DeviceSchemaHelperTests
    {
        private DeviceModel GetValidDevice()
        {
            var d = @"{ ""DeviceProperties"": 
                            { 
                                ""DeviceID"": ""test"", 
                                ""CreatedTime"": ""2015-08-01T01:02:03.0000Z"",
                                ""UpdatedTime"": ""2015-09-01T01:02:03.0000Z"",
                                ""HubEnabledState"": true
                            },
                           ""IoTHub"":
                            {
                                ""MessageId"": ""messageId"",
                                ""CorrelationId"": ""CorrelationId"",
                                ""ConnectionDeviceId"": ""ConnectionDeviceId"",
                                ""ConnectionDeviceGenerationId"": ""ConnectionDeviceGenerationId"",
                                ""EnquedTime"": ""2015-08-01T01:02:03.0000Z"",
                                ""StreamId"": ""StreamId"",
                            }
                        }";

            return ParseDeviceFromJson(d);
        }

        private DeviceModel GetDeviceWithIotHub()
        {
            var d = @"{     ""DeviceProperties"": 
                            { 
                                ""DeviceID"": ""test"", 
                                ""CreatedTime"": ""2015-08-01T01:02:03.0000Z"",
                                ""UpdatedTime"": ""2015-09-01T01:02:03.0000Z"",
                                ""HubEnabledState"": true
                            },
                            ""IoTHub"":
                            {
                                ""MessageId"": ""messageId"",
                                ""CorrelationId"": ""CorrelationId"",
                                ""ConnectionDeviceId"": ""ConnectionDeviceId"",
                                ""ConnectionDeviceGenerationId"": ""ConnectionDeviceGenerationId"",
                                ""EnquedTime"": ""2015-08-01T01:02:03.0000Z"",
                                ""StreamId"": ""StreamId"",
                            }
                        }";
            return ParseDeviceFromJson(d);
        }

        private DeviceModel GetDeviceWithMissingDeviceProperties()
        {
            var d = @"{ ""DeviceXXXProperties"": { ""DeviceID"": ""test"" } }";

            return ParseDeviceFromJson(d);
        }

        private DeviceModel GetDeviceWithMissingDeviceID()
        {
            var d = @"{ ""DeviceProperties"": { ""DeviXXXceID"": ""test"" } }";

            return ParseDeviceFromJson(d);
        }

        private DeviceModel GetDeviceWithMissingCreatedTime()
        {
            var d = @"{ ""DeviceProperties"": 
                            { 
                                ""DeviceID"": ""test"", 
                                ""CreatXXXedTime"": ""2015-08-01T01:02:03.0000Z"" 
                            }
                        }";

            return ParseDeviceFromJson(d);
        }

        private DeviceModel GetDeviceWithMissingUpdatedTime()
        {
            var d = @"{ ""DeviceProperties"": 
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
            var d = @"{ ""DeviceProperties"": 
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
            return JsonConvert.DeserializeObject<DeviceModel>(deviceAsJson);
        }

        [Fact]
        public void GetConnectionDeviceIdTest()
        {
            var d = GetValidDevice();
            var props = DeviceSchemaHelper.GetIoTHubProperties(d);
            var connectionId = props.ConnectionDeviceId;

            Assert.Equal("ConnectionDeviceId", connectionId);
        }

        [Fact]
        public void GetCreatedTimeShouldReturnCreatedTime()
        {
            var d = GetValidDevice();

            var createdTime = DeviceSchemaHelper.GetCreatedTime(d);

            // Need to include RoundtripKind to get a UTC value
            var expectedTime = DateTime.Parse("2015-08-01T01:02:03.0000Z", null, DateTimeStyles.RoundtripKind);
            Assert.Equal(expectedTime, createdTime);
        }

        [Fact]
        public void GetCreatedTimeShouldThrowIfMissingCreatedTime()
        {
            var d = GetDeviceWithMissingCreatedTime();

            Assert.Throws<DeviceRequiredPropertyNotFoundException>(() => DeviceSchemaHelper.GetCreatedTime(d));
        }

        [Fact]
        public void GetCreatedTimeShouldThrowIfMissingDeviceProperties()
        {
            var d = GetDeviceWithMissingDeviceProperties();

            Assert.Throws<DeviceRequiredPropertyNotFoundException>(() => DeviceSchemaHelper.GetCreatedTime(d));
        }

        [Fact]
        public void GetDeviceIDShouldReturnDeviceID()
        {
            var d = GetValidDevice();

            var deviceID = DeviceSchemaHelper.GetDeviceID(d);

            Assert.Equal("test", deviceID);
        }

        [Fact]
        public void GetDeviceIDShouldThrowIfMissingDeviceID()
        {
            var d = GetDeviceWithMissingDeviceID();

            Assert.Throws<DeviceRequiredPropertyNotFoundException>(() => DeviceSchemaHelper.GetDeviceID(d));
        }

        [Fact]
        public void GetDeviceIDShouldThrowIfMissingDeviceProperties()
        {
            var d = GetDeviceWithMissingDeviceProperties();

            Assert.Throws<DeviceRequiredPropertyNotFoundException>(() => DeviceSchemaHelper.GetDeviceID(d));
        }

        [Fact]
        public void GetDevicePropertiesShouldReturnDeviceProperties()
        {
            var d = GetValidDevice();

            var props = DeviceSchemaHelper.GetDeviceProperties(d);

            Assert.NotNull(props);
            Assert.Equal("test", props.DeviceID);
        }

        [Fact]
        public void GetDevicePropertiesShouldThrowIfMissingDeviceProperties()
        {
            var d = GetDeviceWithMissingDeviceProperties();

            Assert.Throws<DeviceRequiredPropertyNotFoundException>(() => DeviceSchemaHelper.GetDeviceProperties(d));
        }

        [Fact]
        public void GetHubEnabledStateShouldReturnNullButNotThrowIfMissingState()
        {
            var d = GetDeviceWithMissingHubEnabledState();

            var hubEnabledState = DeviceSchemaHelper.GetHubEnabledState(d);

            Assert.Equal(null, hubEnabledState);
        }

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
        public void GetIotHubPropertiesTest()
        {
            var d = GetDeviceWithIotHub();
            var props = DeviceSchemaHelper.GetIoTHubProperties(d);
            var messageId = props.MessageId;

            Assert.Equal("messageId", messageId);
        }


        [Fact]
        public void GetUpdatedTimeShouldReturnNullButNotThrowIfMissingUpdatedTime()
        {
            var d = GetDeviceWithMissingUpdatedTime();

            var updatedTime = DeviceSchemaHelper.GetUpdatedTime(d);

            Assert.Equal(null, updatedTime);
        }

        [Fact]
        public void GetUpdatedTimeShouldReturnUpdatedTime()
        {
            var d = GetValidDevice();

            var updatedTime = DeviceSchemaHelper.GetUpdatedTime(d);

            // Need to include RoundtripKind to get a UTC value
            var expectedTime = DateTime.Parse("2015-09-01T01:02:03.0000Z", null, DateTimeStyles.RoundtripKind);
            Assert.Equal(expectedTime, updatedTime);
        }

        [Fact]
        public void GetUpdatedTimeShouldThrowIfMissingDeviceProperties()
        {
            var d = GetDeviceWithMissingDeviceProperties();

            Assert.Throws<DeviceRequiredPropertyNotFoundException>(() => DeviceSchemaHelper.GetUpdatedTime(d));
        }

        [Fact]
        public void IotHubPropertiesTestShouldThrowMissing()
        {
            var d = GetDeviceWithMissingDeviceProperties();
            Assert.Throws<DeviceRequiredPropertyNotFoundException>(() => DeviceSchemaHelper.GetIoTHubProperties(d));
        }

        [Fact]
        public void TestInitializeDeviceProps()
        {
            var d = new DeviceModel();
            DeviceSchemaHelper.InitializeDeviceProperties(d, "test", true);
            var props = d.DeviceProperties;
            Assert.Equal(props.DeviceID, "test");
            Assert.Equal(props.DeviceState, "normal");
        }

        [Fact]
        public void TestInitializeSystemProps()
        {
            var d = new DeviceModel();
            DeviceSchemaHelper.InitializeSystemProperties(d, "iccid");
            var props = d.SystemProperties;
            Assert.Equal(props.ICCID, "iccid");
        }

        [Fact]
        public void TestRemoveSystemProps()
        {
            var d = new DeviceModel();
            DeviceSchemaHelper.InitializeSystemProperties(d, "iccid");
            var props = d.SystemProperties;
            Assert.Equal(props.ICCID, "iccid");

            DeviceSchemaHelper.RemoveSystemPropertiesForSimulatedDeviceInfo(d);
            var RemovedProps = d.SystemProperties;
            Assert.Null(RemovedProps);
        }
    }
}