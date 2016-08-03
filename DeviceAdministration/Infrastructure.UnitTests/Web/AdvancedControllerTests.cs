using System;
using System.Collections.Generic;
using System.Web.Mvc;
using DeviceManagement.Infrustructure.Connectivity.Exceptions;
using DeviceManagement.Infrustructure.Connectivity.Models.TerminalDevice;
using DeviceManagement.Infrustructure.Connectivity.Services;
using Microsoft.Azure.Devices.Applications.RemoteMonitoring.Common.Models;
using Microsoft.Azure.Devices.Applications.RemoteMonitoring.DeviceAdmin.Infrastructure.BusinessLogic;
using Microsoft.Azure.Devices.Applications.RemoteMonitoring.DeviceAdmin.Infrastructure.Models;
using Microsoft.Azure.Devices.Applications.RemoteMonitoring.DeviceAdmin.Infrastructure.Repository;
using Microsoft.Azure.Devices.Applications.RemoteMonitoring.DeviceAdmin.Web.Controllers;
using Microsoft.Azure.Devices.Applications.RemoteMonitoring.DeviceAdmin.Web.Helpers;
using Moq;
using Ploeh.AutoFixture;
using Xunit;

namespace Microsoft.Azure.Devices.Applications.RemoteMonitoring.DeviceAdmin.Infrastructure.UnitTests.Web
{
    public class AdvancedControllerTests
    {
        private readonly AdvancedController advancedController;
        private readonly Mock<IApiRegistrationRepository> apiRegMock;
        private readonly Mock<ICellularExtensions> cellularExtensionMock;
        private readonly Mock<IDeviceLogic> deviceLogicMock;
        private readonly Fixture fixture;

        public AdvancedControllerTests()
        {
            this.apiRegMock = new Mock<IApiRegistrationRepository>();
            this.cellularExtensionMock = new Mock<ICellularExtensions>();
            this.deviceLogicMock = new Mock<IDeviceLogic>();

            this.advancedController = new AdvancedController(this.deviceLogicMock.Object,
                                                             this.apiRegMock.Object,
                                                             this.cellularExtensionMock.Object);
            this.fixture = new Fixture();
        }

        [Fact]
        public void CellularConnTest()
        {
            var result = this.advancedController.CellularConn();
            var view = result as ViewResult;
            Assert.NotNull(view);
        }

        [Fact]
        public async void ApiRegistrationTest()
        {
            var regModel = this.fixture.Create<ApiRegistrationModel>();
            this.apiRegMock.Setup(mock => mock.RecieveDetails()).Returns(regModel);
            var result = this.advancedController.ApiRegistration();
            var model = result.Model as ApiRegistrationModel;
            Assert.Equal(model, regModel);
        }

        [Fact]
        public async void DeviceAssociation()
        {
            var queryResMock = this.fixture.Create<DeviceListQueryResult>();
            var iccids = this.fixture.Create<List<string>>();
            var deviceIds = this.fixture.Create<List<string>>();
            this.deviceLogicMock.Setup(mock => mock.GetDevices(It.IsAny<DeviceListQuery>())).ReturnsAsync(queryResMock);
            this.apiRegMock.Setup(mock => mock.IsApiRegisteredInAzure()).Returns(true);
            this.cellularExtensionMock.Setup(mock => mock.GetListOfAvailableIccids(It.IsAny<List<DeviceModel>>()))
                .Returns(iccids);
            this.cellularExtensionMock.Setup(mock => mock.GetListOfAvailableDeviceIDs(It.IsAny<List<DeviceModel>>()))
                .Returns(deviceIds);

            var result = await this.advancedController.DeviceAssociation();
            var viewBag = result.ViewBag;
            Assert.True(viewBag.HasRegistration);
            Assert.Equal(viewBag.UnassignedIccidList, iccids);
            Assert.Equal(viewBag.UnassignedDeviceIds, deviceIds);

            this.apiRegMock.Setup(mock => mock.IsApiRegisteredInAzure()).Returns(false);
            result = await this.advancedController.DeviceAssociation();
            viewBag = result.ViewBag;
            Assert.False(viewBag.HasRegistration);

            this.apiRegMock.Setup(mock => mock.IsApiRegisteredInAzure()).Throws(new CellularConnectivityException(new Exception()));
            result = await this.advancedController.DeviceAssociation();
            viewBag = result.ViewBag;
            Assert.False(viewBag.HasRegistration);
        }

        [Fact]
        public async void AssociateIccidWithDeviceTest()
        {
            var deviceID = this.fixture.Create<string>();
            var iccID = this.fixture.Create<string>();

            var device = this.fixture.Create<DeviceModel>();
            device.DeviceProperties = this.fixture.Create<DeviceProperties>();
            device.DeviceProperties.DeviceID = deviceID;
            this.deviceLogicMock.Setup(mock => mock.GetDeviceAsync(deviceID)).ReturnsAsync(device);
            this.deviceLogicMock.Setup(mock => mock.UpdateDeviceAsync(It.IsAny<DeviceModel>())).ReturnsAsync(new DeviceModel());

            await this.advancedController.AssociateIccidWithDevice(deviceID, iccID);
            
            device.SystemProperties.ICCID = iccID;
            this.deviceLogicMock.Verify(mock => mock.UpdateDeviceAsync(device), Times.Once());
        }

        [Fact]
        public async void RemoveIccidFromDeviceTest()
        {
            var deviceID = this.fixture.Create<string>();
            var device = this.fixture.Create<DeviceModel>();
            device.DeviceProperties = this.fixture.Create<DeviceProperties>();
            device.DeviceProperties.DeviceID = deviceID;
            this.deviceLogicMock.Setup(mock => mock.GetDeviceAsync(deviceID)).ReturnsAsync(device);
            this.deviceLogicMock.Setup(mock => mock.UpdateDeviceAsync(It.IsAny<DeviceModel>())).ReturnsAsync(new DeviceModel());

            await this.advancedController.RemoveIccidFromDevice(deviceID);
            device.SystemProperties.ICCID = null;
            this.deviceLogicMock.Verify(mock => mock.UpdateDeviceAsync(device), Times.Once());
        }

        [Fact]
        public async void SaveRegistrationTest()
        {
            var apiRegModel = this.fixture.Create<ApiRegistrationModel>();
            this.apiRegMock.Setup(mock => mock.AmendRegistration(apiRegModel)).Returns(true);
            this.cellularExtensionMock.Setup(mock => mock.GetTerminals()).Returns(new List<Iccid>());
            var result = this.advancedController.SaveRegistration(apiRegModel);
            Assert.True(result);

            var ex = new Exception("The remote name could not be resolved");
            this.cellularExtensionMock.Setup(mock => mock.GetTerminals()).Throws(new CellularConnectivityException(ex));
            result = this.advancedController.SaveRegistration(apiRegModel);
            Assert.False(result);

            ex = new Exception("400200");
            this.cellularExtensionMock.Setup(mock => mock.GetTerminals()).Throws(new CellularConnectivityException(ex));
            result = this.advancedController.SaveRegistration(apiRegModel);
            Assert.False(result);

            ex = new Exception("400100");
            this.cellularExtensionMock.Setup(mock => mock.GetTerminals()).Throws(new CellularConnectivityException(ex));
            result = this.advancedController.SaveRegistration(apiRegModel);
            Assert.False(result);

            ex = new Exception("message");
            this.cellularExtensionMock.Setup(mock => mock.GetTerminals()).Throws(new CellularConnectivityException(ex));
            result = this.advancedController.SaveRegistration(apiRegModel);
            Assert.True(result);
        }
    }
}
