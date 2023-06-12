using System.Collections.Generic;

namespace NinjaTrader.UnitTest
{
    public class TestSuite
    {
        private List<TestCase> _tests = new List<TestCase>();

        public void Add(TestCase test)
        {
            _tests.Add(test);
        }

        public TestResult Run()
        {
            TestResult result = new TestResult();
            foreach (var test in _tests)
                test.Run(result);
            return result;
        }
    }
}
