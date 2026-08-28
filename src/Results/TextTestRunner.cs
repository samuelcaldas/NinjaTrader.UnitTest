using System;
using System.IO;

namespace NinjaTrader.UnitTest
{
    /// <summary>
    /// Test runner modeled after Python unittest.TextTestRunner.
    /// </summary>
    public class TextTestRunner
    {
        private readonly int _verbosity;
        private readonly bool _failFast;
        private readonly ITestOutput _output;

        public TextTestRunner(int verbosity = 1, bool failfast = false, TextWriter stream = null, ITestOutput output = null)
        {
            _verbosity = verbosity;
            _failFast = failfast;
            _output = ResolveOutput(stream, output);
        }

        public static TestResult Run(
            TestSuite suite,
            bool descriptions = true,
            int verbosity = 1,
            bool failfast = false,
            bool buffer = false,
            Type resultclass = null,
            string warnings = null,
            bool tb_locals = false,
            TextWriter stream = null,
            ITestOutput output = null)
        {
            var runner = new TextTestRunner(verbosity, failfast, stream, output);
            return runner.Run(suite);
        }

        public static TestResult Run(
            TestCase testCase,
            int verbosity = 1,
            bool failfast = false,
            TextWriter stream = null,
            ITestOutput output = null)
        {
            var suite = new TestSuite();
            suite.Add(testCase);
            var runner = new TextTestRunner(verbosity, failfast, stream, output);
            return runner.Run(suite);
        }

        public TestResult Run(TestSuite suite)
        {
            if (suite == null)
                throw new ArgumentNullException(nameof(suite));

            var result = new TestResult(_verbosity, _output)
            {
                FailFast = _failFast
            };

            suite.Run(result);
            result.PrintSummary();

            return result;
        }

        private static ITestOutput ResolveOutput(TextWriter stream, ITestOutput output)
        {
            if (output != null)
                return output;

            if (stream != null)
                return new TextWriterOutput(stream);

            return TestOutputHelper.Default;
        }
    }
}
