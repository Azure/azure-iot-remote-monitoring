﻿using System;
using System.Collections.Generic;
using Microsoft.Azure.Devices.Applications.RemoteMonitoring.DeviceAdmin.Infrastructure.Models;
using Microsoft.Azure.Devices.Applications.RemoteMonitoring.DeviceAdmin.Infrastructure.Repository;
using Xunit;

namespace Microsoft.Azure.Devices.Applications.RemoteMonitoring.DeviceAdmin.Infrastructure.UnitTests.Infrastructure
{
    public class SampleDeviceTypeRepositoryTest
    {
        private SampleDeviceTypeRepository sampleDeviceTypeRepository;

        public SampleDeviceTypeRepositoryTest()
        {
            sampleDeviceTypeRepository = new SampleDeviceTypeRepository();
        }

        [Fact]
        public async void GetAllDeviceTypesAsyncTest()
        {
            List<DeviceType> deviceTypes = await sampleDeviceTypeRepository.GetAllDeviceTypesAsync();
            Assert.NotNull(deviceTypes);
            Assert.NotEqual(deviceTypes.Count, 0);
        }

        [Fact]
        public async void GetDeviceTypeAsyncTest()
        {
            DeviceType ret = await sampleDeviceTypeRepository.GetDeviceTypeAsync(1);
            Assert.NotNull(ret);
            Assert.True(ret.IsSimulatedDevice);

            ret = await sampleDeviceTypeRepository.GetDeviceTypeAsync(2);
            Assert.NotNull(ret);
            Assert.False(ret.IsSimulatedDevice);
        }
    }
}