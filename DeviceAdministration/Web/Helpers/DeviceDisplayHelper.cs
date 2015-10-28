using System;
using System.Collections.Generic;
using System.Globalization;
using GlobalResources;
using Microsoft.Azure.Devices;

namespace Microsoft.Azure.Devices.Applications.RemoteMonitoring.DeviceAdmin.Web.Helpers
{
    using StringPair = KeyValuePair<string, string>;

    /// <summary>
    /// Static Methods Related to Displaying Devices
    /// </summary>
    public static class DeviceDisplayHelper
    {
        private static readonly HashSet<string> CopyControlDeviceProperties =
            new HashSet<string>(
                new string[] {
                    "DEVICEID",
                    "HOSTNAME"
                });

        /// <summary>
        /// Gets a value indicating whether a named Device property should be 
        /// represented with a CopyControl.
        /// </summary>
        /// <param name="propertyName">
        /// The name of the property.
        /// </param>
        /// <returns>
        /// A value indicating whether the Device property, named by 
        /// <paramref name="propertyName" />, should be represented with a 
        /// CopyControl.
        /// </returns>
        public static bool GetIsCopyControlPropertyName(string propertyName)
        {
            if (string.IsNullOrEmpty(propertyName))
            {
                return false;
            }

            return CopyControlDeviceProperties.Contains(propertyName.ToUpperInvariant());
        }

        /// <summary>
        /// Gets localized text and error text for a Device Command's Result.
        /// </summary>
        /// <param name="commandResult">
        /// The Device Command's Result.
        /// </param>
        /// <param name="viewStateErrorMessage">
        /// The ViewState-provided error message text.
        /// </param>
        /// <returns>
        /// Localized text and error text for a Device Command's Result, with 
        /// text as Key and error text as Value.
        /// </returns>
        public static StringPair GetLocalizedCommandResultText(
            string commandResult,
            object viewStateErrorMessage)
        {
            FeedbackStatusCode resolvedValue;

            var errorMessage = viewStateErrorMessage as string;
            if (Enum.TryParse<FeedbackStatusCode>(
                    commandResult,
                    out resolvedValue))
            {
                switch (resolvedValue)
                {
                    case FeedbackStatusCode.DeliveryCountExceeded:
                        errorMessage = Strings.CommandDeliveryCountExceeded;
                        commandResult = "Error";
                        break;

                    case FeedbackStatusCode.Expired:
                        errorMessage = Strings.CommandExpired;
                        commandResult = "Error";
                        break;

                    case FeedbackStatusCode.Rejected:
                        errorMessage = Strings.CommandRejected;
                        commandResult = "Error";
                        break;

                    case FeedbackStatusCode.Success:
                        errorMessage = string.Empty;
                        commandResult = Strings.CommandSuccess;
                        break;
                }
            }

            return new StringPair(commandResult, errorMessage);
        }

        /// <summary>
        /// Gets the local culture resource for a Device Property field name.
        /// </summary>
        /// <param name="fieldName">
        /// The name of the field for which a local culture resource should be 
        /// returned.
        /// </param>
        /// <returns>
        /// The local culture resource for the Device Property field, named by 
        /// <paramref name="fieldName" />, or <paramref name="fieldName" />,
        /// if no such resource can be found.
        /// </returns>
        public static string GetDevicePropertyFieldLocalName(string fieldName)
        {
            if (string.IsNullOrWhiteSpace(fieldName))
            {
                return fieldName;
            }

            string resourceName = string.Format(CultureInfo.InvariantCulture, "DeviceProperty_{0}", fieldName);
            string resourceValue = Strings.ResourceManager.GetString(resourceName);

            return resourceValue ?? fieldName;
        }
    }
}