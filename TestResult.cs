using System.Collections.Generic;

namespace NinjaTrader.UnitTest
{
    public class TestResult
    {
        public int RunCount { get; set; }
        public int FailCount { get; set; }
        public List<string> Failures { get; set; }

        public TestResult()
        {
            RunCount = 0;
            FailCount = 0;
            Failures = new List<string>();
        }
    }
}
