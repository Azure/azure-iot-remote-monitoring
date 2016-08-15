using System;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web.Http;
using GlobalResources;
using Microsoft.Azure.Devices.Applications.RemoteMonitoring.DeviceAdmin.Infrastructure.Exceptions;
using Microsoft.Azure.Devices.Applications.RemoteMonitoring.DeviceAdmin.Infrastructure.Models;
using Microsoft.Azure.Devices.Applications.RemoteMonitoring.DeviceAdmin.Web.Security;

namespace Microsoft.Azure.Devices.Applications.RemoteMonitoring.DeviceAdmin.Web.WebApiControllers
{
    [Authorize]
    [WebApiCSRFValidation]
    public abstract class WebApiControllerBase : ApiController
    {
        protected async Task<HttpResponseMessage> GetServiceResponseAsync(Func<Task> getData)
        {
            if (getData == null)
            {
                throw new ArgumentNullException("getData");
            }

            return await GetServiceResponseAsync<object>(async () =>
            {
                await getData();
                return null;
            });
        }

        /// <summary>
        /// Wraps the response from the getData call into a ServiceResponse object
        /// If an exception is thrown it is caught and put into the Error property of the service response
        /// </summary>
        /// <typeparam name="T">Type returned by the getData call</typeparam>
        /// <param name="getData">Lambda to actually take the action of retrieving the data from the business logic layer</param>
        /// <returns></returns>
        protected async Task<HttpResponseMessage> GetServiceResponseAsync<T>(Func<Task<T>> getData)
        {
            if (getData == null)
            {
                throw new ArgumentNullException("getData");
            }

            return await GetServiceResponseAsync(getData, true);
        }

        /// <summary>
        /// Wraps the response from the getData call into a ServiceResponse object
        /// If an exception is thrown it is caught and put into the Error property of the service response
        /// </summary>
        /// <typeparam name="T">Type returned by the getData call</typeparam>
        /// <param name="getData">Lambda to actually take the action of retrieving the data from the business logic layer</param>
        /// <param name="useServiceResponse">Returns a service response wrapping the data in a Data property in the response, this is ignored if there is an error</param>
        /// <returns></returns>
        protected async Task<HttpResponseMessage> GetServiceResponseAsync<T>(Func<Task<T>> getData, bool useServiceResponse)
        {
            ServiceResponse<T> response = new ServiceResponse<T>();

            if (getData == null)
            {
                throw new ArgumentNullException("getData");
            }

            try
            {
                response.Data = await getData();
            }
            catch (ValidationException ex)
            {
                if (ex.Errors == null || ex.Errors.Count == 0)
                {
                    response.Error.Add(new Error("Unknown validation error"));
                }
                else
                {
                    foreach (string error in ex.Errors)
                    {
                        response.Error.Add(new Error(error));
                    }
                }
            }
            catch (DeviceAdministrationExceptionBase ex)
            {
                response.Error.Add(new Error(ex.Message));
            }
            catch (HttpResponseException)
            {
                throw;
            }
            catch (Exception ex)
            {
                response.Error.Add(new Error(ex));
                Debug.Write(FormatExceptionMessage(ex), " GetServiceResponseAsync Exception");
            }

            // if there's an error or we've been asked to use a service response, then return a service response
            if (response.Error.Count > 0 || useServiceResponse)
            {
                return Request.CreateResponse(
                        response.Error != null && response.Error.Any() ? HttpStatusCode.BadRequest : HttpStatusCode.OK,
                        response);
            }

            // otherwise there's no error and we need to return the data at the root of the response
            return Request.CreateResponse(HttpStatusCode.OK, response.Data);
        }

        protected HttpResponseMessage GetNullRequestErrorResponse<T>()
        {
            ServiceResponse<T> response = new ServiceResponse<T>();
            response.Error.Add(new Error(Strings.RequestNullError));

            return Request.CreateResponse(HttpStatusCode.BadRequest, response);
        }

        protected HttpResponseMessage GetFormatErrorResponse<T>(string parameterName, string type)
        {
            ServiceResponse<T> response = new ServiceResponse<T>();

            string errorMessage =
                String.Format(
                    CultureInfo.CurrentCulture,
                    Strings.RequestFormatError,
                    parameterName, type);

            response.Error.Add(new Error(errorMessage));

            return Request.CreateResponse(HttpStatusCode.BadRequest, response);
        }

        protected void TerminateProcessingWithMessage(HttpStatusCode statusCode, string message)
        {
            HttpResponseMessage responseMessage = new HttpResponseMessage()
            {
                StatusCode = statusCode,
                ReasonPhrase  = message
            };

            throw new HttpResponseException(responseMessage);
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Globalization", "CA1303:Do not pass literals as localized parameters", MessageId = "Microsoft.Azure.Devices.Applications.RemoteMonitoring.DeviceAdmin.Web.WebApiControllers.WebApiControllerBase.TerminateProcessingWithMessage(System.Net.HttpStatusCode,System.String)")]
        protected void ValidateArgumentNotNullOrWhitespace(string argumentName, string value)
        {
            Debug.Assert(
                !string.IsNullOrWhiteSpace(argumentName),
                "argumentName is a null reference, empty string, or contains only whitespace.");

            if (string.IsNullOrWhiteSpace(value))
            {
                // Error strings are not localized.
                string errorText =
                    string.Format(
                        CultureInfo.InvariantCulture,
                        "{0} is null, empty, or just whitespace.",
                        argumentName);

                TerminateProcessingWithMessage(HttpStatusCode.BadRequest, errorText);
            }
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Globalization", "CA1303:Do not pass literals as localized parameters", MessageId = "Microsoft.Azure.Devices.Applications.RemoteMonitoring.DeviceAdmin.Web.WebApiControllers.WebApiControllerBase.TerminateProcessingWithMessage(System.Net.HttpStatusCode,System.String)")]
        protected void ValidateArgumentNotNull(string argumentName, object value)
        {
            Debug.Assert(
                !string.IsNullOrWhiteSpace(argumentName),
                "argumentName is a null reference, empty string, or contains only whitespace.");

            if (value == null)
            {
                // Error strings are not localized.
                string errorText = string.Format(CultureInfo.InvariantCulture, "{0} is a null reference.", argumentName);

                TerminateProcessingWithMessage(HttpStatusCode.BadRequest, errorText);
            }
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Globalization", "CA1303:Do not pass literals as localized parameters", MessageId = "Microsoft.Azure.Devices.Applications.RemoteMonitoring.DeviceAdmin.Web.WebApiControllers.WebApiControllerBase.TerminateProcessingWithMessage(System.Net.HttpStatusCode,System.String)")]
        protected void ValidatePositiveValue(string argumentName, int value)
        {
            Debug.Assert(
                !string.IsNullOrWhiteSpace(argumentName),
                "argumentName is a null reference, empty string, or contains only whitespace.");

            if (value <= 0)
            {
                // Error strings are not localized.
                string errorText =
                    string.Format(
                        CultureInfo.InvariantCulture,
                        "{0} is not a positive integer.",
                        argumentName);

                TerminateProcessingWithMessage(HttpStatusCode.BadRequest, errorText);
            }
        }

        private static string FormatExceptionMessage(Exception ex)
        {
            Debug.Assert(ex != null, "ex is a null reference.");

            // Error strings are not localized
            return string.Format(
                CultureInfo.CurrentCulture,
                "{0}{0}*** EXCEPTION ***{0}{0}{1}{0}{0}",
                Console.Out.NewLine,
                ex);
        }
    }
}