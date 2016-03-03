using System;
using System.Collections.Generic;
using System.Dynamic;
using System.Linq;
using System.Reflection;
using System.Linq.Expressions;

namespace Microsoft.Azure.Devices.Applications.RemoteMonitoring.Common.DeviceSchema
{
    public static class WireCommandSchemaHelper
    {
        public static dynamic GetParameters(dynamic command)
        {
            if (command == null)
            {
                throw new ArgumentNullException("command");
            }

            var members = new List<string>();

            var metaObjectProvider = command as IDynamicMetaObjectProvider;
            if (metaObjectProvider != null)
            {
                var dynamicMembers = metaObjectProvider.GetMetaObject(Expression.Constant(metaObjectProvider)).GetDynamicMemberNames();
                members.AddRange(dynamicMembers);
            }

            var reflectionMembers = ((object)command).GetType().GetProperties().Select(m => m.Name);
            members.AddRange(reflectionMembers);

            if (!members.Any(m => m == "Parameters"))
            {
                return "";
            }

            dynamic parameters = command.Parameters;

            return parameters;
        }
    }
}
