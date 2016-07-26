using System;
using System.Collections.Generic;
using System.Linq;
using Dynamitey;
using Microsoft.Azure.Devices.Applications.RemoteMonitoring.Common.Models;
using Microsoft.Azure.Devices.Applications.RemoteMonitoring.Common.Models.Commands;

namespace Microsoft.Azure.Devices.Applications.RemoteMonitoring.Common.DeviceSchema
{
    public static class WireCommandSchemaHelper
    {
        public static List<Parameter> GetParameters(Command command)
        {
            if (command == null)
            {
                throw new ArgumentNullException("command");
            }
          
            List<Parameter> parameters = command.Parameters;

            return parameters;
        }
    }
}
