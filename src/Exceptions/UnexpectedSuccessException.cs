using System;

namespace NinjaTrader.UnitTest
{
    /// <summary>
    /// Exception thrown internally when a test decorated with ExpectedFailure passes unexpectedly.
    /// </summary>
    public class UnexpectedSuccessException : Exception
    {
        public UnexpectedSuccessException(string message = "Test succeeded unexpectedly") : base(message) { }
    }
}
