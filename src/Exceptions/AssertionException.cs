using System;

namespace NinjaTrader.UnitTest
{
    /// <summary>
    /// Exception thrown when an assertion fails.
    /// </summary>
    public class AssertionException : Exception
    {
        public AssertionException(string message) : base(message) { }

        public AssertionException(string message, Exception innerException) : base(message, innerException) { }
    }
}
