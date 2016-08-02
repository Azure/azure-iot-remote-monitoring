using System;
using System.Collections;
using System.Collections.Generic;
using Microsoft.Azure.Devices.Applications.RemoteMonitoring.Common.Helpers;
using Microsoft.Azure.Devices.Applications.RemoteMonitoring.Common.Models.Commands;
using Newtonsoft.Json.Linq;
using Microsoft.Azure.Devices.Applications.RemoteMonitoring.Common.Models;

namespace Microsoft.Azure.Devices.Applications.RemoteMonitoring.Common.DeviceSchema
{
    /// <summary>
    /// Helper class to encapsulate interactions with the command schema.
    /// 
    /// </summary>
    public static class CommandSchemaHelper
    {
        /// <summary>
        /// Retrieve from a device the commands that it can perform
        /// </summary>
        /// <param name="device"></param>
        /// <returns></returns>
        public static List<Command> GetSupportedCommands(DeviceModel device)
        {
            if (device == null)
            {
                throw new ArgumentNullException("device");
            }

            List<Command> commands = device.Commands;

            if (commands == null)
            {
                commands = new List<Command>();
                device.Commands = commands;
            }

            return commands;
        }

        /// <summary>
        /// Build up a new command object based on the provided name.
        /// </summary>
        /// <param name="command"></param>
        /// <returns></returns>
        public static Command CreateNewCommand(string command)
        {
            Command result = new Command();

            result.Name = command;
            result.Parameters = null;

            return result;
        }

        /// <summary>
        /// Create a new Telemetry type
        /// </summary>
        /// <param name="name">Name of telemetry object</param>
        /// <param name="displayName">Name to display when referencing the telemetry field to the end user</param>
        /// <param name="type">Value type of telemetry object</param>
        /// <returns></returns>
        public static Telemetry CreateNewTelemetry(string name, string displayName, string type)
        {
            Telemetry result = new Telemetry();
            result.Name = name;
            result.DisplayName = displayName;
            result.Type = type;


            return result;
        }

        /// <summary>
        /// Add a parameter value definition to an existing command. This is not the use of a parameter for a command being sent to the
        /// device, but the definition of a parameter with its names and the data type that it is meant to accept
        /// </summary>
        /// <param name="command"></param>
        /// <param name="name"></param>
        /// <param name="type"></param>
        public static void DefineNewParameterOnCommand(Command command, string name, string type)
        {
            List<Parameter> foundParams = command.Parameters;
            if (foundParams == null)
            {
                foundParams = new List<Parameter>();
                command.Parameters = foundParams;
            }

            Parameter newParam = new Parameter();
            newParam.Name = name;
            newParam.Type = type;
            foundParams.Add(newParam);
        }

        /// <summary>
        /// Looks through the supported commands to see if the requested command is supported by the device
        /// </summary>
        /// <param name="device">Device to check</param>
        /// <param name="commandName">Name of commmand to check to see if the device supports</param>
        /// <returns>True if device can perform command, false if it cannot</returns>
        public static bool CanDevicePerformCommand(DeviceModel device, string commandName)
        {
            List<Command> commands;

            if (device == null)
            {
                return false;
            }

            commands = device.Commands;

            if (commands == null)
            {
                return false;
            }

            foreach (Command command in commands)
            {
                if (command != null && command.Name == commandName)
                {
                    return true;
                }
            }

            return false;
        }

        /// <summary>
        /// This method will add the provided telemetry to the provided device.
        /// </summary>
        /// <param name="device">device object</param>
        /// <param name="telemetry">telemetry to add</param>
        public static void AddTelemetryToDevice(DeviceModel device, Telemetry telemetry)
        {

            if (device.Telemetry == null)
            {
                device.Telemetry = new List<Telemetry>();
            }


            device.Telemetry.Add(telemetry);
        }

        /// <summary>
        /// This method will add the provided command to the provided device. If the underlying infrastructure needs
        /// to be built it will be handled.
        /// </summary>
        /// <param name="device"></param>
        /// <param name="command"></param>
        public static void AddCommandToDevice(DeviceModel device, dynamic command)
        {
            List<Command> commands = GetSupportedCommands(device);
            commands.Add(command);
        }
    }
}
