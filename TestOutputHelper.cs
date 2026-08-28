using System;

namespace NinjaTrader.UnitTest
{
    /// <summary>
    /// Helper to provide or configure the default ITestOutput logger.
    /// </summary>
    public static class TestOutputHelper
    {
        private static ITestOutput _defaultOutput;

        public static ITestOutput Default
        {
            get
            {
                if (_defaultOutput == null)
                {
                    _defaultOutput = CreateDefaultOutput();
                }
                return _defaultOutput;
            }
            set => _defaultOutput = value;
        }

        private static ITestOutput CreateDefaultOutput()
        {
            try
            {
                // Verify if NinjaTrader output is available
                return new NinjaTraderOutput();
            }
            catch
            {
                return new ConsoleOutput();
            }
        }
    }
}
