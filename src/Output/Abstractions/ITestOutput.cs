using System;

namespace NinjaTrader.UnitTest
{
    /// <summary>
    /// Contract for abstracting test output across NinjaTrader, Console, and TextWriter destinations.
    /// </summary>
    public interface ITestOutput
    {
        void Write(string message);
        void WriteLine(string message, OutputLevel level = OutputLevel.Information);
        void WriteError(string message, Exception ex = null);
    }
}
