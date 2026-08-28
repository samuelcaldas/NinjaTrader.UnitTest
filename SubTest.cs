using System;
using System.Collections.Generic;

namespace NinjaTrader.UnitTest
{
    /// <summary>
    /// Represents a scoped sub-test within a TestCase method.
    /// </summary>
    public class SubTest : IDisposable
    {
        public string TestName { get; }
        public string Message { get; }
        public Dictionary<string, object> Parameters { get; }
        public bool IsDisposed { get; private set; }

        private readonly TestCase _testCase;

        public SubTest(string testName, string msg = null, Dictionary<string, object> parameters = null, TestCase testCase = null)
        {
            TestName = testName;
            Message = msg;
            Parameters = parameters ?? new Dictionary<string, object>();
            _testCase = testCase;
        }

        public void Dispose()
        {
            if (!IsDisposed)
            {
                IsDisposed = true;
                if (_testCase != null && _testCase.CurrentSubTest == this)
                {
                    _testCase.CurrentSubTest = null;
                }
            }
        }

        public override string ToString()
        {
            var parts = new List<string>();
            if (!string.IsNullOrEmpty(Message))
                parts.Add(Message);

            if (Parameters != null && Parameters.Count > 0)
            {
                var paramStrings = new List<string>();
                foreach (var kvp in Parameters)
                {
                    paramStrings.Add($"{kvp.Key}={kvp.Value}");
                }
                parts.Add(string.Join(", ", paramStrings));
            }

            string details = parts.Count > 0 ? $" ({string.Join(", ", parts)})" : string.Empty;
            return $"{TestName}{details}";
        }
    }
}
