using System;

namespace NinjaTrader.UnitTest.Mocking
{
    /// <summary>
    /// Represents a single historical OHLCV bar for unit testing.
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
}
