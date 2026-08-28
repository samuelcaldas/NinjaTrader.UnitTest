using System;
using System.Collections;
using System.Collections.Generic;

namespace NinjaTrader.UnitTest.Mocking
{
    /// <summary>
    /// Represents a single historical OHLCV bar for testing.
    /// </summary>
    public class MockBar
    {
        public DateTime Time { get; set; }
        public double Open { get; set; }
        public double High { get; set; }
        public double Low { get; set; }
        public double Close { get; set; }
        public long Volume { get; set; }

        public MockBar(DateTime time, double open, double high, double low, double close, long volume = 0)
        {
            Time = time;
            Open = open;
            High = high;
            Low = low;
            Close = close;
            Volume = volume;
        }

        public override string ToString()
        {
            return $"[{Time:yyyy-MM-dd HH:mm:ss}] O:{Open} H:{High} L:{Low} C:{Close} V:{Volume}";
        }
    }

    /// <summary>
    /// Collection of mock bars providing indexer access matching NinjaTrader bar indexing (0 = most recent, 1 = 1 bar ago).
    /// </summary>
    public class MockBarSeries : IEnumerable<MockBar>
    {
        private readonly List<MockBar> _bars = new List<MockBar>();

        public string InstrumentName { get; set; }
        public int Count => _bars.Count;
        public int CurrentBar => _bars.Count > 0 ? _bars.Count - 1 : -1;

        public MockBarSeries(string instrumentName = "MOCK_INSTRUMENT")
        {
            InstrumentName = instrumentName;
        }

        public void Add(MockBar bar)
        {
            if (bar == null) throw new ArgumentNullException(nameof(bar));
            _bars.Add(bar);
        }

        public void Add(DateTime time, double open, double high, double low, double close, long volume = 0)
        {
            _bars.Add(new MockBar(time, open, high, low, close, volume));
        }

        public MockBar this[int index] => _bars[index];

        /// <summary>
        /// Returns the Close price at barsAgo (0 is current bar, 1 is previous bar).
        /// </summary>
        public double Close(int barsAgo = 0)
        {
            int index = _bars.Count - 1 - barsAgo;
            if (index < 0 || index >= _bars.Count)
                throw new IndexOutOfRangeException($"barsAgo {barsAgo} is out of range for series with count {_bars.Count}");
            return _bars[index].Close;
        }

        /// <summary>
        /// Returns the Open price at barsAgo.
        /// </summary>
        public double Open(int barsAgo = 0)
        {
            int index = _bars.Count - 1 - barsAgo;
            if (index < 0 || index >= _bars.Count)
                throw new IndexOutOfRangeException($"barsAgo {barsAgo} is out of range for series with count {_bars.Count}");
            return _bars[index].Open;
        }

        /// <summary>
        /// Returns the High price at barsAgo.
        /// </summary>
        public double High(int barsAgo = 0)
        {
            int index = _bars.Count - 1 - barsAgo;
            if (index < 0 || index >= _bars.Count)
                throw new IndexOutOfRangeException($"barsAgo {barsAgo} is out of range for series with count {_bars.Count}");
            return _bars[index].High;
        }

        /// <summary>
        /// Returns the Low price at barsAgo.
        /// </summary>
        public double Low(int barsAgo = 0)
        {
            int index = _bars.Count - 1 - barsAgo;
            if (index < 0 || index >= _bars.Count)
                throw new IndexOutOfRangeException($"barsAgo {barsAgo} is out of range for series with count {_bars.Count}");
            return _bars[index].Low;
        }

        /// <summary>
        /// Returns the Volume at barsAgo.
        /// </summary>
        public long Volume(int barsAgo = 0)
        {
            int index = _bars.Count - 1 - barsAgo;
            if (index < 0 || index >= _bars.Count)
                throw new IndexOutOfRangeException($"barsAgo {barsAgo} is out of range for series with count {_bars.Count}");
            return _bars[index].Volume;
        }

        /// <summary>
        /// Returns the Time at barsAgo.
        /// </summary>
        public DateTime Time(int barsAgo = 0)
        {
            int index = _bars.Count - 1 - barsAgo;
            if (index < 0 || index >= _bars.Count)
                throw new IndexOutOfRangeException($"barsAgo {barsAgo} is out of range for series with count {_bars.Count}");
            return _bars[index].Time;
        }

        public IEnumerator<MockBar> GetEnumerator() => _bars.GetEnumerator();
        IEnumerator IEnumerable.GetEnumerator() => _bars.GetEnumerator();
    }
}
