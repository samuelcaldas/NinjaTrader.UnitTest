using System;
using System.IO;
using NinjaTrader.UnitTest;

namespace NinjaTrader.UnitTest.Tests.Execution
{
    public class SkipAndExpectedFailureTests : TestCase
    {
        public void TestDynamicSkip()
        {
            var skippedTest = new InnerSkippedCase("ExecuteSkip");
            var result = new TestResult(verbose: false, output: new TextWriterOutput(new StringWriter()));
            skippedTest.Run(result);

            AssertEqual(1, result.SkipCount);
            AssertEqual(0, result.FailureCount);
            AssertEqual(0, result.ErrorCount);
        }

        public void TestExpectedFailureHandling()
        {
            var expectedFailTest = new InnerExpectedFailCase("ExecuteExpectedToFail");
            var result = new TestResult(verbose: false, output: new TextWriterOutput(new StringWriter()));
            expectedFailTest.Run(result);

            AssertEqual(1, result.ExpectedFailureCount);
            AssertEqual(0, result.FailureCount);
            AssertEqual(0, result.ErrorCount);
            AssertTrue(result.WasSuccessful());
        }
    }

    internal class InnerSkippedCase : TestCase
    {
        public InnerSkippedCase(string name) : base(name) { }
        public void ExecuteSkip() => SkipTest("Skipping intentionally");
    }

    internal class InnerExpectedFailCase : TestCase
    {
        public InnerExpectedFailCase(string name) : base(name) { }

        [ExpectedFailure("This test is expected to fail")]
        public void ExecuteExpectedToFail()
        {
            AssertEqual(1, 2);
        }
    }
}
