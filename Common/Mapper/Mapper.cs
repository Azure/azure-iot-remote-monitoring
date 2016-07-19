using System;
using AutoMapper;

namespace Microsoft.Azure.Devices.Applications.RemoteMonitoring.Common.Mapper
{
    public class Mapper<T>
    {
        private readonly ICustomLogic<T> customLogic;
        public Mapper(ICustomLogic<T> customLogic)
        {
            AutoMapper.Mapper.Initialize(cfg => cfg.CreateMap<dynamic, T>()
                                                   .ConvertUsing(this.typeConverter));

            AutoMapper.Mapper.AssertConfigurationIsValid();
            this.customLogic = customLogic;
        }

        private T typeConverter(dynamic dynamicObj)
        {
            this.customLogic.before(dynamicObj);
            string dynamicObjStr = Newtonsoft.Json.JsonConvert.SerializeObject(dynamicObj);
            T strongObj = Newtonsoft.Json.JsonConvert.DeserializeObject<T>(dynamicObjStr);
            this.customLogic.after(dynamicObj, strongObj);
            return strongObj;
        }

        public T map(dynamic obj)
        {
            return AutoMapper.Mapper.Map<T>(obj);
        }
    }
}
