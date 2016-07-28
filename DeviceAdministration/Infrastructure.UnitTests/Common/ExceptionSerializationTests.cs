using Microsoft.Azure.Devices.Applications.RemoteMonitoring.Common.Exceptions;
using Microsoft.Azure.Devices.Applications.RemoteMonitoring.DeviceAdmin.Infrastructure.Exceptions;
using NUnit.Framework;
using System;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;

namespace Microsoft.Azure.Devices.Applications.RemoteMonitoring.DeviceAdmin.Infrastructure.UnitTests
{
    [TestFixture]
    public class ExceptionSerializationTests
    {
        [Test]
        public void TestDeviceRequiredPropertyNotFoundException()
        {
            var e = new DeviceRequiredPropertyNotFoundException("This is a test!!");
            TestSerialization(e);
        }

        [Test]
        public void TestDeviceRequiredPropertyNotFoundExceptionWithInner()
        {
            var eInner = new ArgumentNullException("Test");
            var e = new DeviceRequiredPropertyNotFoundException("This is a test!!", eInner);
            TestSerialization(e);
        }

        [Test]
        public void TestDeviceAlreadyRegisteredException()
        {
            var e = new DeviceAlreadyRegisteredException("1234");
            TestSerialization(e);
        }

        [Test]
        public void TestDeviceNotRegisteredException()
        {
            var e = new DeviceNotRegisteredException("1234");
            TestSerialization(e);
        }

        [Test]
        public void TestDeviceRegistrationException()
        {
            var innerE = new ArgumentOutOfRangeException("paramName", "test", "This is a test?");
            var e = new DeviceRegistrationException("1234", innerE);

            TestSerialization(e);
        }

        [Test]
        public void TestUnsupportedCommandException()
        {
            var e = new UnsupportedCommandException("1234", "DoSomething");

            TestSerialization(e);
        }
        
        [Test]
        public void TestValidationExceptionWithNoErrorsInList()
        {
            var e = new ValidationException("1234");

            TestSerialization(e);
        }

        [Test]
        public void TestValidationExceptionWithErrorsList()
        {
            var e = new ValidationException("1234");
            e.Errors.Add("Error One");
            e.Errors.Add("Error Two");

            TestSerialization(e);
        }

        [Test]
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
                eRoundTripped = (TException)formatter.Deserialize(stream);
            }

            Assert.AreEqual(eRoundTripped.ToString(), e.ToString());
        }
    }
}
