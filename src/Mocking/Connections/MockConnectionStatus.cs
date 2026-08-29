namespace NinjaTrader.UnitTest.Mocking
{
    /// <summary>
    /// Connection status states for mock trading connections.
    /// </summary>
    public enum MockConnectionStatus
    {
        Disconnected,
        Connecting,
        Connected,
        ConnectionLost,
        Disconnecting
    }
}
