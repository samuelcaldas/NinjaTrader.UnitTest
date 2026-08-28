using System;

namespace NinjaTrader.UnitTest
{
    /// <summary>
    /// Exception thrown when an assertion condition fails.
    /// </summary>
    public class AssertionException : Exception
    {
        public AssertionException(string message) : base(message) { }

        public AssertionException(string message, Exception innerException) : base(message, innerException) { }
    }

    /// <summary>
    /// Exception thrown when a test is skipped dynamically via SkipTest or skip attributes.
    /// </summary>
    public class SkipTestException : Exception
    {
        public SkipTestException(string reason) : base(reason) { }

        public SkipTestException(string reason, Exception innerException) : base(reason, innerException) { }
    }

    /// <summary>
    /// Exception thrown internally when an expected failure passes unexpectedly.
    /// </summary>
    public class UnexpectedSuccessException : Exception
    {
        public UnexpectedSuccessException(string message = "Test succeeded unexpectedly") : base(message) { }
    }
}
