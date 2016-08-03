using System.Collections.Generic;
using System.Linq;
using DeviceManagement.Infrustructure.Connectivity.Models.TerminalDevice;
using DeviceManagement.Infrustructure.Connectivity.Services;
using Microsoft.Azure.Devices.Applications.RemoteMonitoring.Common.Models;
using Microsoft.Azure.Devices.Applications.RemoteMonitoring.DeviceAdmin.Web.Helpers;
using Moq;
using Ploeh.AutoFixture;
using Xunit;

namespace Microsoft.Azure.Devices.Applications.RemoteMonitoring.DeviceAdmin.Infrastructure.UnitTests.Web.Helpers
{
    public class CellularExtensionsTests
    {
        private readonly ICellularExtensions cellularExtensions;
        private readonly Mock<IExternalCellularService> cellularService;
        private readonly IFixture fixture;

        public CellularExtensionsTests()
        {
            this.cellularService = new Mock<IExternalCellularService>();
            this.cellularExtensions = new CellularExtensions(this.cellularService.Object);
            this.fixture = new Fixture();
        }

        [Fact]
        public void GetListOfAvailableIccidsTest()
        {
            var iccids = this.fixture.Create<List<Iccid>>();
            iccids.Add(new Iccid("id1"));
            IList<DeviceModel> devices = this.fixture.Create<List<DeviceModel>>();
            var device = new DeviceModel();
            device.DeviceProperties = new DeviceProperties();
            device.DeviceProperties.DeviceID = "id1";
            device.SystemProperties = new SystemProperties();
            device.SystemProperties.ICCID = "id1";
            devices.Add(device);
            this.cellularService.Setup(mock => mock.GetTerminals()).Returns(iccids);
            var result = this.cellularExtensions.GetListOfAvailableIccids(devices);
            Assert.Equal(result.Count(), devices.Count - 1);
            Assert.False(result.Contains("id1"));
        }

        [Fact]
        public void GetListOfAvailableDeviceIDsTest()
        {
            IList<DeviceModel> devices = this.fixture.Create<List<DeviceModel>>();
            var device = this.fixture.Create<DeviceModel>();
            device.SystemProperties = null;
            devices.Add(device);
            var result = this.cellularExtensions.GetListOfAvailableDeviceIDs(devices);
            Assert.Equal(result.Count(), 1);

            devices = this.fixture.Create<List<DeviceModel>>();
            result = this.cellularExtensions.GetListOfAvailableDeviceIDs(devices);
            Assert.Equal(result.Count(), 0);
        }
    }
}
