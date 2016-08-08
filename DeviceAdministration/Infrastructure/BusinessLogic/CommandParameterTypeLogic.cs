using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;

namespace Microsoft.Azure.Devices.Applications.RemoteMonitoring.DeviceAdmin.Infrastructure.BusinessLogic
{
    public class CommandParameterTypeLogic : ICommandParameterTypeLogic
    {
        private static readonly Lazy<CommandParameterTypeLogic> commandParameterTypeLogic
             = new Lazy<CommandParameterTypeLogic>(() => new CommandParameterTypeLogic());

        private CommandParameterTypeLogic(){ }

        public static CommandParameterTypeLogic Instance
        {
            get
            {
                return commandParameterTypeLogic.Value;
            }
        }

        public bool IsValid(string typeName, object value)
        {
            return IsTypeValid(typeName, value);
        }

        [SuppressMessage(
            "Microsoft.Globalization", 
            "CA1308:NormalizeStringsToUppercase",
            Justification = "CommandTypes.Types' keys are all lower case English values.  CanTypeBeNull is likewise based on lower case English values.")]
        public object Get(string typeName, object value)
        {
            var lowerCaseTypeName = typeName.ToLowerInvariant();

            Type type;

            CommandTypes.Types.TryGetValue(lowerCaseTypeName, out type);

            if (value == null && CanTypeBeNull(lowerCaseTypeName))
            {
                return null;
            }

            string parameterValue = value.ToString();

            return CommandValueFactory(lowerCaseTypeName, parameterValue, type);
        }

        private bool CanTypeBeNull(string typeName)
        {
            if (typeName == "string" || typeName == "binary")
            {
                return true;
            }

            return false;
        }

        private object ReturnDateTimeOffset(object value)
        {
            DateTime datetime;

            var dateString = value.ToString();
            var isValid = DateTime.TryParse(dateString, out datetime);

            if (!isValid)
            {
                return null;
            }

            return datetime.ToUniversalTime();
        }

        [SuppressMessage(
            "Microsoft.Globalization", 
            "CA1308:NormalizeStringsToUppercase",
            Justification = "typeName-related logic works from lower-case English values.")]
        private bool IsTypeValid(string typeName, object value)
        {
            try
            {
                if (value == null && CanTypeBeNull(typeName.ToLowerInvariant()))
                {
                    return true;
                }

                var parsedValue = Get(typeName, value);

                return parsedValue != null;
            }
            catch
            {
                return false;
            }
        }

        private object CommandValueFactory(string typeName, string value, Type type)
        {
            switch (typeName)
            {
                case "datetimeoffset":
                    return ReturnDateTimeOffset(value);

                case "date":
                    return ParseDate(value);

                case "double":
                    return ParseDouble(value);

                case "guid":
                    return ParseGuid(value);

                case "int64":
                    return ParseInt64(value);

                case "decimal":
                    return ParseDecimal(value);

                case "binary":
                    return ParseBase64(value);


                default:
                    return Convert.ChangeType(
                        value, 
                        type, 
                        CultureInfo.CurrentCulture);
            }
        }

        private static object ParseBase64(string base64String)
        {
            var isBase64 = false;
            if (string.IsNullOrEmpty(base64String) ||
                base64String.Length%4 != 0 ||
                base64String.Contains(" ") ||
                base64String.Contains("\t") ||
                base64String.Contains("\r") ||
                base64String.Contains("\n"))
            {
                isBase64 = false;
            }

            // now do the real test
            try
            {
                Convert.FromBase64String(base64String);
                isBase64 = true;
            }
            catch (FormatException)
            {
                isBase64 = false;
            }

            return isBase64 ? base64String : null;
        }

        private static object ParseDecimal(string value)
        {
            decimal parsedValue;
            var isValid = decimal.TryParse(value, out parsedValue);
            if (!isValid)
            {
                return null;
            }
            return parsedValue.ToString(CultureInfo.InvariantCulture);
        }

        private static object ParseInt64(string value)
        {
            Int64 parsedValue;
            var isValid = Int64.TryParse(value, out parsedValue);
            if (!isValid)
            {
                return null;
            }
            return parsedValue.ToString(CultureInfo.InvariantCulture);
        }

        private object ParseGuid(string value)
        {
            Guid guid;
            var isValid = Guid.TryParse(value, out guid);

            if (!isValid)
            {
                return null;
            }

            return guid;
        }

        private object ParseDouble(string value)
        {
            double doubleValue;
            var isValid = double.TryParse(value, out doubleValue);

            if (!isValid)
            {
                return null;
            }

            return doubleValue;
        }

        private object ParseDate(string value)
        {
            DateTime datetime;
            bool isValid = DateTime.TryParse(value, out datetime);

            if (!isValid)
            {
                return null;
            }

            return datetime.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture);
        }
    }

    /// <summary>
    /// A class for translating command value types to managed framework ones.
    /// </summary>
    /// <remarks>
    /// It uses type names, normalized as lower-case.
    /// </remarks>
    public class CommandTypes
    {
        private static readonly ReadOnlyDictionary<string, Type> _typesBacking =
            new ReadOnlyDictionary<string, Type>(
                new Dictionary<string, Type>
                {
                    {"int16", typeof (Int16)},
                    {"int", typeof (Int32)},
                    {"int32", typeof (Int32)},
                    {"int64", typeof (Int64)},
                    {"sbyte", typeof (sbyte)},
                    {"byte", typeof (byte)},
                    {"double", typeof (double)},
                    {"boolean", typeof (bool)},
                    {"decimal", typeof (decimal)},
                    {"single", typeof (Single)},
                    {"guid", typeof (Guid)},
                    {"binary", typeof (string)},
                    {"string", typeof (string)},
                    {"date", typeof (DateTime)},
                    {"datetimeoffset", typeof (DateTime)}
                });

        public static ReadOnlyDictionary<string, Type> Types
        {
            get
            {
                return _typesBacking;
            }
        }
    }
}