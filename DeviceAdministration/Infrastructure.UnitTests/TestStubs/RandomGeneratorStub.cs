using Microsoft.Azure.Devices.Applications.RemoteMonitoring.Common.SampleDataGenerator;

namespace Microsoft.Azure.Devices.Applications.RemoteMonitoring.DeviceAdmin.Infrastructure.UnitTests.TestStubs
{
    public class RandomGeneratorStub : IRandomGenerator
    {
        private readonly double _valueReturned;

        public RandomGeneratorStub(double valueReturned)
        {
            _valueReturned = valueReturned;
        }

        public double GetRandomDouble()
        {
            return _valueReturned;
        }
    }
}