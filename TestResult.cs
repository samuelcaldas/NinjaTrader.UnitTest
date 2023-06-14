using NinjaTrader.Cbi;
using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;

namespace NinjaTrader.UnitTest
{
    public class TestResult
    {
        public int RunCount => SuccessCount + FailureCount + ErrorCount;
        public List<string> Successes { get; } = new List<string>();
        public List<(string, Exception)> Errors { get; } = new List<(string, Exception)>();
        public List<(string, Exception)> Failures { get; } = new List<(string, Exception)>();
        public double Duration { get; private set; }

        public int FailureCount => Failures.Count;
        public int ErrorCount => Errors.Count;
        public int SuccessCount => Successes.Count;

        public TestResult(bool verbose = true)
        {
            this.verbose = verbose;
        }

        internal virtual void AddSuccess(string testCase)
        {
            Successes.Add(testCase);
            if (verbose)
            {
                NinjaTrader.NinjaScript.NinjaScript.Log($"{testCase} ... OK", LogLevel.Information);
            }
        }

        public virtual void AddFailure(string testCase, Exception exception)
        {
            Failures.Add((testCase, exception));
            if (verbose)
            {
                NinjaTrader.NinjaScript.NinjaScript.Log($"{testCase} ... FAIL: {exception.Message}", LogLevel.Warning);
            }
        }

        public virtual void AddError(string testCase, Exception exception)
        {
            Errors.Add((testCase, exception));
            if (verbose)
            {
                NinjaTrader.NinjaScript.NinjaScript.Log($"{testCase} ... ERROR: {exception.Message}", LogLevel.Error);
            }
        }

        public virtual void AddSubTest(string testCase, SubTest subTest, string exception)
        {

        }

        internal void AddTime(double duration)
        {
            Duration += duration;
        }

        public bool WasSuccessful()
        {
            return (FailureCount == 0 && ErrorCount == 0);
        }

        public void PrintSummary()
        {
            NinjaTrader.NinjaScript.NinjaScript.Log($"Ran {RunCount} tests in {Duration:F3}s", LogLevel.Information);
            if (WasSuccessful())
            {
                NinjaTrader.NinjaScript.NinjaScript.Log("OK", LogLevel.Information);
            }
            else
            {
                NinjaTrader.NinjaScript.NinjaScript.Log($"FAILED (failures={FailureCount}, errors={ErrorCount})", LogLevel.Error);
            }
        }
        private bool verbose = true;
    }
}