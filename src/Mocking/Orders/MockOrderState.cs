namespace NinjaTrader.UnitTest.Mocking
{
    /// <summary>
    /// Lifecycle states for simulated orders.
    /// </summary>
    public enum MockOrderState
    {
        Initialized,
        Submitted,
        Accepted,
        Working,
        PartFilled,
        Filled,
        Cancelled,
        Rejected
    }
}
