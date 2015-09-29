using System;

namespace Microsoft.Azure.Devices.Applications.RemoteMonitoring.Common.SampleDataGenerator
{
    public class RandomGenerator : IRandomGenerator
    {
        private readonly Random _random;

        public RandomGenerator()
        {
            _random = new Random();
        }

        public double GetRandomDouble()
        {
            return _random.NextDouble();
        }
    }
}