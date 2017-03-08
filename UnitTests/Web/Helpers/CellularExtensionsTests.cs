using System.Collections.Generic;
using System.Linq;
using DeviceManagement.Infrustructure.Connectivity.Models.TerminalDevice;
using DeviceManagement.Infrustructure.Connectivity.Services;
using Microsoft.Azure.Devices.Applications.RemoteMonitoring.Common.Constants;
using Microsoft.Azure.Devices.Applications.RemoteMonitoring.Common.Models;
using Microsoft.Azure.Devices.Applications.RemoteMonitoring.DeviceAdmin.Infrastructure.Repository;
using Microsoft.Azure.Devices.Applications.RemoteMonitoring.DeviceAdmin.Web.Helpers;
using Moq;
using Ploeh.AutoFixture;
using Xunit;

namespace Microsoft.Azure.Devices.Applications.RemoteMonitoring.UnitTests.Web.Helpers
{
    public class CellularExtensionsTests
    {
        private readonly ICellularExtensions cellularExtensions;
        private readonly Mock<IExternalCellularService> cellularService;
        private readonly IFixture fixture;
        private readonly Mock<IIccidRepository> _iccidRepositoryMock;

        public CellularExtensionsTests()
        {
            cellularService = new Mock<IExternalCellularService>();
            _iccidRepositoryMock = new Mock<IIccidRepository>();
            cellularExtensions = new CellularExtensions(cellularService.Object, _iccidRepositoryMock.Object);
            fixture = new Fixture();
        }

        [Fact]
        public void GetListOfAvailableIccidsTest()
        {
            var iccids = fixture.Create<List<Iccid>>();
            iccids.Add(new Iccid("id1"));
            IList<DeviceModel> devices = fixture.Create<List<DeviceModel>>();
            var device = new DeviceModel();
            device.DeviceProperties = new DeviceProperties();
            device.DeviceProperties.DeviceID = "id1";
            device.SystemProperties = new SystemProperties();
            device.SystemProperties.ICCID = "id1";
            devices.Add(device);
            cellularService.Setup(mock => mock.GetTerminals()).Returns(iccids);
            var result = cellularExtensions.GetListOfAvailableIccids(devices, ApiRegistrationProviderTypes.Jasper);
            Assert.Equal(result.Count(), devices.Count - 1);
            Assert.False(result.Contains("id1"));
        }

        [Fact]
        public void GetListOfAvailableDeviceIDsTest()
        {
            IList<DeviceModel> devices = fixture.Create<List<DeviceModel>>();
            var device = fixture.Create<DeviceModel>();
            device.SystemProperties = null;
            devices.Add(device);
            var result = cellularExtensions.GetListOfAvailableDeviceIDs(devices);
            Assert.Equal(result.Count(), 1);

            devices = fixture.Create<List<DeviceModel>>();
            result = cellularExtensions.GetListOfAvailableDeviceIDs(devices);
            Assert.Equal(result.Count(), 0);
        }
    }
}