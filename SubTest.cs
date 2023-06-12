using NinjaTrader.Cbi;
using System;
using System.Collections.Generic;

namespace NinjaTrader.UnitTest
{
    public class SubTest : IDisposable
    {
        private TestCase testCase;
        private string msg;
        private Dictionary<string, object> parameters;
        private bool success;

        public SubTest(TestCase testCase, string msg = null, Dictionary<string, object> parameters = null)
        {
            this.testCase = testCase;
            this.msg = msg;
            this.parameters = parameters ?? new Dictionary<string, object>();
            this.success = true;
        }

        public void Dispose()
        {
            if (success)
            {
                NinjaTrader.NinjaScript.NinjaScript.Log($"{testCase.GetType().Name} ({msg}) ... ok", LogLevel.Information);
            }
            else
            {
                NinjaTrader.NinjaScript.NinjaScript.Log($"{testCase.GetType().Name} ({msg}) ... FAIL", LogLevel.Error);
            }
        }

        internal void SetSuccess(bool success)
        {
            this.success = success;
        }
    }
}
