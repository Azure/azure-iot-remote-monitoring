using System;
using Microsoft.Azure.Devices.Applications.RemoteMonitoring.Common.Models;

namespace Microsoft.Azure.Devices.Applications.RemoteMonitoring.Common.Mapper
{
    public interface ICustomLogic<in T>
    {
        void before(dynamic obj);
        void after(dynamic obj1, T obj2);
    }

    public class CustomLogic<T> : ICustomLogic<T>
    {
        public void before(dynamic obj)
        {
        }
        public void after(dynamic obj1, T obj2)
        {
            MappingValidator.ValidateAndThrow(obj2, obj2);
        }
    }

    public class DeviceCustomLogic : ICustomLogic<DeviceND>
    {
        public void before(dynamic device)
        {
            FixIsSimulatedDevice(device);
        }

        public void after(dynamic device1, DeviceND device2)
        {
            MappingValidator.ValidateAndThrow(device1, device2);
        }

        private static void FixIsSimulatedDevice(dynamic device)
        {
            if (device.IsSimulatedDevice != null && device.IsSimulatedDevice == 1)
            {
                device.IsSimulatedDevice = true;
            }
            else if (device.IsSimulatedDevice != null && device.IsSimulatedDevice == 0)
            {
                device.IsSimulatedDevice = false;
            }
        }
    }
}