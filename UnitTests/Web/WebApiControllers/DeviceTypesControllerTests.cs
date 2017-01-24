using System.Collections.Generic;
using System.Linq;
using Microsoft.Azure.Devices.Applications.RemoteMonitoring.DeviceAdmin.Infrastructure.BusinessLogic;
using Microsoft.Azure.Devices.Applications.RemoteMonitoring.DeviceAdmin.Infrastructure.Models;
using Microsoft.Azure.Devices.Applications.RemoteMonitoring.DeviceAdmin.Infrastructure.Repository;
using Microsoft.Azure.Devices.Applications.RemoteMonitoring.DeviceAdmin.Web.WebApiControllers;
using Xunit;

namespace Microsoft.Azure.Devices.Applications.RemoteMonitoring.UnitTests.Web.
    WebApiControllers
{
    public class DeviceTypesControllerTests
    {
        private readonly IDeviceTypeLogic deviceTypeLogic;
        private readonly DeviceTypesController deviceTypesController;

        public DeviceTypesControllerTests()
        {
            deviceTypeLogic = new DeviceTypeLogic(new SampleDeviceTypeRepository());
            deviceTypesController = new DeviceTypesController(deviceTypeLogic);
            deviceTypesController.InitializeRequest();
        }

        [Fact]
        public async void GetAllDeviceTypesTest()
        {
            var result = await deviceTypesController.GetAllDeviceTypes();
            result.AssertOnError();
            var data = result.ExtractContentDataAs<List<DeviceType>>();
            Assert.Equal(data.Count, 2);
            Assert.True(data.First().IsSimulatedDevice);
        }

        [Fact]
        public async void GetDeviceTypeTests()
        {
            var res = await deviceTypesController.GetDeviceType(1);
            res.AssertOnError();
            var data = res.ExtractContentDataAs<DeviceType>();
            Assert.Equal(data.Name, "Simulated Device");
            res = await deviceTypesController.GetDeviceType(2);
            res.AssertOnError();
            data = res.ExtractContentDataAs<DeviceType>();
            Assert.Equal(data.Name, "Custom Device");
        }
    }
}