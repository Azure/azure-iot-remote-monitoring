using Microsoft.Azure.Devices.Applications.RemoteMonitoring.Common.Configurations;
using Microsoft.Azure.Devices.Applications.RemoteMonitoring.Common.Models;
using Microsoft.Azure.Devices.Applications.RemoteMonitoring.Simulator.WebJob.SimulatorCore.Logging;
using Microsoft.Azure.Devices.Applications.RemoteMonitoring.Simulator.WebJob.SimulatorCore.Repository;
using Microsoft.Azure.Devices.Applications.RemoteMonitoring.Simulator.WebJob.SimulatorCore.Telemetry;
using Moq;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Xunit;

namespace Microsoft.Azure.Devices.Applications.RemoteMonitoring.UnitTests.Simulator.WebJob
{
    public class AppConfigRepositoryTests
    {
        private readonly Mock<ILogger> _loggerMock;
        private readonly Mock<IConfigurationProvider> _configurationProviderMock;
        private AppConfigRepository repo;

        private string testHostName;
        private string testKey;
        private string testDeviceId;

        public AppConfigRepositoryTests()
        {
            this._loggerMock = new Mock<ILogger>();
            this._configurationProviderMock = new Mock<IConfigurationProvider>();
            this.testHostName = "testHostName";
            this.testKey = "testKey";
            this.testDeviceId = "testDeviceId";

            this._configurationProviderMock.Setup(mock => mock.GetConfigurationSettingValue("iotHub.HostName")).Returns(this.testHostName);

            this.repo = new AppConfigRepository(this._configurationProviderMock.Object, this._loggerMock.Object);
        }

        [Fact]
        public async void GetDeviceListAsyncTest()
        {
            var deviceList = await this.repo.GetDeviceListAsync();

            Assert.NotEmpty(deviceList);
            Assert.Equal(deviceList[0].HostName, this.testHostName);
        }

        [Fact]
        public async void GetDeviceAsyncTest()
        {
            Assert.Null(await this.repo.GetDeviceAsync(""));

            var deviceList = await this.repo.GetDeviceListAsync();

            Assert.NotNull(await this.repo.GetDeviceAsync(deviceList[0].DeviceId));
        }

        [Fact]
        public async void AddOrUpdateDeviceAsyncTest()
        {
            //add device

            InitialDeviceConfig device = new InitialDeviceConfig()
            {
                DeviceId = this.testDeviceId,
                HostName = this.testHostName,
                Key = this.testKey
            };

            //should fail before GetDeviceListAsync initializes the known list of devices
            await this.repo.AddOrUpdateDeviceAsync(device);

            //get all devices 
            Assert.Null(await this.repo.GetDeviceAsync(device.DeviceId));
            List<InitialDeviceConfig> devices = await this.repo.GetDeviceListAsync();
            Assert.False(devices.Contains(device));

            //repeat and it should be added
            await this.repo.AddOrUpdateDeviceAsync(device);
            var returnedDevice = await this.repo.GetDeviceAsync(device.DeviceId);
            Assert.NotNull(returnedDevice);
            Assert.Equal(returnedDevice.Key, this.testKey);
            Assert.Equal(returnedDevice.HostName, this.testHostName);

            //repeat and it should be updated
            string changedKey = "changedKey";
            device.Key = changedKey;
            string changedHostName = "changedHostName";
            device.HostName = changedHostName;
            await this.repo.AddOrUpdateDeviceAsync(device);

            var changedDevice = await this.repo.GetDeviceAsync(device.DeviceId);
            Assert.NotNull(changedDevice);
            Assert.Equal(changedDevice.Key, changedKey);
            Assert.Equal(changedDevice.HostName, changedHostName);
        }

        [Fact]
        public async void RemoveDeviceAsyncTest()
        {
            InitialDeviceConfig device = new InitialDeviceConfig()
            {
                DeviceId = this.testDeviceId,
                HostName = this.testHostName,
                Key = this.testKey
            };

            Assert.False(await this.repo.RemoveDeviceAsync(device.DeviceId));

            await this.repo.GetDeviceListAsync();
            await this.repo.AddOrUpdateDeviceAsync(device);

            Assert.True(await this.repo.RemoveDeviceAsync(device.DeviceId));
            Assert.False(await this.repo.RemoveDeviceAsync(device.DeviceId));
        }
    }
}