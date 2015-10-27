using System;
using System.Security.Cryptography;

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
            byte[] vector = new byte[4];
            using (RandomNumberGenerator cryptoRng = RandomNumberGenerator.Create())
            {
                cryptoRng.GetBytes(vector);
            }

            int seed = 0;
            for (int i = 0; i < vector.Length; ++i)
            {
                seed |= vector[i];
                seed = seed << 8;
            }

            return new Random(seed);
        }
    }
}