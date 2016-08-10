using Microsoft.Azure.Devices.Applications.RemoteMonitoring.DeviceAdmin.Infrastructure.Repository;
using Xunit;

namespace Microsoft.Azure.Devices.Applications.RemoteMonitoring.UnitTests.Infrastructure
{
    public class SampleDeviceTypeRepositoryTest
    {
        private readonly SampleDeviceTypeRepository sampleDeviceTypeRepository;

        public SampleDeviceTypeRepositoryTest()
        {
            sampleDeviceTypeRepository = new SampleDeviceTypeRepository();
        }

        [Fact]
        public async void GetAllDeviceTypesAsyncTest()
        {
            var deviceTypes = await sampleDeviceTypeRepository.GetAllDeviceTypesAsync();
            Assert.NotNull(deviceTypes);
            Assert.NotEqual(deviceTypes.Count, 0);
        }

        [Fact]
        public async void GetDeviceTypeAsyncTest()
        {
            var ret = await sampleDeviceTypeRepository.GetDeviceTypeAsync(1);
            Assert.NotNull(ret);
            Assert.True(ret.IsSimulatedDevice);

            ret = await sampleDeviceTypeRepository.GetDeviceTypeAsync(2);
            Assert.NotNull(ret);
            Assert.False(ret.IsSimulatedDevice);
        }
    }
}