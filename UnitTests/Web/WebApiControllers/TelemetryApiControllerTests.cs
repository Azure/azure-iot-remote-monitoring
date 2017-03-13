using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Http;
using Microsoft.Azure.Devices.Applications.RemoteMonitoring.Common.Configurations;
using Microsoft.Azure.Devices.Applications.RemoteMonitoring.Common.Models;
using Microsoft.Azure.Devices.Applications.RemoteMonitoring.DeviceAdmin.Infrastructure.BusinessLogic;
using Microsoft.Azure.Devices.Applications.RemoteMonitoring.DeviceAdmin.Infrastructure.Models;
using Microsoft.Azure.Devices.Applications.RemoteMonitoring.DeviceAdmin.Web.Models;
using Microsoft.Azure.Devices.Applications.RemoteMonitoring.DeviceAdmin.Web.WebApiControllers;
using Moq;
using Ploeh.AutoFixture;
using Xunit;

namespace Microsoft.Azure.Devices.Applications.RemoteMonitoring.UnitTests.Web.WebApiControllers
{
    public class TelemetryApiControllerTests : IDisposable
    {
        private readonly Mock<IAlertsLogic> alertLogic;
        private readonly Mock<IConfigurationProvider> configProvider;
        private readonly Mock<IDeviceLogic> deviceLogic;
        private readonly IFixture fixture;
        private readonly TelemetryApiController telemetryApiController;
        private readonly Mock<IDeviceTelemetryLogic> telemetryLogic;

        public TelemetryApiControllerTests()
        {
            telemetryLogic = new Mock<IDeviceTelemetryLogic>();
            alertLogic = new Mock<IAlertsLogic>();
            deviceLogic = new Mock<IDeviceLogic>();
            configProvider = new Mock<IConfigurationProvider>();
            telemetryApiController = new TelemetryApiController(telemetryLogic.Object,
                alertLogic.Object,
                deviceLogic.Object,
                configProvider.Object);
            telemetryApiController.InitializeRequest();
            fixture = new Fixture();
        }

        [Fact]
        public async void GetDashboardDevicePaneDataAsyncTest()
        {
            var deviceID = fixture.Create<string>();
            var device = fixture.Create<DeviceModel>();
            var telemetryFields = fixture.Create<IList<DeviceTelemetryFieldModel>>();
            var telemetryModel = fixture.Create<IEnumerable<DeviceTelemetryModel>>();
            var summModel = fixture.Create<DeviceTelemetrySummaryModel>();
            deviceLogic.Setup(mock => mock.GetDeviceAsync(deviceID)).ReturnsAsync(device);
            deviceLogic.Setup(mock => mock.ExtractTelemetry(device)).Returns(telemetryFields);

            telemetryLogic.Setup(mock => mock.LoadLatestDeviceTelemetrySummaryAsync(deviceID, It.IsAny<DateTime>()))
                .ReturnsAsync(summModel);
            telemetryLogic.Setup(
                mock => mock.LoadLatestDeviceTelemetryAsync(deviceID, telemetryFields, It.IsAny<DateTime>()))
                .ReturnsAsync(telemetryModel);

            var res = await telemetryApiController.GetDashboardDevicePaneDataAsync(deviceID);
            res.AssertOnError();
            var data = res.ExtractContentAs<DashboardDevicePaneDataModel>();
            Assert.Equal(data.DeviceTelemetryFields, telemetryFields.ToArray());
            Assert.Equal(data.DeviceTelemetrySummaryModel, summModel);
            Assert.Equal(data.DeviceTelemetryModels, telemetryModel.OrderBy(t => t.Timestamp).ToArray());

            deviceLogic.Setup(mock => mock.ExtractTelemetry(device)).Throws(new Exception());
            await
                Assert.ThrowsAsync<HttpResponseException>(
                    () => telemetryApiController.GetDashboardDevicePaneDataAsync(deviceID));
        }

        [Fact]
        public async void GetDeviceTelemetryAsyncTest()
        {
            var deviceId = fixture.Create<string>();
            var minTime = fixture.Create<DateTime>();
            var device = fixture.Create<DeviceModel>();
            var telemetryModel = fixture.Create<List<DeviceTelemetryModel>>();
            fixture.Create<IEnumerable<DeviceTelemetryModel>>();
            var telemetryFields = fixture.Create<IList<DeviceTelemetryFieldModel>>();

            deviceLogic.Setup(mock => mock.GetDeviceAsync(deviceId)).ReturnsAsync(device);
            deviceLogic.Setup(mock => mock.ExtractTelemetry(device)).Returns(telemetryFields);
            telemetryLogic.Setup(mock => mock.LoadLatestDeviceTelemetryAsync(deviceId, telemetryFields, minTime))
                .ReturnsAsync(telemetryModel);

            var res = await telemetryApiController.GetDeviceTelemetryAsync(deviceId, minTime);
            res.AssertOnError();
            var data = res.ExtractContentAs<DeviceTelemetryModel[]>();
            Assert.Equal(data, telemetryModel.OrderBy(t => t.Timestamp).ToArray());

            deviceLogic.Setup(mock => mock.ExtractTelemetry(device)).Throws(new Exception());
            await
                Assert.ThrowsAsync<HttpResponseException>(
                    () => telemetryApiController.GetDeviceTelemetryAsync(deviceId, minTime));
            res.AssertOnError();
        }

        [Fact]
        public async void GetDeviceTelemetrySummaryAsyncTest()
        {
            var telemetrySummary = fixture.Create<DeviceTelemetrySummaryModel>();
            var deviceId = fixture.Create<string>();
            telemetryLogic.Setup(mock => mock.LoadLatestDeviceTelemetrySummaryAsync(deviceId, null))
                .ReturnsAsync(telemetrySummary);
            var res = await telemetryApiController.GetDeviceTelemetrySummaryAsync(deviceId);
            res.AssertOnError();
            var data = res.ExtractContentAs<DeviceTelemetrySummaryModel>();
            Assert.Equal(data, telemetrySummary);
        }

        [Fact]
        public async void GetLatestAlertHistoryAsyncTest()
        {
            var result = fixture.Create<DeviceListFilterResult>();
            var locationsModel = fixture.Create<DeviceListLocationsModel>();
            var itemModels = fixture.Create<IEnumerable<AlertHistoryItemModel>>();
            Func<string, DateTime?> func = a => null;

            alertLogic.Setup(mock => mock.LoadLatestAlertHistoryAsync(It.IsAny<DateTime>(), It.IsAny<int>()))
                .ReturnsAsync(itemModels);
            deviceLogic.Setup(mock => mock.GetDevices(It.IsAny<DeviceListFilter>())).ReturnsAsync(result);
            deviceLogic.Setup(mock => mock.ExtractLocationsData(It.IsAny<List<DeviceModel>>())).Returns(locationsModel);
            telemetryLogic.Setup(
                mock => mock.ProduceGetLatestDeviceAlertTime(It.IsAny<IEnumerable<AlertHistoryItemModel>>()))
                .Returns(func);

            var res = await telemetryApiController.GetLatestAlertHistoryAsync();
            res.AssertOnError();
            var data = res.ExtractContentAs<AlertHistoryResultsModel>();
            if (itemModels.Count() < 18)
                Assert.Equal(data.Data.Count, itemModels.Count());
            else
            {
                Assert.Equal(data.Data.Count, 18);
            }
            Assert.Equal(data.Devices.Count, itemModels.Count());
            Assert.Equal(data.TotalAlertCount, itemModels.Count());
            Assert.Equal(data.TotalFilteredCount, itemModels.Count());
            Assert.Equal(data.Devices.Select(d => d.Status == AlertHistoryDeviceStatus.AllClear).Count(), itemModels.Count());

            func = a => (DateTime.Now - TimeSpan.FromMinutes(5.0));
            telemetryLogic.Setup(
                mock => mock.ProduceGetLatestDeviceAlertTime(It.IsAny<IEnumerable<AlertHistoryItemModel>>()))
                .Returns(func);

            res = await telemetryApiController.GetLatestAlertHistoryAsync();
            res.AssertOnError();
            data = res.ExtractContentAs<AlertHistoryResultsModel>();
            Assert.Equal(data.Devices.Select(d => d.Status == AlertHistoryDeviceStatus.Critical).Count(),
                itemModels.Count());

            func = a => (DateTime.Now - TimeSpan.FromMinutes(15.0));
            telemetryLogic.Setup(
                mock => mock.ProduceGetLatestDeviceAlertTime(It.IsAny<IEnumerable<AlertHistoryItemModel>>()))
                .Returns(func);

            res = await telemetryApiController.GetLatestAlertHistoryAsync();
            res.AssertOnError();
            data = res.ExtractContentAs<AlertHistoryResultsModel>();
            Assert.Equal(data.Devices.Select(d => d.Status == AlertHistoryDeviceStatus.Caution).Count(),
                itemModels.Count());
        }

        [Fact]
        public async void GetDeviceLocationDataTest()
        {
            var filterMock = fixture.Create<DeviceListFilterResult>();
            var locations = fixture.Create<DeviceListLocationsModel>();
            deviceLogic.Setup(mock => mock.GetDevices(It.IsAny<DeviceListFilter>())).ReturnsAsync(filterMock);
            deviceLogic.Setup(mock => mock.ExtractLocationsData(filterMock.Results)).Returns(locations);
            var res = await telemetryApiController.GetDeviceLocationData();
            res.AssertOnError();
            var data = res.ExtractContentAs<DeviceListLocationsModel>();
            Assert.Equal(data, locations);
        }

        [Fact]
        public async void GetMapApiKeyTest()
        {
            var key = fixture.Create<string>();
            configProvider.Setup(mock => mock.GetConfigurationSettingValue("MapApiQueryKey")).Returns(key);
            var res = await telemetryApiController.GetMapApiKey();
            res.AssertOnError();
            var data = res.ExtractContentAs<string>();
            Assert.Equal(data, key);

            configProvider.Setup(mock => mock.GetConfigurationSettingValue("MapApiQueryKey")).Returns("0");
            res = await telemetryApiController.GetMapApiKey();
            res.AssertOnError();
            data = res.ExtractContentAs<string>();
            Assert.Equal(data, string.Empty);
        }

        #region IDisposable Support
        private bool disposedValue = false; // To detect redundant calls

        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                    telemetryApiController.Dispose();
                }

                // TODO: free unmanaged resources (unmanaged objects) and override a finalizer below.
                // TODO: set large fields to null.

                disposedValue = true;
            }
        }

        // TODO: override a finalizer only if Dispose(bool disposing) above has code to free unmanaged resources.
        // ~TelemetryApiControllerTests() {
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