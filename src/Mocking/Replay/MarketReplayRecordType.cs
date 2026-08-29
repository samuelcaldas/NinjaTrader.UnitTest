namespace NinjaTrader.UnitTest.Mocking
{
    /// <summary>
    /// Supported NinjaTrader historical and market replay file record formats.
    /// </summary>
    public enum MarketReplayRecordType
    {
        /// <summary>
        /// Tick format (yyyyMMdd HHmmss;price;volume or sub-second).
        /// </summary>
        Tick,

        /// <summary>
        /// Tick Replay with Bid/Ask (yyyyMMdd HHmmss;last;bid;ask;volume).
        /// </summary>
        TickReplay,

        /// <summary>
        /// Minute Bar format (yyyyMMdd HHmmss;open;high;low;close;volume).
        /// </summary>
        MinuteBar,

        /// <summary>
        /// Daily Bar format (yyyyMMdd;open;high;low;close;volume).
        /// </summary>
        DayBar,

        /// <summary>
        /// Level 2 Market Depth Replay format (timestamp;type;operation;price;volume;position;marketMaker).
        /// </summary>
        MarketDepth
    }
}
