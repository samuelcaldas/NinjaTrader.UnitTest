using System;
using System.IO;

namespace NinjaTrader.UnitTest
{
    /// <summary>
    /// TestResult implementation tailored for text-based runners and streams.
    /// </summary>
    public class TextTestResult : TestResult
    {
        public TextTestResult(TextWriter writer, int verbosity = 1, bool failfast = false)
            : base(verbosity, new TextWriterOutput(writer))
        {
            FailFast = failfast;
        }

        public TextTestResult(ITestOutput output, int verbosity = 1, bool failfast = false)
            : base(verbosity, output)
        {
            FailFast = failfast;
        }
    }
}