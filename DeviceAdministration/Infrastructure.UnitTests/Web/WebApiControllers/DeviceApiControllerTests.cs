using System.Collections.Generic;
using System.Threading.Tasks;
using System.Web.Http;
using Microsoft.Azure.Devices.Applications.RemoteMonitoring.Common.Models;
using Microsoft.Azure.Devices.Applications.RemoteMonitoring.DeviceAdmin.Infrastructure.BusinessLogic;
using Microsoft.Azure.Devices.Applications.RemoteMonitoring.DeviceAdmin.Infrastructure.Models;
using Microsoft.Azure.Devices.Applications.RemoteMonitoring.DeviceAdmin.Web.DataTables;
using Microsoft.Azure.Devices.Applications.RemoteMonitoring.DeviceAdmin.Web.WebApiControllers;
using Moq;
using Newtonsoft.Json.Linq;
using Ploeh.AutoFixture;
using Xunit;

namespace Microsoft.Azure.Devices.Applications.RemoteMonitoring.DeviceAdmin.Infrastructure.UnitTests.Web.WebApiControllers
{
    public class DeviceApiControllerTests
    {
        private readonly DeviceApiController deviceApiController;
        private readonly Mock<IDeviceLogic> deviceLogic;
        private readonly IFixture fixture;

        public DeviceApiControllerTests()
        {
            this.deviceLogic = new Mock<IDeviceLogic>();
            this.deviceApiController = new DeviceApiController(this.deviceLogic.Object);
            this.deviceApiController.InitializeRequest();
            this.fixture = new Fixture();
        }

        [Fact]
        public async void GenerateSampleDevicesAsyncTest()
        {
            this.deviceLogic.Setup(mock => mock.GenerateNDevices(5)).Returns(Task.FromResult(true));
            var res = await this.deviceApiController.GenerateSampleDevicesAsync(5);
            res.AssertOnError();
            var data = res.ExtractContentDataAs<bool>();
            Assert.True(data);
        }

        [Fact]
        public async void GetDeviceAsyncTest()
        {
            var deviceId = this.fixture.Create<string>();
            var device = this.fixture.Create<DeviceModel>();
            this.deviceLogic.Setup(mock => mock.GetDeviceAsync(deviceId)).ReturnsAsync(device);
            var res = await this.deviceApiController.GetDeviceAsync(deviceId);
            res.AssertOnError();
            var data = res.ExtractContentDataAs<DeviceModel>();
            Assert.Equal(data, device);
        }

        [Fact]
        public async void GetDeviceAsyncWFilterTest()
        {
            var devices = this.fixture.Create<DeviceListQueryResult>();
            this.deviceLogic.Setup(mock => mock.GetDevices(It.IsAny<DeviceListQuery>())).ReturnsAsync(devices);
            var res = await this.deviceApiController.GetDevicesAsync();
            res.AssertOnError();
            var data = res.ExtractContentDataAs<List<DeviceModel>>();
            Assert.Equal(data, devices.Results);
        }

        [Fact]
        public async void GetDevicesTest()
        {
            var reqData = this.fixture.Create<DataTablesRequest>();
            reqData.SortColumns.ForEach(col => col.ColumnIndexAsString = 0.ToString());
            var devices = this.fixture.Create<DeviceListQueryResult>();

            this.deviceLogic.Setup(mock => mock.GetDevices(It.IsAny<DeviceListQuery>())).ReturnsAsync(devices);
            var res = await this.deviceApiController.GetDevices(JObject.FromObject(reqData));

            res.AssertOnError();
            var data = res.ExtractContentAs<DataTablesResponse<DeviceModel>>();
            Assert.Equal(data.Draw, reqData.Draw);
            Assert.Equal(data.RecordsTotal, devices.TotalDeviceCount);
            Assert.Equal(data.RecordsFiltered, devices.TotalFilteredCount);
            Assert.Equal(data.Data, devices.Results.ToArray());
        }

        [Fact]
        public async void RemoveDeviceAsyncTest()
        {
            var deviceId = this.fixture.Create<string>();
            this.deviceLogic.Setup(mock => mock.RemoveDeviceAsync(deviceId)).Returns(Task.FromResult(true));
            var res = await this.deviceApiController.RemoveDeviceAsync(deviceId);
            var data = res.ExtractContentDataAs<bool>();
            Assert.True(data);
        }

        [Fact]
        public async void AddDeviceAsyncTest()
        {
            var device = this.fixture.Create<DeviceModel>();
            var deviceWKeys = this.fixture.Create<DeviceWithKeys>();

            this.deviceLogic.Setup(mock => mock.AddDeviceAsync(device)).ReturnsAsync(deviceWKeys);
            var res = await this.deviceApiController.AddDeviceAsync(device);
            res.AssertOnError();
            var data = res.ExtractContentDataAs<DeviceWithKeys>();
            Assert.Equal(data, deviceWKeys);

            await Assert.ThrowsAsync<HttpResponseException>(() => this.deviceApiController.AddDeviceAsync(null));
        }

        [Fact]
        public async void UpdateDeviceAsyncTest()
        {
            var device = this.fixture.Create<DeviceModel>();
            this.deviceLogic.Setup(mock => mock.UpdateDeviceAsync(device)).ReturnsAsync(device);
            var res = await this.deviceApiController.UpdateDeviceAsync(device);
            res.AssertOnError();
            var data = res.ExtractContentDataAs<bool>();
            Assert.True(data);

            await Assert.ThrowsAsync<HttpResponseException>(() => this.deviceApiController.UpdateDeviceAsync(null));
        }

        [Fact]
        public async void GetDeviceKeysAsyncTest()
        {
            var id = this.fixture.Create<string>();
            var keys = this.fixture.Create<SecurityKeys>();
            this.deviceLogic.Setup(mock => mock.GetIoTHubKeysAsync(id)).ReturnsAsync(keys);
            var res = await this.deviceApiController.GetDeviceKeysAsync(id);
            res.AssertOnError();
            var data = res.ExtractContentDataAs<SecurityKeys>();
            Assert.Equal(data, keys);
        }

        [Fact]
        public async void UpdateDeviceEnabledStatusTest()
        {
            var deviceId = this.fixture.Create<string>();
            var device = this.fixture.Create<DeviceModel>();
            var request = new JObject();
            request.Add("isEnabled", true);

            this.deviceLogic.Setup(mock => mock.UpdateDeviceEnabledStatusAsync(deviceId, true)).ReturnsAsync(device);
            var res = await this.deviceApiController.UpdateDeviceEnabledStatus(deviceId, request);
            res.AssertOnError();
            var data = res.ExtractContentDataAs<bool>();
            Assert.True(data);
        }

        [Fact]
        public async void SendCommand()
        {
            var deviceId = this.fixture.Create<string>();
            var commandName = this.fixture.Create<string>();
            var parameters = this.fixture.Create<IDictionary<string, string>>();

            this.deviceLogic.Setup(mock => mock.SendCommandAsync(deviceId, commandName, parameters)).Returns(Task.FromResult(true));
            var res = await this.deviceApiController.SendCommand(deviceId, commandName, parameters);
            res.AssertOnError();
            var data = res.ExtractContentDataAs<bool>();
            Assert.True(data);
        }

        [Fact]
        public async void DeleteAllDevices()
        {
            var devices = this.fixture.Create<DeviceListQueryResult>();
            DeviceListQuery saveObject = null;
            this.deviceLogic.Setup(mock => mock.GetDevices(It.IsAny<DeviceListQuery>()))
                .Callback<DeviceListQuery>(obj => saveObject = obj)
                .ReturnsAsync(devices);

            this.deviceLogic.Setup(mock => mock.RemoveDeviceAsync(It.IsAny<string>())).Returns(Task.FromResult(true));

            var res = await this.deviceApiController.DeleteAllDevices();
            Assert.Equal(saveObject.Skip, 0);
            Assert.Equal(saveObject.Take, 1000);
            Assert.Equal(saveObject.SortColumn, "DeviceID");
            res.AssertOnError();
            var data = res.ExtractContentDataAs<bool>();
            Assert.True(data);
        }
    }
}
