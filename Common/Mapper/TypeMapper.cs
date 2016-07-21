using System;
using AutoMapper;
using Microsoft.Azure.Devices.Applications.RemoteMonitoring.Common.Models;
using Microsoft.Azure.Devices.Applications.RemoteMonitoring.Common.Models.Commands;

namespace Microsoft.Azure.Devices.Applications.RemoteMonitoring.Common.Mapper
{
    public class TypeMapper
    {
        private static TypeMapper typeMapper;
        private TypeMapper()
        {
            AutoMapper.Mapper.Initialize(cfg =>
            {
                cfg.CreateMap<dynamic, DeviceND>().ConvertUsing(this.typeConverter<DeviceND>);
                cfg.CreateMap<dynamic, CommandHistoryND>().ConvertUsing(this.typeConverter<CommandHistoryND>);
                cfg.CreateMap<dynamic, Command>().ConvertUsing(this.typeConverter<Command>);
            });
            AutoMapper.Mapper.AssertConfigurationIsValid();
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

        public static TypeMapper Get()
        {
            return typeMapper ?? (typeMapper = new TypeMapper());
        }

        private T typeConverter<T>(dynamic dynamicObj)
        {
            if (typeof(T) == typeof(DeviceND))
            {
                FixIsSimulatedDevice(dynamicObj);
            }
            string dynamicObjStr = Newtonsoft.Json.JsonConvert.SerializeObject(dynamicObj);
            T strongObj = Newtonsoft.Json.JsonConvert.DeserializeObject<T>(dynamicObjStr);
            MappingValidator.ValidateAndThrow(dynamicObj, strongObj);
            return strongObj;
        }

        public T map<T>(dynamic obj)
        {
            return AutoMapper.Mapper.Map<T>(obj);
        }
    }
}
