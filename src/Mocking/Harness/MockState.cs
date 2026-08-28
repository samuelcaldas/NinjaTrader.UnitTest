namespace NinjaTrader.UnitTest.Mocking
{
    /// <summary>
    /// Lifecycle states for simulated NinjaScript execution.
    /// </summary>
    public enum MockState
    {
        SetDefaults,
        Configure,
        Active,
        DataLoaded,
        Historical,
        Transition,
        Realtime,
        Terminated
    }
}
