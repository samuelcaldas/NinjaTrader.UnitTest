using System.Collections.Generic;

namespace NinjaTrader.UnitTest
{
    public class TestSuite
    {
        private List<TestCase> testCases = new List<TestCase>();

        public void Add(TestCase testCase)
        {
            testCases.Add(testCase);
        }

        public TestResult Run(TestResult result = null)
        {
            result = result ?? new TestResult();
            foreach (TestCase testCase in testCases)
            {
                testCase.Run(result);
            }
            return result;
        }
    }
}
