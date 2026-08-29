using System;

namespace NinjaTrader.UnitTest.Mocking
{
    /// <summary>
    /// Event arguments for simulating NinjaTrader's OnMarketData event handler.
    /// </summary>
    public class MockMarketDataEventArgs : EventArgs
    {
        public MockMarketDataType MarketDataType { get; set; }
        public double Price { get; set; }
        public long Volume { get; set; }
        public DateTime Time { get; set; }
        public MockInstrument Instrument { get; set; }

        public MockMarketDataEventArgs(MockMarketDataType marketDataType, double price, long volume, DateTime? time = null, MockInstrument instrument = null)
        {
            MarketDataType = marketDataType;
            Price = price;
            Volume = volume;
            Time = time ?? DateTime.Now;
            Instrument = instrument;
        }
    }
}
