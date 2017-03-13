using System;
using System.Collections.Generic;
using System.Dynamic;
using System.Threading.Tasks;
using System.Web.Http;
using Microsoft.Azure.Devices.Applications.RemoteMonitoring.Common.Models;
using Microsoft.Azure.Devices.Applications.RemoteMonitoring.Common.Models.Commands;
using Microsoft.Azure.Devices.Applications.RemoteMonitoring.DeviceAdmin.Infrastructure.BusinessLogic;
using Microsoft.Azure.Devices.Applications.RemoteMonitoring.DeviceAdmin.Infrastructure.Models;
using Microsoft.Azure.Devices.Applications.RemoteMonitoring.DeviceAdmin.Infrastructure.Repository;
using Microsoft.Azure.Devices.Applications.RemoteMonitoring.DeviceAdmin.Web.DataTables;
using Microsoft.Azure.Devices.Applications.RemoteMonitoring.DeviceAdmin.Web.WebApiControllers;
using Moq;
using Newtonsoft.Json.Linq;
using Ploeh.AutoFixture;
using Xunit;

namespace Microsoft.Azure.Devices.Applications.RemoteMonitoring.UnitTests.Web.WebApiControllers
{
    public class DeviceApiControllerTests : IDisposable
    {
        private readonly DeviceApiController deviceApiController;
        private readonly Mock<IDeviceLogic> deviceLogic;
        private readonly Mock<IDeviceListFilterRepository> devicefilterRepository;
        private readonly Mock<IIoTHubDeviceManager> deviceManager;
        private readonly IFixture fixture;

        public DeviceApiControllerTests()
        {
            deviceLogic = new Mock<IDeviceLogic>();
            devicefilterRepository = new Mock<IDeviceListFilterRepository>();
            deviceManager = new Mock<IIoTHubDeviceManager>();
            deviceApiController = new DeviceApiController(deviceLogic.Object, devicefilterRepository.Object, deviceManager.Object);
            deviceApiController.InitializeRequest();
            fixture = new Fixture();
        }

        [Fact]
        public async void GenerateSampleDevicesAsyncTest()
        {
            deviceLogic.Setup(mock => mock.GenerateNDevices(5)).Returns(Task.FromResult(true));
            var res = await deviceApiController.GenerateSampleDevicesAsync(5);
            res.AssertOnError();
            var data = res.ExtractContentDataAs<bool>();
            Assert.True(data);
        }

        [Fact]
        public async void GetDeviceAsyncTest()
        {
            var deviceId = fixture.Create<string>();
            var device = fixture.Create<DeviceModel>();
            deviceLogic.Setup(mock => mock.GetDeviceAsync(deviceId)).ReturnsAsync(device);
            var res = await deviceApiController.GetDeviceAsync(deviceId);
            res.AssertOnError();
            var data = res.ExtractContentDataAs<DeviceModel>();
            Assert.Equal(data, device);
        }

        [Fact]
        public async void GetDeviceAsyncWFilterTest()
        {
            var devices = fixture.Create<DeviceListFilterResult>();
            deviceLogic.Setup(mock => mock.GetDevices(It.IsAny<DeviceListFilter>())).ReturnsAsync(devices);
            var res = await deviceApiController.GetDevicesAsync();
            res.AssertOnError();
            var data = res.ExtractContentDataAs<List<DeviceModel>>();
            Assert.Equal(data, devices.Results);
        }

        [Fact]
        public async void GetDevicesTest()
        {
            var reqData = fixture.Create<DataTablesRequest>();
            reqData.SortColumns.ForEach(col => col.ColumnIndexAsString = 0.ToString());
            var devices = fixture.Create<DeviceListFilterResult>();

            deviceLogic.Setup(mock => mock.GetDevices(It.IsAny<DeviceListFilter>())).ReturnsAsync(devices);
            var res = await deviceApiController.GetDevices(JObject.FromObject(reqData));

            res.AssertOnError();
            var data = res.ExtractContentAs<DataTablesResponse<DeviceModel>>();
            Assert.Equal(data.Draw, reqData.Draw);
            Assert.Equal(data.RecordsTotal, devices.TotalDeviceCount);
            Assert.Equal(data.RecordsFiltered, devices.TotalFilteredCount);
            Assert.Equal(data.Data, devices.Results.ToArray());
        }

        [Fact]
        public async void GetApplicableDeviceCountByMethodTest()
        {
            var filter = fixture.Create<DeviceListFilter>();

            dynamic method = new ExpandoObject();
            method.methodName = "mockname";
            method.parameters = new List<ExpandoObject>();

            deviceManager.Setup(mock => mock.GetDeviceCountAsync(It.IsAny<string>(), It.IsAny<string>()))
                .ReturnsAsync(100);
            deviceManager.Setup(mock => mock.GetDeviceCountAsync(It.Is<string>(p => p.Contains("is_defined")), It.IsAny<string>()))
                .ReturnsAsync(80);
            devicefilterRepository.Setup(mock => mock.GetFilterAsync(It.IsAny<string>()))
                .ReturnsAsync(filter);

            System.Net.Http.HttpResponseMessage res = await deviceApiController.GetApplicableDeviceCountByMethod("mockfilterId", method);

            res.AssertOnError();
            var data = res.ExtractContentDataAs<DeviceApplicableResult>();
            Assert.Equal(80, data.Applicable);
            Assert.Equal(100, data.Total);
        }

        [Fact]
        public async void RemoveDeviceAsyncTest()
        {
            var deviceId = fixture.Create<string>();
            deviceLogic.Setup(mock => mock.RemoveDeviceAsync(deviceId)).Returns(Task.FromResult(true));
            var res = await deviceApiController.RemoveDeviceAsync(deviceId);
            var data = res.ExtractContentDataAs<bool>();
            Assert.True(data);
        }

        [Fact]
        public async void AddDeviceAsyncTest()
        {
            var device = fixture.Create<DeviceModel>();
            var deviceWKeys = fixture.Create<DeviceWithKeys>();

            deviceLogic.Setup(mock => mock.AddDeviceAsync(device)).ReturnsAsync(deviceWKeys);
            var res = await deviceApiController.AddDeviceAsync(device);
            res.AssertOnError();
            var data = res.ExtractContentDataAs<DeviceWithKeys>();
            Assert.Equal(data, deviceWKeys);

            await Assert.ThrowsAsync<HttpResponseException>(() => deviceApiController.AddDeviceAsync(null));
        }

        [Fact]
        public async void UpdateDeviceAsyncTest()
        {
            var device = fixture.Create<DeviceModel>();
            deviceLogic.Setup(mock => mock.UpdateDeviceAsync(device)).ReturnsAsync(device);
            var res = await deviceApiController.UpdateDeviceAsync(device);
            res.AssertOnError();
            var data = res.ExtractContentDataAs<bool>();
            Assert.True(data);

            await Assert.ThrowsAsync<HttpResponseException>(() => deviceApiController.UpdateDeviceAsync(null));
        }

        [Fact]
        public async void GetDeviceKeysAsyncTest()
        {
            var id = fixture.Create<string>();
            var keys = fixture.Create<SecurityKeys>();
            deviceLogic.Setup(mock => mock.GetIoTHubKeysAsync(id)).ReturnsAsync(keys);
            var res = await deviceApiController.GetDeviceKeysAsync(id);
            res.AssertOnError();
            var data = res.ExtractContentDataAs<SecurityKeys>();
            Assert.Equal(data, keys);
        }

        [Fact]
        public async void UpdateDeviceEnabledStatusTest()
        {
            var deviceId = fixture.Create<string>();
            var device = fixture.Create<DeviceModel>();
            var request = new JObject();
            request.Add("isEnabled", true);

            deviceLogic.Setup(mock => mock.UpdateDeviceEnabledStatusAsync(deviceId, true)).ReturnsAsync(device);
            var res = await deviceApiController.UpdateDeviceEnabledStatus(deviceId, request);
            res.AssertOnError();
            var data = res.ExtractContentDataAs<bool>();
            Assert.True(data);
        }

        [Fact]
        public async void SendCommand()
        {
            var deviceId = fixture.Create<string>();
            var commandName = fixture.Create<string>();
            var deliveryType = fixture.Create<DeliveryType>();
            var parameters = fixture.Create<IDictionary<string, string>>();

            deviceLogic.Setup(mock => mock.SendCommandAsync(deviceId, commandName, deliveryType, parameters))
                .Returns(Task.FromResult(true));
            var res = await deviceApiController.SendCommand(deviceId, commandName, deliveryType, parameters);
            res.AssertOnError();
            var data = res.ExtractContentDataAs<bool>();
            Assert.True(data);
        }

#if DEBUG
        [Fact]
        public async void DeleteAllDevices()
        {
            var devices = fixture.Create<DeviceListFilterResult>();
            DeviceListFilter saveObject = null;
            deviceLogic.Setup(mock => mock.GetDevices(It.IsAny<DeviceListFilter>()))
                .Callback<DeviceListFilter>(obj => saveObject = obj)
                .ReturnsAsync(devices);

            deviceLogic.Setup(mock => mock.RemoveDeviceAsync(It.IsAny<string>())).Returns(Task.FromResult(true));

            var res = await deviceApiController.DeleteAllDevices();
            Assert.Equal(saveObject.Skip, 0);
            Assert.Equal(saveObject.Take, 1000);
            Assert.Equal(saveObject.SortColumn, "twin.deviceId");
            res.AssertOnError();
            var data = res.ExtractContentDataAs<bool>();
            Assert.True(data);
        }
#endif
        #region IDisposable Support
        private bool disposedValue = false; // To detect redundant calls

        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                    deviceApiController.Dispose();
                }

                // TODO: free unmanaged resources (unmanaged objects) and override a finalizer below.
                // TODO: set large fields to null.

                disposedValue = true;
            }
        }

        // TODO: override a finalizer only if Dispose(bool disposing) above has code to free unmanaged resources.
        // ~DeviceApiControllerTests() {
        //   // Do not change this code. Put cleanup code in Dispose(bool disposing) above.
        //   Dispose(false);
        // }

        // This code added to correctly implement the disposable pattern.
        public void Dispose()
        {
            // Do not change this code. Put cleanup code in Dispose(bool disposing) above.
            Dispose(true);
            // TODO: uncomment the following line if the finalizer is overridden above.
            // GC.SuppressFinalize(this);
        }
        #endregion
    }
}