using System;
using System.Diagnostics;
using AutoMapper;
using Microsoft.Azure.Devices.Applications.RemoteMonitoring.Common.Models;
using Microsoft.Azure.Devices.Applications.RemoteMonitoring.Common.Models.Commands;
using Newtonsoft.Json.Linq;

namespace Microsoft.Azure.Devices.Applications.RemoteMonitoring.Common.Mapper
{
    public class TypeMapper
    {
        private static TypeMapper typeMapper;
        private TypeMapper()
        {
            AutoMapper.Mapper.Initialize(cfg =>
            {
                cfg.CreateMap<dynamic, DeviceModel>().ConvertUsing(this.typeConverter<DeviceModel>);
                cfg.CreateMap<dynamic, CommandHistory>().ConvertUsing(this.typeConverter<CommandHistory>);
                cfg.CreateMap<dynamic, Command>().ConvertUsing(this.typeConverter<Command>);
            });
            AutoMapper.Mapper.AssertConfigurationIsValid();
        }

        private static void FixIsSimulatedDevice(dynamic device)
        {
            if (device.IsSimulatedDevice != null)
            {
                if (device.IsSimulatedDevice.ToString() == "1")
                    device.IsSimulatedDevice = true;
                else if (device.IsSimulatedDevice.ToString() == "0")
                    device.IsSimulatedDevice = false;
            }
        }

        /// <summary>
        /// Several aspects of the device schema can be modified after passing through and ASA Event Stream
        /// or some other process. Fix up the schema to keep it clean.
        /// </summary>
        /// <param name="device"></param>
        public static void FixDeviceSchema(dynamic device)
        {
            FixHubEnabledStateFormat(device);
            RemoveUnwantedAsaEventProperties(device);
        }

        /// <summary>
        /// Verify that the hub enabled state is stored in the correct format,
        /// and try to fix incorrect formats if possible.
        /// </summary>
        /// <param name="device"></param>
        private static void FixHubEnabledStateFormat(dynamic device)
        {
            dynamic props = device.DeviceProperties;
            if (props.HubEnabledState != null && props.HubEnabledState == 1)
            {
                props.HubEnabledState = true;
            }
            else if (props.HubEnabledState != null && props.HubEnabledState == 0)
            {
                props.HubEnabledState = false;
            }
        }

        /// <summary>
        /// Running the device through ASA can add certain unwanted properties that will persist in
        /// non-strongly typed schemas like Json. Remove those unwanted properties. It may be necessary
        /// to check the type of data we are working with and pass the object on to another private
        /// helper method to handle that specific type of data.
        /// </summary>
        /// <param name="device"></param>
        private static void RemoveUnwantedAsaEventProperties(dynamic device)
        {
            if (device.GetType() == typeof(JObject))
            {
                RemoveUnwantedAsaEventPropertiesFromJObject((JObject)device);
            }
        }

        /// <summary>
        /// Remove unwanted properties that were added by ASA to a Json representation of a device.
        /// </summary>
        /// <param name="device"></param>
        private static void RemoveUnwantedAsaEventPropertiesFromJObject(JObject device)
        {
            device.Remove("EventProcessedUtcTime");
            device.Remove("EventEnqueuedUtcTime");
            device.Remove("PartitionId");
        }

        public static TypeMapper Get()
        {
            return typeMapper ?? (typeMapper = new TypeMapper());
        }

        private T typeConverter<T>(dynamic dynamicObj)
        {
            if (typeof(T) == typeof(DeviceModel))
            {
                FixIsSimulatedDevice(dynamicObj);
                FixDeviceSchema(dynamicObj);
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
