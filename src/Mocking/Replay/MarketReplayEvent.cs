using System;

namespace NinjaTrader.UnitTest.Mocking
{
    /// <summary>
    /// Represents a discrete market replay event (Tick, Bar, or Level 2 Depth update).
    /// </summary>
    public class MarketReplayEvent
    {
        public DateTime Time { get; set; }
        public MarketReplayRecordType RecordType { get; set; }
        public MockMarketDataEventArgs MarketData { get; set; }
        public MockMarketDepthEventArgs MarketDepth { get; set; }
        public MockBar Bar { get; set; }
        public double Price { get; set; }
        public double BidPrice { get; set; }
        public double AskPrice { get; set; }
        public long Volume { get; set; }

        public static MarketReplayEvent CreateTick(DateTime time, double price, long volume, MockInstrument instrument = null)
        {
            return new MarketReplayEvent
            {
                Time = time,
                RecordType = MarketReplayRecordType.Tick,
                Price = price,
                Volume = volume,
                MarketData = new MockMarketDataEventArgs(MockMarketDataType.Last, price, volume, time, instrument)
            };
        }

        public static MarketReplayEvent CreateTickReplay(DateTime time, double lastPrice, double bidPrice, double askPrice, long volume, MockInstrument instrument = null)
        {
            return new MarketReplayEvent
            {
                Time = time,
                RecordType = MarketReplayRecordType.TickReplay,
                Price = lastPrice,
                BidPrice = bidPrice,
                AskPrice = askPrice,
                Volume = volume,
                MarketData = new MockMarketDataEventArgs(MockMarketDataType.Last, lastPrice, volume, time, instrument)
            };
        }

        public static MarketReplayEvent CreateDepth(
            DateTime time,
            MockMarketDataType type,
            MockMarketDepthOperation op,
            double price,
            long volume,
            int position = 0,
            string marketMaker = "",
            MockInstrument instrument = null)
        {
            return new MarketReplayEvent
            {
                Time = time,
                RecordType = MarketReplayRecordType.MarketDepth,
                Price = price,
                Volume = volume,
                MarketDepth = new MockMarketDepthEventArgs(type, op, price, volume, position, marketMaker, time, instrument)
            };
        }

        public static MarketReplayEvent CreateBar(MockBar bar, MarketReplayRecordType barType = MarketReplayRecordType.MinuteBar)
        {
            if (bar == null)
                throw new ArgumentNullException(nameof(bar));

            return new MarketReplayEvent
            {
                Time = bar.Time,
                RecordType = barType,
                Price = bar.Close,
                Volume = bar.Volume,
                Bar = bar
            };
        }
    }
}
