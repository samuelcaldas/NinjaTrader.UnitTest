using System;
using System.Collections.Generic;
using NinjaTrader.Cbi;
using NinjaTrader.NinjaScript;

namespace NinjaTrader.UnitTest
{
    public class TestResult
    {
        public int TestsRun => successes + failures + errors;

        public int Successes => successes;

        public int Failures => failures;

        public int Errors => errors;

        public List<(TestCase, string)> FailuresList => failuresList;

        public List<(TestCase, string)> ErrorsList => errorsList;

        public double Time => time;

        internal void AddSuccess(TestCase testCase)
        {
            successes++;
            if (verbose)
            {
                NinjaTrader.NinjaScript.NinjaScript.Log($"{testCase.GetType().Name} ... ok", LogLevel.Information);
            }
        }

        internal void AddFailure(TestCase testCase, string message)
        {
            failures++;
            failuresList.Add((testCase, message));
            if (verbose)
            {
                NinjaTrader.NinjaScript.NinjaScript.Log($"{testCase.GetType().Name} ... FAIL", LogLevel.Error);
            }
        }

        internal void AddError(TestCase testCase, string message)
        {
            errors++;
            errorsList.Add((testCase, message));
            if (verbose)
            {
                NinjaTrader.NinjaScript.NinjaScript.Log($"{testCase.GetType().Name} ... ERROR", LogLevel.Error);
            }
        }

        internal void AddTime(double seconds)
        {
            time += seconds;
        }

        public void PrintSummary()
        {
            NinjaTrader.NinjaScript.NinjaScript.Log($"\nRan {TestsRun} tests in {Time:F3}s\n", LogLevel.Information);
            if (Failures == 0 && Errors == 0)
            {
                NinjaTrader.NinjaScript.NinjaScript.Log("OK", LogLevel.Information);
            }
            else
            {
                NinjaTrader.NinjaScript.NinjaScript.Log($"FAILED (failures={Failures}, errors={Errors})", LogLevel.Error);
            }
        }

        public void SetVerbose(bool verbose)
        {
            this.verbose = verbose;
        }

        private int successes = 0;
        private int failures = 0;
        private int errors = 0;
        private double time = 0;
        private bool verbose = true;
        private List<(TestCase, string)> failuresList = new List<(TestCase, string)>();
        private List<(TestCase, string)> errorsList = new List<(TestCase, string)>();
    }
}
