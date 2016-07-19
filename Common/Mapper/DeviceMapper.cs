using System;
using System.Collections;
using System.Collections.Generic;
using System.Dynamic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using AutoMapper;
using Microsoft.Azure.Devices.Applications.RemoteMonitoring.Common.Helpers;
using Microsoft.Azure.Devices.Applications.RemoteMonitoring.Common.Models;

namespace Microsoft.Azure.Devices.Applications.RemoteMonitoring.Common.Mapper
{
    public class DeviceMapper : Mapper<DeviceND>
    {
        private static DeviceMapper dm;
        private DeviceMapper() : base(new DeviceCustomLogic())
        {
        }

        public static DeviceMapper Get()
        {
            return dm ?? (dm = new DeviceMapper());
        }
    }
}
