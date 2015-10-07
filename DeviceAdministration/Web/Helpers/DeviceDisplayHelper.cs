using System;
using System.Collections.Generic;
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
        #region Static Variables

        private static readonly HashSet<string> CopyControlDeviceProperties =
            new HashSet<string>(
                new string[] {
                    "DEVICEID",
                    "HOSTNAME"
                });

        #endregion

        #region Public Methods

        #region Static Method: GetIsCopyControlPropertyName

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

        #endregion

        #region Static Method: GetLocalizedCommandResultText

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
            string errorMessage;
            FeedbackStatusCode resolvedValue;

            errorMessage = viewStateErrorMessage as string;
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

        #endregion

        #endregion
    }
}