using System;

namespace NinjaTrader.UnitTest
{
    public class SkipTestException : Exception
    {
        public SkipTestException(string message) : base(message) { }
    }
}
