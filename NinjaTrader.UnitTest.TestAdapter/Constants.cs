using System;

namespace NinjaTrader.UnitTest.TestAdapter
{
    /// <summary>
    /// Constants for the NinjaTrader.UnitTest Visual Studio Test Adapter.
    /// </summary>
    public static class Constants
    {
        /// <summary>
        /// The unique executor URI identifying this test adapter to VSTest.
        /// </summary>
        public const string ExecutorUri = "executor://NinjaTrader.UnitTest.TestAdapter/v1";

        /// <summary>
        /// Friendly extension name for logs and diagnostics.
        /// </summary>
        public const string ExtensionName = "NinjaTrader.UnitTest.TestAdapter";
    }
}
