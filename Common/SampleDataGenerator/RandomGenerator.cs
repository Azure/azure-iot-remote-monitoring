using System;

namespace Microsoft.Azure.Devices.Applications.RemoteMonitoring.Common.SampleDataGenerator
{
    public class RandomGenerator : IRandomGenerator
    {
        private static readonly Random _random = BuildRandomSource();

        public RandomGenerator()
        {
        }

        public double GetRandomDouble()
        {
            lock (_random)
            {
                return _random.NextDouble();
            }
        }

        private static Random BuildRandomSource()
        {
            return new Random(Guid.NewGuid().GetHashCode());
        }
    }
}