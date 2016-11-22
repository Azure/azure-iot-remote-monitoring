using System;
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;

namespace Microsoft.Azure.Devices.Applications.RemoteMonitoring.Common.JsonContractResolvers
{
    public class SkipByNameContractResolver : DefaultContractResolver
    {
        private IEnumerable<string> _toBoSkipped;

        public SkipByNameContractResolver(params string[] toBeSkipped)
        {
            _toBoSkipped = toBeSkipped;
        }

        protected override IList<JsonProperty> CreateProperties(Type type, MemberSerialization memberSerialization)
        {
            return base.CreateProperties(type, memberSerialization).Where(p => !_toBoSkipped.Contains(p.PropertyName)).ToList();
        }
    }
}
