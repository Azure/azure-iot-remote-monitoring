using System;

namespace Microsoft.Azure.Devices.Applications.RemoteMonitoring.Common.SampleDataGenerator
{
    /// <summary>
    /// Generates simple random data for simulator
    /// </summary>
    public class SampleDataGenerator
    {
        // Limit changes to a percentage of the total range per tick -- feel free to change
        private const double _maximumFractionToChangePerTick = 0.10;

        private double _minValueToGenerate;
        private double _maxNonPeakValueToGenerate;
        private double _minPeakValueToGenerate;

        private readonly bool _generatePeaks;
        private readonly int _peakInterval;

        private double _startValue;
        private double _nextValue;

        private readonly double _deltaValue;
        private double _thresholdWidth;
        private readonly IRandomGenerator _randomGenerator;
        private long _tickCounter;
        object sync = new Object();

        public SampleDataGenerator(double minValueToGenerate,
            double maxNonPeakValueToGenerate, double minPeakValueToGenerate,
            int peakInterval, IRandomGenerator randomGenerator)
        {
            if (minValueToGenerate >= maxNonPeakValueToGenerate)
            {
                throw new ArgumentOutOfRangeException(
                    "maxNonPeakValueToGenerate",
                    maxNonPeakValueToGenerate,
                    "maxNonPeakValueToGenerate must be greater than minValueToGenerate.");
            }

            if ((minPeakValueToGenerate != 0) &&
                (maxNonPeakValueToGenerate >= minPeakValueToGenerate))
            {
                throw new ArgumentOutOfRangeException(
                    "minPeakValueToGenerate",
                    minPeakValueToGenerate,
                    "If not 0, minPeakValueToGenerate must be greater than maxNonPeakValueToGenerate.");

            }

            // minPeakValueToGenerate is zero when peaks are not generated
            _generatePeaks = minPeakValueToGenerate != 0 ? true : false;

            if (_generatePeaks && peakInterval == 0)
            {
                throw new ArgumentOutOfRangeException("peakInterval", "peakInterval cannot be 0.");
            }

            if (randomGenerator == null)
            {
                throw new ArgumentNullException("randomGenerator");
            }

            _minValueToGenerate = minValueToGenerate;
            _maxNonPeakValueToGenerate = maxNonPeakValueToGenerate;

            // Scale up to ensure we exceed rather than equal threshold in all cases
            _minPeakValueToGenerate = minPeakValueToGenerate * 1.01;

            _peakInterval = peakInterval;

            // Start in the middle of the range
            _startValue = ((_maxNonPeakValueToGenerate - _minValueToGenerate) / 2.0) + _minValueToGenerate;
            _nextValue = _startValue;

            _deltaValue = (_maxNonPeakValueToGenerate - _minValueToGenerate) * _maximumFractionToChangePerTick;
            _thresholdWidth = (_minPeakValueToGenerate - _minValueToGenerate);

            _tickCounter = 1;

            _randomGenerator = randomGenerator;

        }

        public SampleDataGenerator(double minValueToGenerate, double maxNonPeakValueToGenerate,
            double minPeakValueToGenerate, int peakInterval)
            : this(minValueToGenerate, maxNonPeakValueToGenerate, minPeakValueToGenerate,
            peakInterval, new RandomGenerator())
        {
        }

        public SampleDataGenerator(double minValueToGenerate, double maxNonPeakValueToGenerate,
            IRandomGenerator randomGenerator)
            : this(minValueToGenerate, maxNonPeakValueToGenerate, 0, 0, randomGenerator)
        {
        }

        public SampleDataGenerator(double minValueToGenerate, double maxNonPeakValueToGenerate)
            : this(minValueToGenerate, maxNonPeakValueToGenerate, 0, 0, new RandomGenerator())
        {
        }

        public double GetNextValue()
        {
            GetNextRawValue();
            bool determinePeak = _generatePeaks && (_tickCounter % _peakInterval) == 0;
            ++_tickCounter;
            if (determinePeak)
            {
                return _nextValue + _thresholdWidth;
            }
            return _nextValue;
        }

        /// <summary>
        /// Shift all subsequent data to a new mid-point 
        /// (shifts existing ranges and peaks to the new value)
        /// </summary>
        /// <param name="newMidPointOfRange">New mid-point in the expected data range</param>
        public void ShiftSubsequentData(double newMidPointOfRange)
        {
            lock (sync)
            {
                _nextValue = newMidPointOfRange;

                double shift = _startValue - _minValueToGenerate;

                _startValue = newMidPointOfRange;

                _minValueToGenerate = newMidPointOfRange - shift;
                _maxNonPeakValueToGenerate = newMidPointOfRange + shift;
            }
        }

        public double GetMidPointOfRange()
        {
            return (_minValueToGenerate + _maxNonPeakValueToGenerate) / 2;
        }

        private void GetNextRawValue()
        {
            double adjustment = ((2.0 * _deltaValue) * _randomGenerator.GetRandomDouble()) - _deltaValue;
            _nextValue += adjustment;
            if (_nextValue < _minValueToGenerate || _nextValue > _maxNonPeakValueToGenerate)
            {
                _nextValue -= adjustment;

                // in case of cosmic ray or memory issues (or bugs), check and make sure we're back in the correct range, and fix if otherwise
                if (_nextValue < _minValueToGenerate || _nextValue > _maxNonPeakValueToGenerate)
                {
                    _nextValue = _startValue;
                }
            }
        }
    }
}