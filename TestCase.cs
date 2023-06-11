using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NinjaTrader.Cbi;
using NinjaTrader.NinjaScript;

namespace NinjaTraderUnitTesting
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
                NinjaTrader.NinjaScript.NinjaScript.Log($"{Name}: {ex.Message}", LogLevel.Error);
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

        protected void AssertNotEqual<T>(T a, T b, string message = null)
        {
            if (EqualityComparer<T>.Default.Equals(a, b))
                throw new Exception(message ?? $"Assertion failed: {a} == {b}");
        }

        protected void AssertTrue(bool condition, string message = null)
        {
            if (!condition)
                throw new Exception(message ?? "Assertion failed: condition is not true");
        }

        protected void AssertFalse(bool condition, string message = null)
        {
            if (condition)
                throw new Exception(message ?? "Assertion failed: condition is not false");
        }

        protected void AssertIs<T>(T a, T b, string message = null)
        {
            if (!object.ReferenceEquals(a, b))
                throw new Exception(message ?? $"Assertion failed: {a} is not the same object as {b}");
        }

        protected void AssertIsNot<T>(T a, T b, string message = null)
        {
            if (object.ReferenceEquals(a, b))
                throw new Exception(message ?? $"Assertion failed: {a} is the same object as {b}");
        }

        protected void AssertIsNone(object obj, string message = null)
        {
            if (obj != null)
                throw new Exception(message ?? "Assertion failed: object is not null");
        }

        protected void AssertIsNotNone(object obj, string message = null)
        {
            if (obj == null)
                throw new Exception(message ?? "Assertion failed: object is null");

        }

        protected void AssertIn<T>(T a, IEnumerable<T> b, string message = null)
        {
            if (!new HashSet<T>(b).Contains(a))
                throw new Exception(message ?? $"Assertion failed: {a} not found in collection");
        }

        protected void AssertNotIn<T>(T a, IEnumerable<T> b, string message = null)
        {
            if (new HashSet<T>(b).Contains(a))
                throw new Exception(message ?? $"Assertion failed: {a} found in collection");
        }

        protected void AssertIsInstance(object obj, Type type, string message = null)
        {
            if (!type.IsInstanceOfType(obj))
                throw new Exception(message ?? $"Assertion failed: object is not an instance of {type}");
        }

        protected void AssertNotIsInstance(object obj, Type type, string message = null)
        {
            if (type.IsInstanceOfType(obj))
                throw new Exception(message ?? $"Assertion failed: object is an instance of {type}");
        }

        // ... outros métodos de assert ...
    }
}
