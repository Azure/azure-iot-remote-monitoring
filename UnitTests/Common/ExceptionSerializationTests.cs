using System;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using Microsoft.Azure.Devices.Applications.RemoteMonitoring.Common.Exceptions;
using Microsoft.Azure.Devices.Applications.RemoteMonitoring.DeviceAdmin.Infrastructure.Exceptions;
using Xunit;

namespace Microsoft.Azure.Devices.Applications.RemoteMonitoring.UnitTests.Common
{
    public class ExceptionSerializationTests
    {
        [Fact]
        public void TestDeviceRequiredPropertyNotFoundException()
        {
            var e = new DeviceRequiredPropertyNotFoundException("This is a test!!");
            TestSerialization(e);
        }

        [Fact]
        public void TestDeviceRequiredPropertyNotFoundExceptionWithInner()
        {
            var eInner = new ArgumentNullException("Test");
            var e = new DeviceRequiredPropertyNotFoundException("This is a test!!", eInner);
            TestSerialization(e);
        }

        [Fact]
        public void TestDeviceAlreadyRegisteredException()
        {
            var e = new DeviceAlreadyRegisteredException("1234");
            TestSerialization(e);
        }

        [Fact]
        public void TestDeviceNotRegisteredException()
        {
            var e = new DeviceNotRegisteredException("1234");
            TestSerialization(e);
        }

        [Fact]
        public void TestDeviceRegistrationException()
        {
            var innerE = new ArgumentOutOfRangeException("paramName", "test", "This is a test?");
            var e = new DeviceRegistrationException("1234", innerE);

            TestSerialization(e);
        }

        [Fact]
        public void TestUnsupportedCommandException()
        {
            var e = new UnsupportedCommandException("1234", "DoSomething");

            TestSerialization(e);
        }

        [Fact]
        public void TestValidationExceptionWithNoErrorsInList()
        {
            var e = new ValidationException("1234");

            TestSerialization(e);
        }

        [Fact]
        public void TestValidationExceptionWithErrorsList()
        {
            var e = new ValidationException("1234");
            e.Errors.Add("Error One");
            e.Errors.Add("Error Two");

            TestSerialization(e);
        }

        [Fact]
        public void TestValidationExceptionWithInnerException()
        {
            var eInner = new InvalidOperationException("Whoops!");
            var e = new ValidationException("1234", eInner);
            e.Errors.Add("Error One");
            e.Errors.Add("Error Two");

            TestSerialization(e);
        }

        // Serializes and deserializes an exception, then compares the .ToString() to ensure
        // it did not change
        private void TestSerialization<TException>(TException e) where TException : Exception
        {
            TException eRoundTripped = null;
            var formatter = new BinaryFormatter();
            using (var stream = new MemoryStream())
            {
                // serialize exception into stream
                formatter.Serialize(stream, e);

                // put the stream back to the start
                stream.Seek(0, 0);

                // now deserialize into a new object
                eRoundTripped = (TException) formatter.Deserialize(stream);
            }

            Assert.Equal(eRoundTripped.ToString(), e.ToString());
        }
    }
}