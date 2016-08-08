﻿using System;
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
        private readonly AdvancedController _advancedController;
        private readonly Mock<IApiRegistrationRepository> _apiRegistrationRepositoryMock;
        private readonly Mock<ICellularExtensions> _cellularExtensionMock;
        private readonly Mock<IDeviceLogic> _deviceLogicMock;
        private readonly Fixture _fixture;

        public AdvancedControllerTests()
        {
            _apiRegistrationRepositoryMock = new Mock<IApiRegistrationRepository>();
            _cellularExtensionMock = new Mock<ICellularExtensions>();
            _deviceLogicMock = new Mock<IDeviceLogic>();

            _advancedController = new AdvancedController(
                _deviceLogicMock.Object, 
                _apiRegistrationRepositoryMock.Object,
                _cellularExtensionMock.Object);

            _fixture = new Fixture();
        }

        [Fact]
        public void CellularConnTest()
        {
            var result = _advancedController.CellularConn();
            var view = result as ViewResult;
            Assert.NotNull(view);
        }

        [Fact]
        public void ApiRegistrationTest()
        {
            var regModel = _fixture.Create<ApiRegistrationModel>();
            _apiRegistrationRepositoryMock.Setup(mock => mock.RecieveDetails()).Returns(regModel);
            var result = _advancedController.ApiRegistration();
            var model = result.Model as ApiRegistrationModel;
            Assert.Equal(model, regModel);
        }

        [Fact]
        public async void DeviceAssociation()
        {
            var queryResMock = _fixture.Create<DeviceListQueryResult>();
            var iccids = _fixture.Create<List<string>>();
            var deviceIds = _fixture.Create<List<string>>();
            _deviceLogicMock
                .Setup(mock => mock.GetDevices(It.IsAny<DeviceListQuery>()))
                .ReturnsAsync(queryResMock);

            _apiRegistrationRepositoryMock
                .Setup(mock => mock.IsApiRegisteredInAzure())
                .Returns(true);

            _cellularExtensionMock
                .Setup(mock => mock.GetListOfAvailableIccids(It.IsAny<List<DeviceModel>>()))
                .Returns(iccids);

            _cellularExtensionMock
                .Setup(mock => mock.GetListOfAvailableDeviceIDs(It.IsAny<List<DeviceModel>>()))
                .Returns(deviceIds);

            var result = await _advancedController.DeviceAssociation();
            var viewBag = result.ViewBag;
            Assert.True(viewBag.HasRegistration);
            Assert.Equal(viewBag.UnassignedIccidList, iccids);
            Assert.Equal(viewBag.UnassignedDeviceIds, deviceIds);

            _apiRegistrationRepositoryMock.Setup(mock => mock.IsApiRegisteredInAzure()).Returns(false);
            result = await _advancedController.DeviceAssociation();
            viewBag = result.ViewBag;
            Assert.False(viewBag.HasRegistration);

            _apiRegistrationRepositoryMock.Setup(mock => mock.IsApiRegisteredInAzure())
                .Throws(new CellularConnectivityException(new Exception()));
            result = await _advancedController.DeviceAssociation();
            viewBag = result.ViewBag;
            Assert.False(viewBag.HasRegistration);
        }

        [Fact]
        public async void AssociateIccidWithDeviceTest()
        {
            var deviceId = _fixture.Create<string>();
            var iccid = _fixture.Create<string>();

            var device = _fixture.Create<DeviceModel>();
            device.DeviceProperties = _fixture.Create<DeviceProperties>();
            device.DeviceProperties.DeviceID = deviceId;
            _deviceLogicMock.Setup(mock => mock.GetDeviceAsync(deviceId)).ReturnsAsync(device);
            _deviceLogicMock.Setup(mock => mock.UpdateDeviceAsync(It.IsAny<DeviceModel>()))
                .ReturnsAsync(new DeviceModel());

            await _advancedController.AssociateIccidWithDevice(deviceId, iccid);

            device.SystemProperties.ICCID = iccid;
            _deviceLogicMock.Verify(mock => mock.UpdateDeviceAsync(device), Times.Once());
        }

        [Fact]
        public async void RemoveIccidFromDeviceTest()
        {
            var deviceId = _fixture.Create<string>();
            var device = _fixture.Create<DeviceModel>();
            device.DeviceProperties = _fixture.Create<DeviceProperties>();
            device.DeviceProperties.DeviceID = deviceId;
            _deviceLogicMock.Setup(mock => mock.GetDeviceAsync(deviceId)).ReturnsAsync(device);
            _deviceLogicMock.Setup(mock => mock.UpdateDeviceAsync(It.IsAny<DeviceModel>()))
                .ReturnsAsync(new DeviceModel());

            await _advancedController.RemoveIccidFromDevice(deviceId);
            device.SystemProperties.ICCID = null;
            _deviceLogicMock.Verify(mock => mock.UpdateDeviceAsync(device), Times.Once());
        }

        [Fact]
        public async void SaveRegistrationTest()
        {
            var apiRegModel = _fixture.Create<ApiRegistrationModel>();
            _apiRegistrationRepositoryMock.Setup(mock => mock.AmendRegistration(apiRegModel)).Returns(true);
            _cellularExtensionMock.Setup(mock => mock.GetTerminals()).Returns(new List<Iccid>());
            var result = _advancedController.SaveRegistration(apiRegModel);
            Assert.True(result);

            var ex = new Exception("The remote name could not be resolved");
            _cellularExtensionMock.Setup(mock => mock.GetTerminals()).Throws(new CellularConnectivityException(ex));
            result = _advancedController.SaveRegistration(apiRegModel);
            Assert.False(result);

            ex = new Exception("400200");
            _cellularExtensionMock.Setup(mock => mock.GetTerminals()).Throws(new CellularConnectivityException(ex));
            result = _advancedController.SaveRegistration(apiRegModel);
            Assert.False(result);

            ex = new Exception("400100");
            _cellularExtensionMock.Setup(mock => mock.GetTerminals()).Throws(new CellularConnectivityException(ex));
            result = _advancedController.SaveRegistration(apiRegModel);
            Assert.False(result);

            ex = new Exception("message");
            _cellularExtensionMock.Setup(mock => mock.GetTerminals()).Throws(new CellularConnectivityException(ex));
            result = _advancedController.SaveRegistration(apiRegModel);
            Assert.True(result);
        }
    }
}