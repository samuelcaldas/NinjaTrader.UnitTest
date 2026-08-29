using System;
using System.Collections;
using System.Collections.Generic;

namespace NinjaTrader.UnitTest.Mocking
{
    /// <summary>
    /// Collection of data series simulating NinjaTrader's BarsArray for multi-timeframe and multi-instrument scripts.
    /// </summary>
    public class MockBarsArray : IReadOnlyList<MockBarSeries>
    {
        private readonly List<MockBarSeries> _series = new List<MockBarSeries>();

        public MockBarSeries this[int index]
        {
            get
            {
                if (index < 0 || index >= _series.Count)
                    throw new ArgumentOutOfRangeException(nameof(index), $"BarsArray index {index} is out of range (Count: {_series.Count})");

                return _series[index];
            }
        }

        public int Count => _series.Count;

        public MockBarSeries Primary => _series.Count > 0 ? _series[0] : null;

        public MockBarsArray() { }

        public MockBarsArray(params MockBarSeries[] series)
        {
            if (series != null)
            {
                _series.AddRange(series);
            }
        }

        public void Add(MockBarSeries series)
        {
            if (series == null)
                throw new ArgumentNullException(nameof(series));

            _series.Add(series);
        }

        public IEnumerator<MockBarSeries> GetEnumerator() => _series.GetEnumerator();

        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
    }
}
