using System;
using System.Collections.Generic;

namespace NinjaTrader.UnitTest
{
    /// <summary>
    /// Accumulates and reports results of test execution.
    /// </summary>
    public class TestResult
    {
        public List<string> Successes { get; } = new List<string>();
        public List<(string TestName, Exception Exception)> Failures { get; } = new List<(string, Exception)>();
        public List<(string TestName, Exception Exception)> Errors { get; } = new List<(string, Exception)>();
        public List<(string TestName, string Reason)> Skipped { get; } = new List<(string, string)>();
        public List<(string TestName, Exception Exception)> ExpectedFailures { get; } = new List<(string, Exception)>();
        public List<string> UnexpectedSuccesses { get; } = new List<string>();
        public List<(string TestName, SubTest SubTest, Exception Exception)> SubTestFailures { get; } = new List<(string, SubTest, Exception)>();

        public double Duration { get; private set; }
        public bool FailFast { get; set; }
        public bool ShouldStop { get; private set; }
        public int Verbosity { get; set; }
        public ITestOutput Output { get; set; }

        public int SuccessCount => Successes.Count;
        public int FailureCount => Failures.Count + SubTestFailures.Count;
        public int ErrorCount => Errors.Count;
        public int SkipCount => Skipped.Count;
        public int ExpectedFailureCount => ExpectedFailures.Count;
        public int UnexpectedSuccessCount => UnexpectedSuccesses.Count;
        public int RunCount => SuccessCount + Failures.Count + ErrorCount + ExpectedFailureCount + UnexpectedSuccessCount;

        public TestResult(bool verbose = true, ITestOutput output = null)
        {
            Verbosity = verbose ? 1 : 0;
            Output = output ?? TestOutputHelper.Default;
        }

        public TestResult(int verbosity, ITestOutput output = null)
        {
            Verbosity = verbosity;
            Output = output ?? TestOutputHelper.Default;
        }

        public virtual void AddSuccess(string testCase)
        {
            Successes.Add(testCase);
            if (Verbosity > 0)
            {
                Output.WriteLine($"{testCase} ... OK", OutputLevel.Information);
            }
        }

        public virtual void AddFailure(string testCase, Exception exception)
        {
            Failures.Add((testCase, exception));
            if (Verbosity > 0)
            {
                Output.WriteLine($"{testCase} ... FAIL: {exception.Message}", OutputLevel.Warning);
            }
            CheckFailFast();
        }

        public virtual void AddError(string testCase, Exception exception)
        {
            Errors.Add((testCase, exception));
            if (Verbosity > 0)
            {
                Output.WriteLine($"{testCase} ... ERROR: {exception.Message}", OutputLevel.Error);
            }
            CheckFailFast();
        }

        public virtual void AddSkip(string testCase, string reason)
        {
            Skipped.Add((testCase, reason));
            if (Verbosity > 0)
            {
                Output.WriteLine($"{testCase} ... SKIPPED ({reason})", OutputLevel.Information);
            }
        }

        public virtual void AddExpectedFailure(string testCase, Exception exception)
        {
            ExpectedFailures.Add((testCase, exception));
            if (Verbosity > 0)
            {
                Output.WriteLine($"{testCase} ... expected failure: {exception.Message}", OutputLevel.Information);
            }
        }

        public virtual void AddUnexpectedSuccess(string testCase)
        {
            UnexpectedSuccesses.Add(testCase);
            if (Verbosity > 0)
            {
                Output.WriteLine($"{testCase} ... unexpected success", OutputLevel.Warning);
            }
            CheckFailFast();
        }

        public virtual void AddSubTest(string testCase, SubTest subTest, Exception exception)
        {
            if (exception == null)
            {
                LogSubTestSuccess(subTest);
                return;
            }

            SubTestFailures.Add((testCase, subTest, exception));
            if (Verbosity > 0)
            {
                Output.WriteLine($"  {subTest} ... FAIL: {exception.Message}", OutputLevel.Warning);
            }
            CheckFailFast();
        }

        public void AddTime(double duration)
        {
            Duration += duration;
        }

        public void Stop()
        {
            ShouldStop = true;
        }

        public bool WasSuccessful()
        {
            return FailureCount == 0 && ErrorCount == 0 && UnexpectedSuccessCount == 0;
        }

        public void PrintSummary()
        {
            Output.WriteLine("\n----------------------------------------------------------------------", OutputLevel.Information);
            Output.WriteLine($"Ran {RunCount} test(s) in {Duration:F3}s", OutputLevel.Information);

            PrintFailures();
            PrintErrors();
            PrintOverallStatus();
        }

        private void CheckFailFast()
        {
            if (FailFast)
            {
                Stop();
            }
        }

        private void LogSubTestSuccess(SubTest subTest)
        {
            if (Verbosity > 1)
            {
                Output.WriteLine($"  {subTest} ... OK", OutputLevel.Information);
            }
        }

        private void PrintFailures()
        {
            if (Failures.Count == 0 && SubTestFailures.Count == 0)
                return;

            Output.WriteLine("\nFAILURES:", OutputLevel.Warning);
            foreach (var f in Failures)
            {
                Output.WriteLine($"FAIL: {f.TestName}\n{f.Exception}", OutputLevel.Warning);
            }
            foreach (var stf in SubTestFailures)
            {
                Output.WriteLine($"SUBTEST FAIL: {stf.SubTest}\n{stf.Exception}", OutputLevel.Warning);
            }
        }

        private void PrintErrors()
        {
            if (Errors.Count == 0)
                return;

            Output.WriteLine("\nERRORS:", OutputLevel.Error);
            foreach (var e in Errors)
            {
                Output.WriteLine($"ERROR: {e.TestName}\n{e.Exception}", OutputLevel.Error);
            }
        }

        private void PrintOverallStatus()
        {
            if (WasSuccessful())
            {
                string extra = SkipCount > 0 ? $" (skipped={SkipCount})" : "";
                Output.WriteLine($"OK{extra}", OutputLevel.Information);
                return;
            }

            var details = new List<string>();
            if (FailureCount > 0) details.Add($"failures={FailureCount}");
            if (ErrorCount > 0) details.Add($"errors={ErrorCount}");
            if (SkipCount > 0) details.Add($"skipped={SkipCount}");
            if (UnexpectedSuccessCount > 0) details.Add($"unexpected_successes={UnexpectedSuccessCount}");

            Output.WriteLine($"FAILED ({string.Join(", ", details)})", OutputLevel.Error);
        }
    }
}
