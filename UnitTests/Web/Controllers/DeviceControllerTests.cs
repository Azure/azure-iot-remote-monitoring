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

namespace Microsoft.Azure.Devices.Applications.RemoteMonitoring.UnitTests.Web
{
    public class DeviceControllerTests
    {
        private readonly Mock<IApiRegistrationRepository> _apiRegistrationRepository;
        private readonly Mock<ICellularExtensions> _cellulerExtensionsMock;
        private readonly DeviceController _deviceController;
        private readonly Mock<IDeviceLogic> _deviceLogicMock;
        private readonly Fixture _fixture;

        public DeviceControllerTests()
        {
            _deviceLogicMock = new Mock<IDeviceLogic>();
            _cellulerExtensionsMock = new Mock<ICellularExtensions>();
            _apiRegistrationRepository = new Mock<IApiRegistrationRepository>();

            var configProviderMock = new Mock<IConfigurationProvider>();
            configProviderMock
                .Setup(mock => mock.GetConfigurationSettingValue(("iotHub.HostName")))
                .Returns("hubName");

            _deviceController = new DeviceController(
                _deviceLogicMock.Object,
                new DeviceTypeLogic(new SampleDeviceTypeRepository()),
                configProviderMock.Object,
                _apiRegistrationRepository.Object,
                _cellulerExtensionsMock.Object);

            _fixture = new Fixture();
        }

        [Fact]
        public async void AddDeviceTestTest()
        {
            var result = await _deviceController.AddDevice();
            var viewResult = result as ViewResult;
            var model = viewResult.Model as List<DeviceType>;
            Assert.Equal(model.Count, 2);
            Assert.True(model.First().Name == "Simulated Device" || model.First().Name == "Custom Device");
        }

        [Fact]
        public async void SelectTypeTest()
        {
            var deviceType = _fixture.Create<DeviceType>();
            var devices = _fixture.Create<DeviceListQueryResult>();
            var iccids = _fixture.Create<List<string>>();

            _apiRegistrationRepository.Setup(repo => repo.IsApiRegisteredInAzure()).Returns(true);
            _deviceLogicMock.Setup(mock => mock.GetDevices(It.IsAny<DeviceListQuery>())).ReturnsAsync(devices);
            _cellulerExtensionsMock.Setup(mock => mock.GetListOfAvailableIccids(It.IsAny<List<DeviceModel>>()))
                .Returns(iccids);

            var result = await _deviceController.SelectType(deviceType);
            var viewResult = result as PartialViewResult;
            var model = viewResult.Model as UnregisteredDeviceModel;
            var viewBag = viewResult.ViewBag;
            Assert.Equal(model.DeviceType, deviceType);
            Assert.True(model.IsDeviceIdSystemGenerated);
            Assert.True(viewBag.CanHaveIccid);
            Assert.Equal(viewBag.AvailableIccids, iccids);

            //IsApiRegisteredInAzure returns false
            _apiRegistrationRepository.Setup(repo => repo.IsApiRegisteredInAzure()).Returns(false);
            result = await _deviceController.SelectType(deviceType);
            viewResult = result as PartialViewResult;
            model = viewResult.Model as UnregisteredDeviceModel;
            viewBag = viewResult.ViewBag;
            Assert.Equal(model.DeviceType, deviceType);
            Assert.True(model.IsDeviceIdSystemGenerated);
            Assert.False(viewBag.CanHaveIccid);

            //GetListOfAvailableIccids throws
            _apiRegistrationRepository.Setup(repo => repo.IsApiRegisteredInAzure()).Returns(true);
            _cellulerExtensionsMock.Setup(mock => mock.GetListOfAvailableIccids(It.IsAny<List<DeviceModel>>()))
                .Throws(new CellularConnectivityException(new Exception()));
            result = await _deviceController.SelectType(deviceType);
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
            var button = _fixture.Create<string>();
            var deviceModel = _fixture.Create<UnregisteredDeviceModel>();
            var devices = _fixture.Create<DeviceListQueryResult>();
            var iccids = _fixture.Create<List<string>>();

            _apiRegistrationRepository.Setup(repo => repo.IsApiRegisteredInAzure()).Returns(true);
            _deviceLogicMock.Setup(mock => mock.GetDevices(It.IsAny<DeviceListQuery>())).ReturnsAsync(devices);
            _deviceLogicMock.Setup(mock => mock.GetDeviceAsync(It.IsAny<string>())).ReturnsAsync(new DeviceModel());
            var result = await _deviceController.AddDeviceCreate(button, deviceModel);

            var viewResult = result as PartialViewResult;
            var model = viewResult.Model as UnregisteredDeviceModel;
            var viewBag = viewResult.ViewBag;
            Assert.True(viewBag.CanHaveIccid);
            Assert.Equal(model, deviceModel);
        }

        [Fact]
        public async void EditDevicePropertiesTest()
        {
            var editModel = _fixture.Create<EditDevicePropertiesModel>();
            _deviceLogicMock.Setup(mock => mock.GetDeviceAsync(It.IsAny<string>())).ReturnsAsync(new DeviceModel());
            var result = await _deviceController.EditDeviceProperties(editModel);
            Assert.NotNull(result);

            //TODO: doesn't work
            //var view = result as ViewResult;
        }

        [Fact]
        public async void EditDevicePropertiesWithDeviceIdTest()
        {
            var deviceId = _fixture.Create<string>();
            var deviceModel = _fixture.Create<DeviceModel>();
            deviceModel.DeviceProperties = _fixture.Create<DeviceProperties>();
            deviceModel.DeviceProperties.DeviceID = deviceId;
            var propModel = _fixture.Create<IEnumerable<DevicePropertyValueModel>>();
            _deviceLogicMock.Setup(mock => mock.GetDeviceAsync(deviceId)).ReturnsAsync(deviceModel);
            _deviceLogicMock.Setup(mock => mock.ExtractDevicePropertyValuesModels(deviceModel)).Returns(propModel);

            var result = await _deviceController.EditDeviceProperties(deviceId);
            var view = result as ViewResult;
            var model = view.Model as EditDevicePropertiesModel;
            Assert.NotNull(model.DevicePropertyValueModels);
            Assert.Equal(model.DevicePropertyValueModels.Count, propModel.Count());


            _deviceLogicMock.Setup(mock => mock.GetDeviceAsync(deviceId)).ReturnsAsync(null);
            result = await _deviceController.EditDeviceProperties(deviceId);
            view = result as ViewResult;
            model = view.Model as EditDevicePropertiesModel;
            Assert.Equal(model.DevicePropertyValueModels.Count, 0);
        }

        [Fact]
        public async void GetDeviceDetailsTest()
        {
            var deviceId = _fixture.Create<string>();
            var deviceModel = _fixture.Create<DeviceModel>();
            deviceModel.DeviceProperties = _fixture.Create<DeviceProperties>();
            deviceModel.DeviceProperties.DeviceID = deviceId;
            _deviceLogicMock.Setup(mock => mock.GetDeviceAsync(deviceId)).ReturnsAsync(deviceModel);

            var result = await _deviceController.GetDeviceDetails(deviceId);
            var view = result as PartialViewResult;
            var model = view.Model as DeviceDetailModel;
            Assert.Equal(model.DeviceID, deviceId);
            Assert.Equal(model.HubEnabledState, deviceModel.DeviceProperties.HubEnabledState);
            Assert.Equal(model.IsCellular, true);
            Assert.Equal(model.Iccid, deviceModel.SystemProperties.ICCID);

            _deviceLogicMock.Setup(mock => mock.GetDeviceAsync(deviceId)).ReturnsAsync(null);
            await Assert.ThrowsAsync<InvalidOperationException>(() => _deviceController.GetDeviceDetails(deviceId));
        }

        [Fact]
        public async void GetDeviceKeysTest()
        {
            var keys = _fixture.Create<SecurityKeys>();
            var deviceId = _fixture.Create<string>();
            _deviceLogicMock.Setup(mock => mock.GetIoTHubKeysAsync(deviceId)).ReturnsAsync(keys);
            var keyModel = await _deviceController.GetDeviceKeys(deviceId);
            var viewResult = keyModel as PartialViewResult;
            var model = viewResult.Model as SecurityKeysModel;
            Assert.Equal(model.PrimaryKey, keys.PrimaryKey);
            Assert.Equal(model.SecondaryKey, keys.SecondaryKey);
        }

        [Fact]
        public void GetDeviceCellularDetailsTest()
        {
            var iccId = _fixture.Create<string>();
            var terminalDevice = _fixture.Create<Terminal>();
            var sessionInfo = _fixture.Create<List<SessionInfo>>();
            _cellulerExtensionsMock.Setup(mock => mock.GetSingleTerminalDetails(It.IsAny<Iccid>()))
                .Returns(terminalDevice);
            _cellulerExtensionsMock.Setup(mock => mock.GetSingleSessionInfo(It.IsAny<Iccid>())).Returns(sessionInfo);

            var result = _deviceController.GetDeviceCellularDetails(iccId);
            var viewResult = result as PartialViewResult;
            var model = viewResult.Model as SimInformationViewModel;
            Assert.Equal(model.TerminalDevice, terminalDevice);
            Assert.Equal(model.SessionInfo, sessionInfo.LastOrDefault());
        }

        [Fact]
        public void RemoveDeviceTest()
        {
            var deviceID = _fixture.Create<string>();
            var result = _deviceController.RemoveDevice(deviceID);
            var viewResult = result as ViewResult;
            var model = viewResult.Model as RegisteredDeviceModel;
            Assert.Equal(model.HostName, "hubName");
            Assert.Equal(model.DeviceId, deviceID);
        }

        [Fact]
        public async void DeleteDeviceTest()
        {
            var deviceID = _fixture.Create<string>();
            _deviceLogicMock.Setup(mock => mock.RemoveDeviceAsync(deviceID)).Returns(Task.FromResult(true));
            await _deviceController.DeleteDevice(deviceID);
            _deviceLogicMock.Verify(mock => mock.RemoveDeviceAsync(deviceID), Times.Once());
        }
    }
}