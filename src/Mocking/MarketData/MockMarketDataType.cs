namespace NinjaTrader.UnitTest.Mocking
{
    /// <summary>
    /// Supported market data event types modeling NinjaTrader's MarketDataType.
    /// </summary>
    public enum MockMarketDataType
    {
        Ask,
        Bid,
        Last,
        Volume,
        OpenInterest,
        Opening,
        High,
        Low,
        Settlement,
        DailyVolume
    }
}
