using System;
using System.Security.Cryptography;

namespace Microsoft.Azure.Devices.Applications.RemoteMonitoring.Common.SampleDataGenerator
{
    public class RandomGenerator : IRandomGenerator
    {
        private readonly Random _random;

        public RandomGenerator()
        {
            byte[] vector = new byte[4];
            using (RandomNumberGenerator cryptoRng = RandomNumberGenerator.Create())
            {
                cryptoRng.GetBytes(vector);
            }

            int seed = 0;
            for (int i = 0 ; i < vector.Length ; ++i)
            {
                seed |= vector[i];
                seed = seed << 8;
            }

            _random = new Random(seed);
        }

        public double GetRandomDouble()
        {
            return _random.NextDouble();
        }
    }
}