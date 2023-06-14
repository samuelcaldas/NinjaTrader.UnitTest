using System;
using System.IO;
using System.Reflection;

namespace NinjaTrader.UnitTest
{
    public class TextTestResult : TestResult
    {
        private readonly TextWriter _writer;

        public TextTestResult(TextWriter writer)
        {
            _writer = writer;
        }

        public override void AddError(string testCase, Exception exception)
        {
            base.AddError(testCase, exception);
            _writer.WriteLine($"ERROR: {testCase}");
            _writer.WriteLine(exception);
        }

        public override void AddFailure(string testCase, Exception exception)
        {
            base.AddFailure(testCase, exception);
            _writer.WriteLine($"FAIL: {testCase}");
            _writer.WriteLine(exception);
        }

        //public override void StartTest(string testCase)
        //{
        //    base.StartTest(testCase);
        //    _writer.Write(testCase);
        //    _writer.Write(" ... ");
        //}

        //public override void StopTest(string testCase)
        //{
        //    base.StopTest(testCase);
        //    _writer.WriteLine("ok");
        //}
    }
}