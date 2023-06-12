using NinjaTrader.Cbi;
using System;
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
            SetUp();
            try
            {
                MethodInfo method = this.GetType().GetMethod(methodName);
                if (method != null)
                {
                    method.Invoke(this, null);
                    result.AddSuccess();
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

        public SubTest SubTest(string msg = null, params object[] parameters)
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

    public class SkipTestException : Exception
    {
        public SkipTestException(string message) : base(message) { }
    }

    public class SubTest : IDisposable
    {
        private TestCase testCase;
        private string msg;
        private object[] parameters;

        public SubTest(TestCase testCase, string msg, object[] parameters)
        {
            this.testCase = testCase;
            this.msg = msg;
            this.parameters = parameters;
        }
        public void Dispose()
        {
            // Implement any necessary cleanup code here
        }
    }
}
