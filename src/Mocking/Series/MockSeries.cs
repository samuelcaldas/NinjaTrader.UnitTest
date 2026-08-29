using System;
using System.Collections.Generic;

namespace NinjaTrader.UnitTest.Mocking
{
    /// <summary>
    /// Generic data series structure simulating NinjaTrader's Series&lt;T&gt; data structure.
    /// </summary>
    /// <typeparam name="T">The data type of the series elements.</typeparam>
    public class MockSeries<T> : ISeries<T>
    {
        private readonly List<T> _values = new List<T>();
        private readonly List<bool> _validity = new List<bool>();
        private readonly MockBarSeries _synchronizedBars;
        private int _currentBar = -1;

        public int Count => _synchronizedBars != null ? _synchronizedBars.Count : _values.Count;

        public int CurrentBar => _synchronizedBars != null ? _synchronizedBars.CurrentBar : _currentBar;

        public MockSeries(MockBarSeries synchronizedBars = null, int initialCapacity = 0)
        {
            _synchronizedBars = synchronizedBars;
            if (initialCapacity > 0)
            {
                for (int i = 0; i < initialCapacity; i++)
                {
                    _values.Add(default);
                    _validity.Add(false);
                }
                _currentBar = initialCapacity - 1;
            }
        }

        public T this[int barsAgo]
        {
            get
            {
                int index = ResolveAbsoluteIndex(barsAgo);
                return _values[index];
            }
            set
            {
                int index = ResolveAbsoluteIndex(barsAgo);
                _values[index] = value;
                _validity[index] = true;
            }
        }

        public void Set(T value)
        {
            this[0] = value;
        }

        public void Set(int barsAgo, T value)
        {
            this[barsAgo] = value;
        }

        public bool IsValidDataPoint(int barsAgo)
        {
            int index = ResolveAbsoluteIndex(barsAgo);
            return _validity[index];
        }

        public T GetValueAt(int index)
        {
            if (index < 0 || index >= _values.Count)
                throw new ArgumentOutOfRangeException(nameof(index), $"Index {index} is out of range (Count: {_values.Count})");

            return _values[index];
        }

        public void Reset(int barsAgo = 0)
        {
            int index = ResolveAbsoluteIndex(barsAgo);
            _values[index] = default;
            _validity[index] = false;
        }

        public void EnsureCapacity(int barIndex)
        {
            while (_values.Count <= barIndex)
            {
                _values.Add(default);
                _validity.Add(false);
            }
            if (_synchronizedBars == null)
            {
                _currentBar = barIndex;
            }
        }

        private int ResolveAbsoluteIndex(int barsAgo)
        {
            if (barsAgo < 0)
                throw new ArgumentOutOfRangeException(nameof(barsAgo), "barsAgo cannot be negative");

            int activeBar = CurrentBar;
            if (activeBar < 0 && _values.Count > 0)
                activeBar = _values.Count - 1;

            if (activeBar < 0)
                throw new InvalidOperationException("Series is empty. No bars have been loaded or processed.");

            EnsureCapacity(activeBar);

            int targetIndex = activeBar - barsAgo;
            if (targetIndex < 0 || targetIndex >= _values.Count)
                throw new ArgumentOutOfRangeException(nameof(barsAgo), $"barsAgo {barsAgo} exceeds available historical bars (CurrentBar: {activeBar})");

            return targetIndex;
        }
    }
}
