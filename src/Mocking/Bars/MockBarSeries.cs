using System;
using System.Collections;
using System.Collections.Generic;

namespace NinjaTrader.UnitTest.Mocking
{
    /// <summary>
    /// Collection of mock bars providing indexer access matching NinjaTrader bar indexing (0 = most recent, 1 = 1 bar ago).
    /// </summary>
    public class MockBarSeries : IEnumerable<MockBar>
    {
        private readonly List<MockBar> _bars = new List<MockBar>();

        public string InstrumentName { get; set; }
        public MockBarsPeriodType PeriodType { get; set; } = MockBarsPeriodType.Minute;
        public int PeriodValue { get; set; } = 1;

        public int Count => _bars.Count;
        public int CurrentBar => _bars.Count > 0 ? _bars.Count - 1 : -1;

        public MockBarSeries(string instrumentName = "MOCK_INSTRUMENT", MockBarsPeriodType periodType = MockBarsPeriodType.Minute, int periodValue = 1)
        {
            InstrumentName = instrumentName;
            PeriodType = periodType;
            PeriodValue = periodValue;
        }

        public void Add(MockBar bar)
        {
            if (bar == null)
                throw new ArgumentNullException(nameof(bar));

            _bars.Add(bar);
        }

        public void Add(DateTime time, double open, double high, double low, double close, long volume = 0)
        {
            _bars.Add(new MockBar(time, open, high, low, close, volume));
        }

        public MockBar this[int index] => _bars[index];

        public MockBar GetBarAt(int index)
        {
            if (index < 0 || index >= _bars.Count)
                throw new ArgumentOutOfRangeException(nameof(index), $"Index {index} is out of range for series with count {_bars.Count}");

            return _bars[index];
        }

        public double Close(int barsAgo = 0) => GetBar(barsAgo).Close;
        public double Open(int barsAgo = 0) => GetBar(barsAgo).Open;
        public double High(int barsAgo = 0) => GetBar(barsAgo).High;
        public double Low(int barsAgo = 0) => GetBar(barsAgo).Low;
        public long Volume(int barsAgo = 0) => GetBar(barsAgo).Volume;
        public DateTime Time(int barsAgo = 0) => GetBar(barsAgo).Time;

        public IEnumerable<double> GetIntrabarTicks(int barsAgo = 0)
        {
            var bar = GetBar(barsAgo);
            if (bar.Close >= bar.Open)
            {
                return new[] { bar.Open, bar.Low, bar.High, bar.Close };
            }
            return new[] { bar.Open, bar.High, bar.Low, bar.Close };
        }

        public IEnumerator<MockBar> GetEnumerator() => _bars.GetEnumerator();
        IEnumerator IEnumerable.GetEnumerator() => _bars.GetEnumerator();

        private MockBar GetBar(int barsAgo)
        {
            int index = _bars.Count - 1 - barsAgo;
            if (index < 0 || index >= _bars.Count)
                throw new IndexOutOfRangeException($"barsAgo {barsAgo} is out of range for series with count {_bars.Count}");

            return _bars[index];
        }
    }
}
