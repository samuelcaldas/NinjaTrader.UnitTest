using System;

namespace NinjaTrader.UnitTest.Mocking
{
    /// <summary>
    /// Event arguments for simulating NinjaTrader's OnMarketDepth Level 2 book updates.
    /// </summary>
    public class MockMarketDepthEventArgs : EventArgs
    {
        public MockMarketDataType MarketDataType { get; set; }
        public MockMarketDepthOperation Operation { get; set; }
        public double Price { get; set; }
        public long Volume { get; set; }
        public int Position { get; set; }
        public string MarketMaker { get; set; }
        public DateTime Time { get; set; }
        public MockInstrument Instrument { get; set; }

        public MockMarketDepthEventArgs(
            MockMarketDataType marketDataType,
            MockMarketDepthOperation operation,
            double price,
            long volume,
            int position,
            string marketMaker = "",
            DateTime? time = null,
            MockInstrument instrument = null)
        {
            MarketDataType = marketDataType;
            Operation = operation;
            Price = price;
            Volume = volume;
            Position = position;
            MarketMaker = marketMaker;
            Time = time ?? DateTime.Now;
            Instrument = instrument;
        }
    }
}
