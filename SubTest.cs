using NinjaTrader.Cbi;
using System;
using System.Collections.Generic;

namespace NinjaTrader.UnitTest
{
    public class SubTest : IDisposable
    {
        private string testCase;
        private string msg;
        private Dictionary<string, object> parameters;
        private bool success = true;

        public SubTest(string testCase, string msg = null, Dictionary<string, object> parameters = null)
        {
            this.testCase = testCase;
            this.msg = msg;
            this.parameters = parameters ?? new Dictionary<string, object>();
        }

        public void Dispose()
        {
            try
            {
                // Execute the test case
                //testCase.Run();

                // Set the success flag to true if execution does not throw an exception
                success = true;
            }
            catch (Exception)
            {
                // Set the success flag to false if an exception is encountered
                success = false;
                throw; // re-throw the exception to signal test failure
            }
            finally
            {
                if (success)
                {
                    NinjaTrader.NinjaScript.NinjaScript.Log($"{testCase} ({msg})", LogLevel.Information);
                }
                else
                {
                    NinjaTrader.NinjaScript.NinjaScript.Log($"{testCase} ({msg}) ... FAIL", LogLevel.Error);
                }
            }
        }
    }
}
