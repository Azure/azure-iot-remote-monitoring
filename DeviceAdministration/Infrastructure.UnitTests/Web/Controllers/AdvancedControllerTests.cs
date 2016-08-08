using System;
using System.Collections.Generic;
using System.Web.Mvc;
using DeviceManagement.Infrustructure.Connectivity.Exceptions;
using DeviceManagement.Infrustructure.Connectivity.Models.TerminalDevice;
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
            apiRegMock = new Mock<IApiRegistrationRepository>();
            cellularExtensionMock = new Mock<ICellularExtensions>();
            deviceLogicMock = new Mock<IDeviceLogic>();

            advancedController = new AdvancedController(deviceLogicMock.Object,
                apiRegMock.Object,
                cellularExtensionMock.Object);
            fixture = new Fixture();
        }

        [Fact]
        public void CellularConnTest()
        {
            var result = advancedController.CellularConn();
            var view = result as ViewResult;
            Assert.NotNull(view);
        }

        [Fact]
        public async void ApiRegistrationTest()
        {
            var regModel = fixture.Create<ApiRegistrationModel>();
            apiRegMock.Setup(mock => mock.RecieveDetails()).Returns(regModel);
            var result = advancedController.ApiRegistration();
            var model = result.Model as ApiRegistrationModel;
            Assert.Equal(model, regModel);
        }

        [Fact]
        public async void DeviceAssociation()
        {
            var queryResMock = fixture.Create<DeviceListQueryResult>();
            var iccids = fixture.Create<List<string>>();
            var deviceIds = fixture.Create<List<string>>();
            deviceLogicMock.Setup(mock => mock.GetDevices(It.IsAny<DeviceListQuery>())).ReturnsAsync(queryResMock);
            apiRegMock.Setup(mock => mock.IsApiRegisteredInAzure()).Returns(true);
            cellularExtensionMock.Setup(mock => mock.GetListOfAvailableIccids(It.IsAny<List<DeviceModel>>()))
                .Returns(iccids);
            cellularExtensionMock.Setup(mock => mock.GetListOfAvailableDeviceIDs(It.IsAny<List<DeviceModel>>()))
                .Returns(deviceIds);

            var result = await advancedController.DeviceAssociation();
            var viewBag = result.ViewBag;
            Assert.True(viewBag.HasRegistration);
            Assert.Equal(viewBag.UnassignedIccidList, iccids);
            Assert.Equal(viewBag.UnassignedDeviceIds, deviceIds);

            apiRegMock.Setup(mock => mock.IsApiRegisteredInAzure()).Returns(false);
            result = await advancedController.DeviceAssociation();
            viewBag = result.ViewBag;
            Assert.False(viewBag.HasRegistration);

            apiRegMock.Setup(mock => mock.IsApiRegisteredInAzure())
                .Throws(new CellularConnectivityException(new Exception()));
            result = await advancedController.DeviceAssociation();
            viewBag = result.ViewBag;
            Assert.False(viewBag.HasRegistration);
        }

        [Fact]
        public async void AssociateIccidWithDeviceTest()
        {
            var deviceID = fixture.Create<string>();
            var iccID = fixture.Create<string>();

            var device = fixture.Create<DeviceModel>();
            device.DeviceProperties = fixture.Create<DeviceProperties>();
            device.DeviceProperties.DeviceID = deviceID;
            deviceLogicMock.Setup(mock => mock.GetDeviceAsync(deviceID)).ReturnsAsync(device);
            deviceLogicMock.Setup(mock => mock.UpdateDeviceAsync(It.IsAny<DeviceModel>()))
                .ReturnsAsync(new DeviceModel());

            await advancedController.AssociateIccidWithDevice(deviceID, iccID);

            device.SystemProperties.ICCID = iccID;
            deviceLogicMock.Verify(mock => mock.UpdateDeviceAsync(device), Times.Once());
        }

        [Fact]
        public async void RemoveIccidFromDeviceTest()
        {
            var deviceID = fixture.Create<string>();
            var device = fixture.Create<DeviceModel>();
            device.DeviceProperties = fixture.Create<DeviceProperties>();
            device.DeviceProperties.DeviceID = deviceID;
            deviceLogicMock.Setup(mock => mock.GetDeviceAsync(deviceID)).ReturnsAsync(device);
            deviceLogicMock.Setup(mock => mock.UpdateDeviceAsync(It.IsAny<DeviceModel>()))
                .ReturnsAsync(new DeviceModel());

            await advancedController.RemoveIccidFromDevice(deviceID);
            device.SystemProperties.ICCID = null;
            deviceLogicMock.Verify(mock => mock.UpdateDeviceAsync(device), Times.Once());
        }

        [Fact]
        public async void SaveRegistrationTest()
        {
            var apiRegModel = fixture.Create<ApiRegistrationModel>();
            apiRegMock.Setup(mock => mock.AmendRegistration(apiRegModel)).Returns(true);
            cellularExtensionMock.Setup(mock => mock.GetTerminals()).Returns(new List<Iccid>());
            var result = advancedController.SaveRegistration(apiRegModel);
            Assert.True(result);

            var ex = new Exception("The remote name could not be resolved");
            cellularExtensionMock.Setup(mock => mock.GetTerminals()).Throws(new CellularConnectivityException(ex));
            result = advancedController.SaveRegistration(apiRegModel);
            Assert.False(result);

            ex = new Exception("400200");
            cellularExtensionMock.Setup(mock => mock.GetTerminals()).Throws(new CellularConnectivityException(ex));
            result = advancedController.SaveRegistration(apiRegModel);
            Assert.False(result);

            ex = new Exception("400100");
            cellularExtensionMock.Setup(mock => mock.GetTerminals()).Throws(new CellularConnectivityException(ex));
            result = advancedController.SaveRegistration(apiRegModel);
            Assert.False(result);

            ex = new Exception("message");
            cellularExtensionMock.Setup(mock => mock.GetTerminals()).Throws(new CellularConnectivityException(ex));
            result = advancedController.SaveRegistration(apiRegModel);
            Assert.True(result);
        }
    }
}