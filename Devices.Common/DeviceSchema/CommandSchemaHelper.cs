using System;
using System.Collections;
using System.Collections.Generic;
using Microsoft.Azure.Devices.Applications.RemoteMonitoring.Common.Models.Commands;
using Newtonsoft.Json.Linq;

namespace Microsoft.Azure.Devices.Applications.RemoteMonitoring.Common.DeviceSchema
{
    /// <summary>
    /// Helper class to encapsulate interactions with the command schema.
    /// 
    /// Elsewhere in the app we try to always deal with this flexible schema as dynamic,
    /// but here we take a dependency on Json.Net to populate the objects behind the schema.
    /// </summary>
    public static class CommandSchemaHelper
    {
        /// <summary>
        /// Retrieve from a device the commands that it can perform
        /// </summary>
        /// <param name="device"></param>
        /// <returns></returns>
        public static dynamic GetSupportedCommands(dynamic device)
        {
            if (device == null)
            {
                throw new ArgumentNullException("device");
            }

            dynamic commands = device.Commands;

            if (commands == null)
            {
                commands = new JArray();
                device.Commands = commands;
            }

            return commands;
        }
    }
}
