using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web.Mvc;
using DeviceManagement.Infrustructure.Connectivity.Exceptions;
using DeviceManagement.Infrustructure.Connectivity.Models.TerminalDevice;
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
        private readonly Mock<IApiRegistrationRepository> apiRegistrationRepository;
        private readonly Mock<ICellularExtensions> cellulerExtensionsMock;
        private readonly DeviceController deviceController;
        private readonly Mock<IDeviceLogic> deviceLogicMock;
        private readonly Fixture fixture;

        public DeviceControllerTests()
        {
            deviceLogicMock = new Mock<IDeviceLogic>();
            IDeviceTypeLogic deviceTypeLogic = new DeviceTypeLogic(new SampleDeviceTypeRepository());
            var configProviderMock = new Mock<IConfigurationProvider>();
            cellulerExtensionsMock = new Mock<ICellularExtensions>();
            apiRegistrationRepository = new Mock<IApiRegistrationRepository>();

            configProviderMock.Setup(mock => mock.GetConfigurationSettingValue(("iotHub.HostName"))).Returns("hubName");
            deviceController = new DeviceController(deviceLogicMock.Object,
                deviceTypeLogic,
                configProviderMock.Object,
                apiRegistrationRepository.Object,
                cellulerExtensionsMock.Object);

            fixture = new Fixture();
        }

        [Fact]
        public void IndexTest()
        {
            var result = deviceController.Index();
        }

        [Fact]
        public async void AddDeviceTestTest()
        {
            var result = await deviceController.AddDevice();
            var viewResult = result as ViewResult;
            var model = viewResult.Model as List<DeviceType>;
            Assert.Equal(model.Count, 2);
            Assert.True(model.First().Name == "Simulated Device" || model.First().Name == "Custom Device");
        }

        [Fact]
        public async void SelectTypeTest()
        {
            var deviceType = fixture.Create<DeviceType>();
            var devices = fixture.Create<DeviceListQueryResult>();
            var iccids = fixture.Create<List<string>>();

            apiRegistrationRepository.Setup(repo => repo.IsApiRegisteredInAzure()).Returns(true);
            deviceLogicMock.Setup(mock => mock.GetDevices(It.IsAny<DeviceListQuery>())).ReturnsAsync(devices);
            cellulerExtensionsMock.Setup(mock => mock.GetListOfAvailableIccids(It.IsAny<List<DeviceModel>>()))
                .Returns(iccids);

            var result = await deviceController.SelectType(deviceType);
            var viewResult = result as PartialViewResult;
            var model = viewResult.Model as UnregisteredDeviceModel;
            var viewBag = viewResult.ViewBag;
            Assert.Equal(model.DeviceType, deviceType);
            Assert.True(model.IsDeviceIdSystemGenerated);
            Assert.True(viewBag.CanHaveIccid);
            Assert.Equal(viewBag.AvailableIccids, iccids);

            //IsApiRegisteredInAzure returns false
            apiRegistrationRepository.Setup(repo => repo.IsApiRegisteredInAzure()).Returns(false);
            result = await deviceController.SelectType(deviceType);
            viewResult = result as PartialViewResult;
            model = viewResult.Model as UnregisteredDeviceModel;
            viewBag = viewResult.ViewBag;
            Assert.Equal(model.DeviceType, deviceType);
            Assert.True(model.IsDeviceIdSystemGenerated);
            Assert.False(viewBag.CanHaveIccid);

            //GetListOfAvailableIccids throws
            apiRegistrationRepository.Setup(repo => repo.IsApiRegisteredInAzure()).Returns(true);
            cellulerExtensionsMock.Setup(mock => mock.GetListOfAvailableIccids(It.IsAny<List<DeviceModel>>()))
                .Throws(new CellularConnectivityException(new Exception()));
            result = await deviceController.SelectType(deviceType);
            viewResult = result as PartialViewResult;
            model = viewResult.Model as UnregisteredDeviceModel;
            viewBag = viewResult.ViewBag;
            Assert.Equal(model.DeviceType, deviceType);
            Assert.True(model.IsDeviceIdSystemGenerated);
            Assert.False(viewBag.CanHaveIccid);
        }

        [Fact]
        public async void AddDeviceCreateTest()
        {
            var button = fixture.Create<string>();
            var deviceModel = fixture.Create<UnregisteredDeviceModel>();
            var devices = fixture.Create<DeviceListQueryResult>();
            var iccids = fixture.Create<List<string>>();

            apiRegistrationRepository.Setup(repo => repo.IsApiRegisteredInAzure()).Returns(true);
            deviceLogicMock.Setup(mock => mock.GetDevices(It.IsAny<DeviceListQuery>())).ReturnsAsync(devices);
            deviceLogicMock.Setup(mock => mock.GetDeviceAsync(It.IsAny<string>())).ReturnsAsync(new DeviceModel());
            var result = await deviceController.AddDeviceCreate(button, deviceModel);

            var viewResult = result as PartialViewResult;
            var model = viewResult.Model as UnregisteredDeviceModel;
            var viewBag = viewResult.ViewBag;
            Assert.True(viewBag.CanHaveIccid);
            Assert.Equal(model, deviceModel);
        }

        [Fact]
        public async void EditDevicePropertiesTest()
        {
            var editModel = fixture.Create<EditDevicePropertiesModel>();
            deviceLogicMock.Setup(mock => mock.GetDeviceAsync(It.IsAny<string>())).ReturnsAsync(new DeviceModel());
            var result = await deviceController.EditDeviceProperties(editModel);
            Assert.NotNull(result);

            //TODO: doesn't work
            //var view = result as ViewResult;
        }

        [Fact]
        public async void EditDevicePropertiesWithDeviceIdTest()
        {
            var deviceId = fixture.Create<string>();
            var deviceModel = fixture.Create<DeviceModel>();
            deviceModel.DeviceProperties = fixture.Create<DeviceProperties>();
            deviceModel.DeviceProperties.DeviceID = deviceId;
            var propModel = fixture.Create<IEnumerable<DevicePropertyValueModel>>();
            deviceLogicMock.Setup(mock => mock.GetDeviceAsync(deviceId)).ReturnsAsync(deviceModel);
            deviceLogicMock.Setup(mock => mock.ExtractDevicePropertyValuesModels(deviceModel)).Returns(propModel);

            var result = await deviceController.EditDeviceProperties(deviceId);
            var view = result as ViewResult;
            var model = view.Model as EditDevicePropertiesModel;
            Assert.NotNull(model.DevicePropertyValueModels);
            Assert.Equal(model.DevicePropertyValueModels.Count, propModel.Count());


            deviceLogicMock.Setup(mock => mock.GetDeviceAsync(deviceId)).ReturnsAsync(null);
            result = await deviceController.EditDeviceProperties(deviceId);
            view = result as ViewResult;
            model = view.Model as EditDevicePropertiesModel;
            Assert.Equal(model.DevicePropertyValueModels.Count, 0);
        }

        [Fact]
        public async void GetDeviceDetailsTest()
        {
            var deviceId = fixture.Create<string>();
            var deviceModel = fixture.Create<DeviceModel>();
            deviceModel.DeviceProperties = fixture.Create<DeviceProperties>();
            deviceModel.DeviceProperties.DeviceID = deviceId;
            deviceLogicMock.Setup(mock => mock.GetDeviceAsync(deviceId)).ReturnsAsync(deviceModel);

            var result = await deviceController.GetDeviceDetails(deviceId);
            var view = result as PartialViewResult;
            var model = view.Model as DeviceDetailModel;
            Assert.Equal(model.DeviceID, deviceId);
            Assert.Equal(model.HubEnabledState, deviceModel.DeviceProperties.HubEnabledState);
            Assert.Equal(model.IsCellular, true);
            Assert.Equal(model.Iccid, deviceModel.SystemProperties.ICCID);

            deviceLogicMock.Setup(mock => mock.GetDeviceAsync(deviceId)).ReturnsAsync(null);
            await Assert.ThrowsAsync<InvalidOperationException>(() => deviceController.GetDeviceDetails(deviceId));
        }

        [Fact]
        public async void GetDeviceKeysTest()
        {
            var keys = fixture.Create<SecurityKeys>();
            var deviceId = fixture.Create<string>();
            deviceLogicMock.Setup(mock => mock.GetIoTHubKeysAsync(deviceId)).ReturnsAsync(keys);
            var keyModel = await deviceController.GetDeviceKeys(deviceId);
            var viewResult = keyModel as PartialViewResult;
            var model = viewResult.Model as SecurityKeysModel;
            Assert.Equal(model.PrimaryKey, keys.PrimaryKey);
            Assert.Equal(model.SecondaryKey, keys.SecondaryKey);
        }

        [Fact]
        public void GetDeviceCellularDetailsTest()
        {
            var iccId = fixture.Create<string>();
            var terminalDevice = fixture.Create<Terminal>();
            var sessionInfo = fixture.Create<List<SessionInfo>>();
            cellulerExtensionsMock.Setup(mock => mock.GetSingleTerminalDetails(It.IsAny<Iccid>()))
                .Returns(terminalDevice);
            cellulerExtensionsMock.Setup(mock => mock.GetSingleSessionInfo(It.IsAny<Iccid>())).Returns(sessionInfo);

            var result = deviceController.GetDeviceCellularDetails(iccId);
            var viewResult = result as PartialViewResult;
            var model = viewResult.Model as SimInformationViewModel;
            Assert.Equal(model.TerminalDevice, terminalDevice);
            Assert.Equal(model.SessionInfo, sessionInfo.LastOrDefault());
        }

        [Fact]
        public void RemoveDeviceTest()
        {
            var deviceID = fixture.Create<string>();
            var result = deviceController.RemoveDevice(deviceID);
            var viewResult = result as ViewResult;
            var model = viewResult.Model as RegisteredDeviceModel;
            Assert.Equal(model.HostName, "hubName");
            Assert.Equal(model.DeviceId, deviceID);
        }

        [Fact]
        public async void DeleteDeviceTest()
        {
            var deviceID = fixture.Create<string>();
            deviceLogicMock.Setup(mock => mock.RemoveDeviceAsync(deviceID)).Returns(Task.FromResult(true));
            await deviceController.DeleteDevice(deviceID);
            deviceLogicMock.Verify(mock => mock.RemoveDeviceAsync(deviceID), Times.Once());
        }
    }
}