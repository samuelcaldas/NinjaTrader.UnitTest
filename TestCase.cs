using NinjaTrader.Cbi;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Reflection;

namespace NinjaTrader.UnitTest
{
    public class TestCase : Assert
    {
        public TestCase(string methodName = "RunTest")
        {
            this.methodName = methodName;
        }

        public TestResult Run(TestResult result = null)
        {
            result = result ?? new TestResult();
            Stopwatch stopwatch = Stopwatch.StartNew();
            SetUp();
            try
            {
                MethodInfo method = this.GetType().GetMethod(methodName);
                if (method != null)
                {
                    method.Invoke(this, null);
                }
                else
                {
                    NinjaTrader.NinjaScript.NinjaScript.Log($"No such test method: {methodName}", LogLevel.Error);
                }
                result.AddSuccess(methodName);
            }
            catch (TargetInvocationException e)
            {
                if (e.InnerException is Exception exception)
                {
                    result.AddFailure(methodName, exception);
                }
                else
                {
                    result.AddError(methodName, e.InnerException);
                }
            }
            TearDown();
            stopwatch.Stop();
            result.AddTime(stopwatch.Elapsed.TotalSeconds);
            return result;
        }

        public virtual void SetUp() { }

        public virtual void TearDown() { }

        public static void SetUpClass() { }

        public static void TearDownClass() { }

        public void SkipTest(string reason)
        {
            throw new SkipTestException(reason);
        }

        public SubTest SubTest(string msg = null, Dictionary<string, object> parameters = null)
        {
            return new SubTest(methodName, msg, parameters);
        }

        private string methodName;
    }
}
