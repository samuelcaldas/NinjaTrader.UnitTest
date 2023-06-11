using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UnittestExample
{
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
    }
}
