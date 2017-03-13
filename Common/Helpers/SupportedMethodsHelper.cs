using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Microsoft.Azure.Devices.Applications.RemoteMonitoring.Common.Extensions;
using Microsoft.Azure.Devices.Applications.RemoteMonitoring.Common.Models;
using Microsoft.Azure.Devices.Applications.RemoteMonitoring.Common.Models.Commands;
using Microsoft.Azure.Devices.Shared;

namespace Microsoft.Azure.Devices.Applications.RemoteMonitoring.Common.Helpers
{

    public static class SupportedMethodsHelper
    {
        private static string SupportedMethodsKey = "SupportedMethods";

        public static void CreateSupportedMethodReport(TwinCollection patch, IEnumerable<Command> commands, TwinCollection reported)
        {
            var existingMethods = new HashSet<string>();
            if (reported != null && reported.Contains("SupportedMethods"))
            {
                existingMethods.UnionWith(reported.AsEnumerableFlatten()
                    .Select(pair => pair.Key)
                    .Where(key => key.StartsWith("SupportedMethods.", StringComparison.Ordinal))
                    .Select(key => key.Split('.')[1]));
            }

            var supportedMethods = new TwinCollection();
            foreach (var method in commands.Where(c => c.DeliveryType == DeliveryType.Method))
            {
                if (string.IsNullOrWhiteSpace(method.Name))
                {
                    continue;
                }

                if (method.Parameters.Any(p => string.IsNullOrWhiteSpace(p.Name) || string.IsNullOrWhiteSpace(p.Type)))
                {
                    continue;
                }

                var pair = method.Serialize();
                supportedMethods[pair.Key] = pair.Value;

                existingMethods.Remove(pair.Key);
            }

            foreach (var method in existingMethods)
            {
                supportedMethods[method] = null;
            }

            patch["SupportedMethods"] = supportedMethods;
        }

        public static void AddSupportedMethodsFromReportedProperty(DeviceModel device, Twin twin)
        {
            foreach (var pair in twin.Properties.Reported.AsEnumerableFlatten().Where(pair => IsSupportedMethodProperty(pair.Key)))
            {
                try
                {
                    var command = Command.Deserialize(
                        pair.Key.Substring(SupportedMethodsKey.Length + 1),
                        pair.Value.Value.Value.ToString());

                    if (!device.Commands.Any(c => c.Name == command.Name && c.DeliveryType == DeliveryType.Method))
                    {
                        device.Commands.Add(command);
                    }
                }
                catch (Exception ex)
                {
                    Trace.TraceError(FormattableString.Invariant($"Exception raised while deserializing method {pair.Key}: {ex}"));
                    continue;
                }
            }
        }

        public static bool IsSupportedMethodProperty(string propertyName)
        {
            return propertyName == SupportedMethodsKey || propertyName.StartsWith(SupportedMethodsKey + ".", StringComparison.Ordinal);
        }
    }
}
