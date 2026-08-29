namespace NinjaTrader.UnitTest.Mocking
{
    /// <summary>
    /// Supported time in force options for mock orders.
    /// </summary>
    public enum MockTimeInForce
    {
        Day,
        Gtc, // Good Till Cancelled
        Ioc, // Immediate Or Cancel
        Fok  // Fill Or Kill
    }
}
