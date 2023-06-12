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
                    result.AddSuccess(this);
                    NinjaTrader.NinjaScript.NinjaScript.Log($"{method.Name} passed", LogLevel.Information);
                }
                else
                {
                    throw new Exception($"No such test method: {methodName}");
                }
            }
            catch (TargetInvocationException e)
            {
                if (e.InnerException is Exception exception)
                {
                    result.AddFailure(this, exception.Message);
                    NinjaTrader.NinjaScript.NinjaScript.Log($"{methodName} failed: {exception.Message}", LogLevel.Error);
                }
                else
                {
                    result.AddError(this, e.InnerException.Message);
                    NinjaTrader.NinjaScript.NinjaScript.Log($"{methodName} errored: {e.InnerException.Message}", LogLevel.Error);
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

        public SubTest SubTest(string msg = null, Dictionary<string, object> parameters)
        {
            return new SubTest(this, msg, parameters);
        }

        public void SetVerbose(bool verbose)
        {
            this.verbose = verbose;
        }

        private string methodName;
        private bool verbose = true;

        protected bool IsVerbose()
        {
            return verbose;
        }
    }
}
