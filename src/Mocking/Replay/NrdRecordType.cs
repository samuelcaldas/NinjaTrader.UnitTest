namespace NinjaTrader.UnitTest.Mocking
{
    /// <summary>
    /// Binary record identifier tags within an .nrd replay file.
    /// </summary>
    public enum NrdRecordType : byte
    {
        /// <summary>
        /// Level 1 Market Data record (Trades, Best Bid, Best Ask).
        /// </summary>
        MarketData = 1,

        /// <summary>
        /// Level 2 Market Depth record (DOM ladder updates).
        /// </summary>
        MarketDepth = 2,

        /// <summary>
        /// Historical Bar summary record.
        /// </summary>
        Bar = 3
    }
}
