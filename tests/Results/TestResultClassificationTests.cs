using System;
using System.IO;
using NinjaTrader.UnitTest;

namespace NinjaTrader.UnitTest.Tests.Results
{
    public class TestResultClassificationTests : TestCase
    {
        public void TestErrorAndFailureClassification()
        {
            var failingTest = new InnerFailingCase("ExecuteFail");
            var errorTest = new InnerErrorCase("ExecuteError");
            var passingTest = new InnerPassingCase("ExecutePass");

            var result = new TestResult(verbose: false, output: new TextWriterOutput(new StringWriter()));
            failingTest.Run(result);
            errorTest.Run(result);
            passingTest.Run(result);

            AssertEqual(1, result.FailureCount, "Expected exactly 1 failure");
            AssertEqual(1, result.ErrorCount, "Expected exactly 1 error");
            AssertEqual(1, result.SuccessCount, "Expected exactly 1 success");
            AssertEqual(3, result.RunCount, "Expected exactly 3 run count");
            AssertFalse(result.WasSuccessful());
        }
    }

    internal class InnerPassingCase : TestCase
    {
        public InnerPassingCase(string name) : base(name) { }
        public void ExecutePass() => AssertTrue(true);
    }

    internal class InnerFailingCase : TestCase
    {
        public InnerFailingCase(string name) : base(name) { }
        public void ExecuteFail() => Fail("Expected failure");
    }

    internal class InnerErrorCase : TestCase
    {
        public InnerErrorCase(string name) : base(name) { }
        public void ExecuteError() => throw new InvalidOperationException("Unexpected runtime crash");
    }
}
