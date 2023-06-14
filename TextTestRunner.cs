using System;
using System.Collections.Generic;
using System.IO;
using NinjaTrader.Cbi;
using NinjaTrader.NinjaScript;

namespace NinjaTrader.UnitTest
{
    public static class TextTestRunner
    {
        public static TestResult Run(
            TestSuite suite,
            bool descriptions = true,
            int verbosity = 1,
            bool failfast = false,
            bool buffer = false,
            Type resultclass = null,
            string warnings = null,
            bool tb_locals = false)
        {
            var runner = new BasicTestRunner(descriptions, verbosity);
            return runner.Run(suite);
        }
    }

    public class BasicTestRunner
    {
        private readonly bool _descriptions;
        private readonly int _verbosity;
        private readonly TextWriter _stream;

        public BasicTestRunner(bool descriptions, int verbosity, TextWriter stream = null)
        {
            _descriptions = descriptions;
            _verbosity = verbosity;
            _stream = stream;
        }

        public TestResult Run(TestSuite suite)
        {
            var result = new TestResult();
            suite.Run(result);

            if (_verbosity > 0)
                PrintResult(result);

            return result;
        }

        private void PrintResult(TestResult result)
        {
            var messages = new List<string>();

            foreach (var failure in result.Failures)
                messages.Add(failure.ToString());

            foreach (var error in result.Errors)
                messages.Add(error.ToString());

            messages.Add($"Ran {result.RunCount} test(s) in {result.Duration} seconds");
            messages.Add($"Total: {result.RunCount}, Errors: {result.ErrorCount}, Failures: {result.FailureCount}");

            if (_stream != null)
            {
                foreach (var message in messages)
                    _stream.WriteLine(message);
            }
            else
            {
                foreach (var message in messages)
                    NinjaTrader.NinjaScript.NinjaScript.Log(message, LogLevel.Information);
            }
        }
    }
}