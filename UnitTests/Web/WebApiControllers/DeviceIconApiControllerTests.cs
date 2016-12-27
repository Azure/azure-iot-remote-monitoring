using System.Collections.Generic;
using System.Threading.Tasks;
using System.Web.Http;
using Microsoft.Azure.Devices.Applications.RemoteMonitoring.Common.Models;
using Microsoft.Azure.Devices.Applications.RemoteMonitoring.Common.Models.Commands;
using Microsoft.Azure.Devices.Applications.RemoteMonitoring.DeviceAdmin.Infrastructure.BusinessLogic;
using Microsoft.Azure.Devices.Applications.RemoteMonitoring.DeviceAdmin.Infrastructure.Models;
using Microsoft.Azure.Devices.Applications.RemoteMonitoring.DeviceAdmin.Web.DataTables;
using Microsoft.Azure.Devices.Applications.RemoteMonitoring.DeviceAdmin.Web.WebApiControllers;
using Moq;
using Newtonsoft.Json.Linq;
using Ploeh.AutoFixture;
using Xunit;
using Microsoft.Azure.Devices.Applications.RemoteMonitoring.DeviceAdmin.Infrastructure.Repository;
using Microsoft.WindowsAzure.Storage.Blob;
using Ploeh.AutoFixture.Kernel;
using System.Linq;
using System;

namespace Microsoft.Azure.Devices.Applications.RemoteMonitoring.UnitTests.Web.WebApiControllers
{
    public class DeviceIconApiControllerTests
    {
        private readonly DeviceIconApiController deviceIconApiController;
        private readonly Mock<IIoTHubDeviceManager> deviceManager;
        private readonly Mock<IDeviceIconRepository> deviceIconRepository;
        private readonly IFixture fixture;

        public DeviceIconApiControllerTests()
        {
            deviceManager = new Mock<IIoTHubDeviceManager>();
            deviceIconRepository = new Mock<IDeviceIconRepository>();
            deviceIconApiController = new DeviceIconApiController(deviceManager.Object, deviceIconRepository.Object);
            fixture = new Fixture();
        }
    }
}
