using System.Collections.Generic;

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

        internal void AddSuccess()
        {
            successes++;
        }

        internal void AddFailure(TestCase testCase, string message)
        {
            failures++;
            failuresList.Add((testCase, message));
        }

        internal void AddError(TestCase testCase, string message)
        {
            errors++;
            errorsList.Add((testCase, message));
        }

        private int successes = 0;
        private int failures = 0;
        private int errors = 0;
        private List<(TestCase, string)> failuresList = new List<(TestCase, string)>();
        private List<(TestCase, string)> errorsList = new List<(TestCase, string)>();
    }
}
