using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Microsoft.Azure.Devices.Applications.RemoteMonitoring.Common.Models;
using Microsoft.Azure.Devices.Applications.RemoteMonitoring.Common.Models.Commands;
using Microsoft.Azure.Devices.Shared;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Microsoft.Azure.Devices.Applications.RemoteMonitoring.Common.Helpers
{

    public static class SupportedMethodsHelper
    {
        private static string SupportedMethodsKey = "SupportedMethods";

        public static JObject GenerateSupportedMethodsReportedProperty(List<Command> commands)
        {
            var methods = new Dictionary<string, SupportedMethod>();
            foreach (var command in commands.Where(c => c.DeliveryType == DeliveryType.Method))
            {
                string normalizedMethodName = NormalizeMethodName(command);
                methods[normalizedMethodName] = Convert(command);
            }

            var obj = new JObject();
            obj[SupportedMethodsKey] = JObject.FromObject(methods);

            return obj;
        }

        public static void AddSupportedMethodsFromReportedProperty(DeviceModel device, Twin twin)
        {
            if (!twin.Properties.Reported.Contains(SupportedMethodsKey))
            {
                return;
            }

            // The property could be either TwinColltion or JObject that depends on where the property is generated from
            // Convert the property to JObject to unify the type.
            var methods = JObject.FromObject(twin.Properties.Reported[SupportedMethodsKey]).ToObject<Dictionary<string, SupportedMethod>>();

            foreach (var method in methods)
            {
                var command = Convert(method.Value);

                if (command != null && !device.Commands.Any(c => c.Name == command.Name && c.DeliveryType == DeliveryType.Method))
                {
                    device.Commands.Add(command);
                }
            }
        }

        public static bool IsSupportedMethodProperty(string propertyName)
        {
            return propertyName == SupportedMethodsKey || propertyName.StartsWith(SupportedMethodsKey + ".");
        }

        /// <summary>
        /// Convert Command to SupportedMethod. Array will be convert to an object since the Twin doesn't support array.
        /// </summary>
        /// <param name="command"></param>
        /// <returns></returns>
        private static SupportedMethod Convert(Command command)
        {
            var method = new SupportedMethod();
            method.Name = command.Name;
            method.Description = command.Description;
            command.Parameters.ForEach(p => method.Parameters.Add(p.Name, p));

            return method;
        }

        /// <summary>
        /// Convert SupportedMethod back to Command.
        /// </summary>
        /// <param name="method"></param>
        /// <returns></returns>
        private static Command Convert(SupportedMethod method)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(method.Name))
                {
                    throw new ArgumentNullException("Method Name");
                }

                var command = new Command();
                command.Name = method.Name;
                command.Description = method.Description;
                command.DeliveryType = DeliveryType.Method;

                foreach (var parameter in method.Parameters)
                {
                    if (string.IsNullOrWhiteSpace(parameter.Value.Type))
                    {
                        throw new ArgumentNullException("Parameter Type");
                    }

                    command.Parameters.Add(new Parameter(parameter.Key, parameter.Value.Type));
                }

                return command;
            }
            catch (Exception ex)
            {
                Trace.TraceWarning("Failed to covert supported method from reported property : {0}, message: {1}", JsonConvert.SerializeObject(method), ex.Message);

                return null;
            }
        }

        public static string NormalizeMethodName(Command command)
        {
            var parts = new List<string> { command.Name.Replace("_", "__") };
            parts.AddRange(command.Parameters.Select(p => p.Type).ToList());

            return string.Join("_", parts);
        }
    }
}
