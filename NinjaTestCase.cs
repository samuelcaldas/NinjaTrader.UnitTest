using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UnittestExample
{
    public enum LogLevel
    {
        Debug,
        Info,
        Warning,
        Error
    }

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

    public class TestCase
    {
        public string Name { get; set; }
        public Action TestMethod { get; set; }

        public TestCase(string name, Action testMethod)
        {
            Name = name;
            TestMethod = testMethod;
        }

        public void Run(TestResult result)
        {
            result.RunCount++;
            try
            {
                TestMethod();
            }
            catch (Exception ex)
            {
                result.FailCount++;
                result.Failures.Add($"{Name}: {ex.Message}");
                Log($"{Name}: {ex.Message}", LogLevel.Error);
            }
        }

        protected void Assert(bool condition, string message = null)
        {
            if (!condition)
                throw new Exception(message ?? "Assertion failed");
        }

        protected void AssertEqual<T>(T a, T b, string message = null)
        {
            if (!EqualityComparer<T>.Default.Equals(a, b))
                throw new Exception(message ?? $"Assertion failed: {a} != {b}");
        }

        // ... outros métodos de assert ...

        private void Log(string message, LogLevel logLevel)
        {
            // Implementação do método Log do NinjaTrader NinjaScript
            // ...
        }
    }

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
