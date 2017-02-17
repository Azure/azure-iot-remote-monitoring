using System;

namespace Microsoft.Azure.Devices.Applications.RemoteMonitoring.DeviceAdmin.Infrastructure.Models
{
    /// <summary>
    /// Wraps error details to pass back to the caller of a WebAPI
    /// </summary>
    [Serializable()]
    public class Error
    {
        public enum ErrorType
        {
            Exception = 0,
            Validation = 1
        }

        public ErrorType Type { get; set; }
        public string Message { get; set; }
        public string ExceptionType { get; set; }

        public Error(Exception exception)
        {
            Type = ErrorType.Exception;
            Message = Strings.UnexpectedErrorOccurred;
            ExceptionType = exception.GetType().Name;
        }

        public Error(string validationError)
        {
            Type = ErrorType.Validation;
            Message = validationError;
        }
    }
}
