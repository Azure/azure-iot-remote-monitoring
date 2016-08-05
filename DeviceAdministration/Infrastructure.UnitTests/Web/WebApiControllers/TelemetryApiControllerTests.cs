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

namespace Microsoft.Azure.Devices.Applications.RemoteMonitoring.DeviceAdmin.Infrastructure.UnitTests.Web.WebApiControllers
{
    public class TelemetryApiControllerTests
    {
        private readonly TelemetryApiController telemetryApiController;
        private readonly Mock<IDeviceTelemetryLogic> telemetryLogic;
        private readonly Mock<IAlertsLogic> alertLogic;
        private readonly Mock<IDeviceLogic> deviceLogic;
        private readonly Mock<IConfigurationProvider> configProvider;
        private readonly IFixture fixture;

        public TelemetryApiControllerTests()
        {
            this.telemetryLogic = new Mock<IDeviceTelemetryLogic>();
            this.alertLogic = new Mock<IAlertsLogic>();
            this.deviceLogic = new Mock<IDeviceLogic>();
            this.configProvider = new Mock<IConfigurationProvider>();
            this.telemetryApiController = new TelemetryApiController(this.telemetryLogic.Object,
                                                                     this.alertLogic.Object,
                                                                     this.deviceLogic.Object,
                                                                     this.configProvider.Object);
            this.telemetryApiController.InitializeRequest();
            this.fixture = new Fixture();
        }

        [Fact]
        public async void GetDashboardDevicePaneDataAsyncTest()
        {
            var deviceID = this.fixture.Create<string>();
            var device = this.fixture.Create<DeviceModel>();
            var telemetryFields = this.fixture.Create<IList<DeviceTelemetryFieldModel>>();
            var telemetryModel = this.fixture.Create<IEnumerable<DeviceTelemetryModel>>();
            var summModel = this.fixture.Create<DeviceTelemetrySummaryModel>();
            this.deviceLogic.Setup(mock => mock.GetDeviceAsync(deviceID)).ReturnsAsync(device);
            this.deviceLogic.Setup(mock => mock.ExtractTelemetry(device)).Returns(telemetryFields);

            this.telemetryLogic.Setup(mock => mock.LoadLatestDeviceTelemetrySummaryAsync(deviceID, It.IsAny<DateTime>())).ReturnsAsync(summModel);
            this.telemetryLogic.Setup(mock => mock.LoadLatestDeviceTelemetryAsync(deviceID, telemetryFields, It.IsAny<DateTime>()))
                .ReturnsAsync(telemetryModel);

            var res = await this.telemetryApiController.GetDashboardDevicePaneDataAsync(deviceID);
            res.AssertOnError();
            var data = res.ExtractContentAs<DashboardDevicePaneDataModel>();
            Assert.Equal(data.DeviceTelemetryFields, telemetryFields.ToArray());
            Assert.Equal(data.DeviceTelemetrySummaryModel, summModel);
            Assert.Equal(data.DeviceTelemetryModels, telemetryModel.OrderBy(t => t.Timestamp).ToArray());

            this.deviceLogic.Setup(mock => mock.ExtractTelemetry(device)).Throws(new Exception());
            await Assert.ThrowsAsync<HttpResponseException>(() => this.telemetryApiController.GetDashboardDevicePaneDataAsync(deviceID));
        }

        [Fact]
        public async void GetDeviceTelemetryAsyncTest()
        {
            var deviceId = this.fixture.Create<string>();
            var minTime = this.fixture.Create<DateTime>();
            var device = this.fixture.Create<DeviceModel>();
            var telemetryModel = this.fixture.Create<List<DeviceTelemetryModel>>();
            this.fixture.Create<IEnumerable<DeviceTelemetryModel>>();
            var telemetryFields = this.fixture.Create<IList<DeviceTelemetryFieldModel>>();

            this.deviceLogic.Setup(mock => mock.GetDeviceAsync(deviceId)).ReturnsAsync(device);
            this.deviceLogic.Setup(mock => mock.ExtractTelemetry(device)).Returns(telemetryFields);
            this.telemetryLogic.Setup(mock => mock.LoadLatestDeviceTelemetryAsync(deviceId, telemetryFields, minTime)).ReturnsAsync(telemetryModel);

            var res = await this.telemetryApiController.GetDeviceTelemetryAsync(deviceId, minTime);
            res.AssertOnError();
            var data = res.ExtractContentAs<DeviceTelemetryModel[]>();
            Assert.Equal(data, telemetryModel.OrderBy(t => t.Timestamp).ToArray());

            this.deviceLogic.Setup(mock => mock.ExtractTelemetry(device)).Throws(new Exception());
            await Assert.ThrowsAsync<HttpResponseException>(() => this.telemetryApiController.GetDeviceTelemetryAsync(deviceId, minTime));
            res.AssertOnError();
        }

        [Fact]
        public async void GetDeviceTelemetrySummaryAsyncTest()
        {
            var telemetrySummary = this.fixture.Create<DeviceTelemetrySummaryModel>();
            var deviceId = this.fixture.Create<string>();
            this.telemetryLogic.Setup(mock => mock.LoadLatestDeviceTelemetrySummaryAsync(deviceId, null)).ReturnsAsync(telemetrySummary);
            var res = await this.telemetryApiController.GetDeviceTelemetrySummaryAsync(deviceId);
            res.AssertOnError();
            var data = res.ExtractContentAs<DeviceTelemetrySummaryModel>();
            Assert.Equal(data, telemetrySummary);
        }

        [Fact]
        public async void GetLatestAlertHistoryAsyncTest()
        {
            var result = this.fixture.Create<DeviceListQueryResult>();
            var locationsModel = this.fixture.Create<DeviceListLocationsModel>();
            var itemModels = this.fixture.Create<IEnumerable<AlertHistoryItemModel>>();
            Func<string, DateTime?> func = (a) => null;

            this.alertLogic.Setup(mock => mock.LoadLatestAlertHistoryAsync(It.IsAny<DateTime>(), It.IsAny<int>())).ReturnsAsync(itemModels);
            this.deviceLogic.Setup(mock => mock.GetDevices(It.IsAny<DeviceListQuery>())).ReturnsAsync(result);
            this.deviceLogic.Setup(mock => mock.ExtractLocationsData(It.IsAny<List<DeviceModel>>())).Returns(locationsModel);
            this.telemetryLogic.Setup(mock => mock.ProduceGetLatestDeviceAlertTime(It.IsAny<IEnumerable<AlertHistoryItemModel>>()))
                .Returns(func);

            var res = await this.telemetryApiController.GetLatestAlertHistoryAsync();
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
            Assert.Equal(data.Devices.Select(d => d.Status == null).Count(), itemModels.Count());

            func = (a) => (DateTime.Now - TimeSpan.FromMinutes(5.0));
            this.telemetryLogic.Setup(mock => mock.ProduceGetLatestDeviceAlertTime(It.IsAny<IEnumerable<AlertHistoryItemModel>>()))
                .Returns(func);

            res = await this.telemetryApiController.GetLatestAlertHistoryAsync();
            res.AssertOnError();
            data = res.ExtractContentAs<AlertHistoryResultsModel>();
            Assert.Equal(data.Devices.Select(d => d.Status == AlertHistoryDeviceStatus.Critical).Count(), itemModels.Count());
            
            func = (a) => (DateTime.Now - TimeSpan.FromMinutes(15.0));
            this.telemetryLogic.Setup(mock => mock.ProduceGetLatestDeviceAlertTime(It.IsAny<IEnumerable<AlertHistoryItemModel>>()))
                .Returns(func);

            res = await this.telemetryApiController.GetLatestAlertHistoryAsync();
            res.AssertOnError();
            data = res.ExtractContentAs<AlertHistoryResultsModel>();
            Assert.Equal(data.Devices.Select(d => d.Status == AlertHistoryDeviceStatus.Caution).Count(), itemModels.Count());
        }

        [Fact]
        public async void GetDeviceLocationDataTest()
        {
            var queryRes = this.fixture.Create<DeviceListQueryResult>();
            var locations = this.fixture.Create<DeviceListLocationsModel>();
            this.deviceLogic.Setup(mock => mock.GetDevices(It.IsAny<DeviceListQuery>())).ReturnsAsync(queryRes);
            this.deviceLogic.Setup(mock => mock.ExtractLocationsData(queryRes.Results)).Returns(locations);
            var res = await this.telemetryApiController.GetDeviceLocationData();
            res.AssertOnError();
            var data = res.ExtractContentAs<DeviceListLocationsModel>();
            Assert.Equal(data, locations);
        }

        [Fact]
        public async void GetMapApiKeyTest()
        {
            var key = this.fixture.Create<string>();
            this.configProvider.Setup(mock => mock.GetConfigurationSettingValue("MapApiQueryKey")).Returns(key);
            var res = await this.telemetryApiController.GetMapApiKey();
            res.AssertOnError();
            var data = res.ExtractContentAs<string>();
            Assert.Equal(data, key);

            this.configProvider.Setup(mock => mock.GetConfigurationSettingValue("MapApiQueryKey")).Returns("0");
            res = await this.telemetryApiController.GetMapApiKey();
            res.AssertOnError();
            data = res.ExtractContentAs<string>();
            Assert.Equal(data, string.Empty);
        }
    }
}
