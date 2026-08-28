using System;

namespace NinjaTrader.UnitTest
{
    /// <summary>
    /// Exception thrown when a test is skipped dynamically via SkipTest or skip attributes.
    /// </summary>
    public class SkipTestException : Exception
    {
        public SkipTestException(string reason) : base(reason) { }

        public SkipTestException(string reason, Exception innerException) : base(reason, innerException) { }
    }
}
