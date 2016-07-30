using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web.Mvc;
using DeviceManagement.Infrustructure.Connectivity.Exceptions;
using DeviceManagement.Infrustructure.Connectivity.Models.TerminalDevice;
using DeviceManagement.Infrustructure.Connectivity.Services;
using Microsoft.Azure.Devices.Applications.RemoteMonitoring.Common.Configurations;
using Microsoft.Azure.Devices.Applications.RemoteMonitoring.Common.Models;
using Microsoft.Azure.Devices.Applications.RemoteMonitoring.DeviceAdmin.Infrastructure.BusinessLogic;
using Microsoft.Azure.Devices.Applications.RemoteMonitoring.DeviceAdmin.Infrastructure.Models;
using Microsoft.Azure.Devices.Applications.RemoteMonitoring.DeviceAdmin.Infrastructure.Repository;
using Microsoft.Azure.Devices.Applications.RemoteMonitoring.DeviceAdmin.Web.Controllers;
using Microsoft.Azure.Devices.Applications.RemoteMonitoring.DeviceAdmin.Web.Helpers;
using Microsoft.Azure.Devices.Applications.RemoteMonitoring.DeviceAdmin.Web.Models;
using Moq;
using Ploeh.AutoFixture;
using Xunit;

namespace Microsoft.Azure.Devices.Applications.RemoteMonitoring.DeviceAdmin.Infrastructure.UnitTests.Web
{
    public class DeviceControllerTests
    {
        private readonly DeviceController deviceController;
        private readonly Mock<IApiRegistrationRepository> apiRegistrationRepository;
        private readonly Mock<IExternalCellularService> cellularServiceMock;
        private readonly Mock<IDeviceLogic> deviceLogicMock;
        private readonly Mock<ICellularExtensions> cellulerExtensionsMock;
        private readonly Fixture fixture;

        public DeviceControllerTests()
        {
            this.deviceLogicMock = new Mock<IDeviceLogic>();
            IDeviceTypeLogic deviceTypeLogic = new DeviceTypeLogic(new SampleDeviceTypeRepository());
            var configProviderMock = new Mock<IConfigurationProvider>();
            this.cellularServiceMock = new Mock<IExternalCellularService>();
            this.cellulerExtensionsMock = new Mock<ICellularExtensions>();
            this.apiRegistrationRepository = new Mock<IApiRegistrationRepository>();

            configProviderMock.Setup(mock => mock.GetConfigurationSettingValue(("iotHub.HostName"))).Returns("hubName");
            this.deviceController = new DeviceController(this.deviceLogicMock.Object,
                                                         deviceTypeLogic,
                                                         configProviderMock.Object,
                                                         this.cellularServiceMock.Object,
                                                         this.apiRegistrationRepository.Object,
                                                         this.cellulerExtensionsMock.Object);

            this.fixture = new Fixture();
        }

        [Fact]
        public void IndexTest()
        {
            var result = this.deviceController.Index();
        }

        [Fact]
        public async void AddDeviceTest()
        {
            var result = await this.deviceController.AddDevice();
            var viewResult = result as ViewResult;
            var model = viewResult.Model as List<DeviceType>;
            Assert.Equal(model.Count, 2);
            Assert.True(model.First().Name == "Simulated Device" || model.First().Name == "Custom Device");
        }

        [Fact]
        public async void SelectType()
        {
            var deviceType = this.fixture.Create<DeviceType>();
            var devices = this.fixture.Create<DeviceListQueryResult>();
            var iccids = this.fixture.Create<List<string>>();

            this.apiRegistrationRepository.Setup(repo => repo.IsApiRegisteredInAzure()).Returns(true);
            this.deviceLogicMock.Setup(mock => mock.GetDevices(It.IsAny<DeviceListQuery>())).ReturnsAsync(devices);
            this.cellulerExtensionsMock.Setup(mock => mock.GetListOfAvailableIccids(this.cellularServiceMock.Object, It.IsAny<List<DeviceModel>>()))
                .Returns(iccids);

            var result = await this.deviceController.SelectType(deviceType);
            var viewResult = result as PartialViewResult;
            var model = viewResult.Model as UnregisteredDeviceModel;
            var viewBag = viewResult.ViewBag;
            Assert.Equal(model.DeviceType, deviceType);
            Assert.True(model.IsDeviceIdSystemGenerated);
            Assert.True(viewBag.CanHaveIccid);
            Assert.Equal(viewBag.AvailableIccids, iccids);

            //IsApiRegisteredInAzure returns false
            this.apiRegistrationRepository.Setup(repo => repo.IsApiRegisteredInAzure()).Returns(false);
            result = await this.deviceController.SelectType(deviceType);
            viewResult = result as PartialViewResult;
            model = viewResult.Model as UnregisteredDeviceModel;
            viewBag = viewResult.ViewBag;
            Assert.Equal(model.DeviceType, deviceType);
            Assert.True(model.IsDeviceIdSystemGenerated);
            Assert.False(viewBag.CanHaveIccid);

            //GetListOfAvailableIccids throws
            this.apiRegistrationRepository.Setup(repo => repo.IsApiRegisteredInAzure()).Returns(true);
            this.cellulerExtensionsMock.Setup(mock => mock.GetListOfAvailableIccids(this.cellularServiceMock.Object, It.IsAny<List<DeviceModel>>()))
                .Throws(new CellularConnectivityException(new Exception()));
            result = await this.deviceController.SelectType(deviceType);
            viewResult = result as PartialViewResult;
            model = viewResult.Model as UnregisteredDeviceModel;
            viewBag = viewResult.ViewBag;
            Assert.Equal(model.DeviceType, deviceType);
            Assert.True(model.IsDeviceIdSystemGenerated);
            Assert.False(viewBag.CanHaveIccid);
        }

        [Fact]
        public async void AddDeviceCreate()
        {
            var button = this.fixture.Create<string>();
            var deviceModel = this.fixture.Create<UnregisteredDeviceModel>();
            var devices = this.fixture.Create<DeviceListQueryResult>();
            var iccids = this.fixture.Create<List<string>>();

            this.apiRegistrationRepository.Setup(repo => repo.IsApiRegisteredInAzure()).Returns(true);
            this.deviceLogicMock.Setup(mock => mock.GetDevices(It.IsAny<DeviceListQuery>())).ReturnsAsync(devices);
            this.deviceLogicMock.Setup(mock => mock.GetDeviceAsync(It.IsAny<string>())).ReturnsAsync(new DeviceModel());
            var result = await this.deviceController.AddDeviceCreate(button, deviceModel);

            var viewResult = result as PartialViewResult;
            var model = viewResult.Model as UnregisteredDeviceModel;
            var viewBag = viewResult.ViewBag;
            Assert.True(viewBag.CanHaveIccid);
            Assert.Equal(model, deviceModel);
        }

        [Fact]
        public async void EditDeviceProperties()
        {
            var editModel = this.fixture.Create<EditDevicePropertiesModel>();
            this.deviceLogicMock.Setup(mock => mock.GetDeviceAsync(It.IsAny<string>())).ReturnsAsync(new DeviceModel());
            var result = await this.deviceController.EditDeviceProperties(editModel);
            Assert.NotNull(result);

            //doesn't work
            //var view = result as ViewResult;
        }

        [Fact]
        public async void EditDevicePropertiesWithDeviceId()
        {
            var deviceId = this.fixture.Create<string>();
            var deviceModel = this.fixture.Create<DeviceModel>();
            deviceModel.DeviceProperties = this.fixture.Create<DeviceProperties>();
            deviceModel.DeviceProperties.DeviceID = deviceId;
            var propModel = this.fixture.Create<IEnumerable<DevicePropertyValueModel>>();
            this.deviceLogicMock.Setup(mock => mock.GetDeviceAsync(deviceId)).ReturnsAsync(deviceModel);
            this.deviceLogicMock.Setup(mock => mock.ExtractDevicePropertyValuesModels(deviceModel)).Returns(propModel);

            var result = await this.deviceController.EditDeviceProperties(deviceId);
            var view = result as ViewResult;
            var model = view.Model as EditDevicePropertiesModel;
            Assert.NotNull(model.DevicePropertyValueModels);
            Assert.Equal(model.DevicePropertyValueModels.Count, propModel.Count());


            this.deviceLogicMock.Setup(mock => mock.GetDeviceAsync(deviceId)).ReturnsAsync(null);
            result = await this.deviceController.EditDeviceProperties(deviceId);
            view = result as ViewResult;
            model = view.Model as EditDevicePropertiesModel;
            Assert.Equal(model.DevicePropertyValueModels.Count, 0);
        }

        [Fact]
        public async void GetDeviceDetails()
        {
            var deviceId = this.fixture.Create<string>();
            var deviceModel = this.fixture.Create<DeviceModel>();
            deviceModel.DeviceProperties = this.fixture.Create<DeviceProperties>();
            deviceModel.DeviceProperties.DeviceID = deviceId;
            this.deviceLogicMock.Setup(mock => mock.GetDeviceAsync(deviceId)).ReturnsAsync(deviceModel);

            var result = await this.deviceController.GetDeviceDetails(deviceId);
            var view = result as PartialViewResult;
            var model = view.Model as DeviceDetailModel;
            Assert.Equal(model.DeviceID, deviceId);
            Assert.Equal(model.HubEnabledState, deviceModel.DeviceProperties.HubEnabledState);
            Assert.Equal(model.IsCellular, true);
            Assert.Equal(model.Iccid, deviceModel.SystemProperties.ICCID);

            this.deviceLogicMock.Setup(mock => mock.GetDeviceAsync(deviceId)).ReturnsAsync(null);
            await Assert.ThrowsAsync<InvalidOperationException>(() => this.deviceController.GetDeviceDetails(deviceId));
        }

        [Fact]
        public async void GetDeviceKeys()
        {
            var keys = this.fixture.Create<SecurityKeys>();
            var deviceId = this.fixture.Create<string>();
            this.deviceLogicMock.Setup(mock => mock.GetIoTHubKeysAsync(deviceId)).ReturnsAsync(keys);
            var keyModel = await this.deviceController.GetDeviceKeys(deviceId);
            var viewResult = keyModel as PartialViewResult;
            var model = viewResult.Model as SecurityKeysModel;
            Assert.Equal(model.PrimaryKey, keys.PrimaryKey);
            Assert.Equal(model.SecondaryKey, keys.SecondaryKey);
        }

        [Fact]
        public void GetDeviceCellularDetails()
        {
            var iccId = this.fixture.Create<string>();
            var terminalDevice = this.fixture.Create<Terminal>();
            var sessionInfo = this.fixture.Create<List<SessionInfo>>();
            this.cellularServiceMock.Setup(mock => mock.GetSingleTerminalDetails(It.IsAny<Iccid>())).Returns(terminalDevice);
            this.cellularServiceMock.Setup(mock => mock.GetSingleSessionInfo(It.IsAny<Iccid>())).Returns(sessionInfo);

            var result = this.deviceController.GetDeviceCellularDetails(iccId);
            var viewResult = result as PartialViewResult;
            var model = viewResult.Model as SimInformationViewModel;
            Assert.Equal(model.TerminalDevice, terminalDevice);
            Assert.Equal(model.SessionInfo, sessionInfo.LastOrDefault());
        }

        [Fact]
        public void RemoveDevice()
        {
            var deviceID = this.fixture.Create<string>();
            var result = this.deviceController.RemoveDevice(deviceID);
            var viewResult = result as ViewResult;
            var model = viewResult.Model as RegisteredDeviceModel;
            Assert.Equal(model.HostName, "hubName");
            Assert.Equal(model.DeviceId, deviceID);
        }

        [Fact]
        public async void DeleteDevice()
        {
            var deviceID = this.fixture.Create<string>();
            this.deviceLogicMock.Setup(mock => mock.RemoveDeviceAsync(deviceID)).Returns(Task.FromResult(true));
            await this.deviceController.DeleteDevice(deviceID);
            this.deviceLogicMock.Verify(mock => mock.RemoveDeviceAsync(deviceID), Times.Once());
        }
    }
}
