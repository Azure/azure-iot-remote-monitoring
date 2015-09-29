using System;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.Azure.Devices.Common.Exceptions;

namespace Microsoft.Azure.Devices.Applications.RemoteMonitoring.Common.Helpers
{
    /// <summary>
    /// Static helper class that implement retry logic around async operations by running the
    /// operation additional times on transient errors such as a network failure
    /// </summary>
    public static class AzureRetryHelper
    {
        private const int RETRY_COUNT = 2;

        /// <summary>
        /// Wrapper for the generic method for async operations that don't return a value
        /// </summary>
        /// <param name="asyncOperation"></param>
        /// <returns></returns>
        public static async Task OperationWithBasicRetryAsync(Func<Task> asyncOperation)
        {
            await OperationWithBasicRetryAsync<object>(async () =>
            {
                await asyncOperation();
                return null;
            });
        }

        /// <summary>
        /// Main generic method to perform the supplied async method with multiple retires on transient exceptions/errors
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="asyncOperation"></param>
        /// <returns></returns>
        public static async Task<T> OperationWithBasicRetryAsync<T>(Func<Task<T>> asyncOperation)
        {
            int currentRetry = 0;

            while(true)
            {
                try
                {
                    return await asyncOperation();
                }
                catch (Exception ex)
                {
                    currentRetry++;

                    if (currentRetry > RETRY_COUNT || !IsTransient(ex))
                    {
                        // If this is not a transient error or we should not retry re-throw the exception. 
                        throw;
                    }
                }

                // Wait to retry the operation.  
                await Task.Delay(100 * currentRetry);
            }
        }

        /// <summary>
        /// Checks if the provided exception is considered transient in nature or not
        /// Transient include issues such as a single failed network attempt
        /// </summary>
        /// <param name="originalException"></param>
        /// <returns></returns>
        private static bool IsTransient(Exception originalException)
        {
            // If the exception is a IotHubException its IsTransient property can be inspected
            IotHubException iotHubException = originalException as IotHubException;
            if (iotHubException != null)
            {
                return iotHubException.IsTransient;
            }

            // If the exception is an HTTP request exception then assume it is transient
            HttpRequestException httpException = originalException as HttpRequestException;
            if (httpException != null)
            {
                return true;
            }

            WebException webException = originalException as WebException;
            if (webException != null)
            {
                // If the web exception contains one of the following status values  it may be transient.
                return new[]
                {
                    WebExceptionStatus.ConnectionClosed, 
                    WebExceptionStatus.Timeout, 
                    WebExceptionStatus.RequestCanceled
                }.Contains(webException.Status);
            }

            return false;
        }
    }
}
