using System;

namespace NinjaTrader.UnitTest
{
    /// <summary>
    /// Marks a test method as an expected failure. If the test fails, it is counted as an expected failure.
    /// If the test passes, it is counted as an unexpected success.
    /// </summary>
    [AttributeUsage(AttributeTargets.Method | AttributeTargets.Class, AllowMultiple = false, Inherited = true)]
    public class ExpectedFailureAttribute : Attribute
    {
        public string Reason { get; }

        public ExpectedFailureAttribute(string reason = null)
        {
            Reason = reason;
        }
    }
}
