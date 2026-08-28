using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Reflection;

namespace NinjaTrader.UnitTest
{
    /// <summary>
    /// Base class for all unit test cases, modeled after Python unittest.TestCase.
    /// </summary>
    public class TestCase : Assert
    {
        public string MethodName { get; set; }
        public virtual string TestName => $"{GetType().Name}.{MethodName}";

        public SubTest CurrentSubTest { get; internal set; }
        public TestResult CurrentResult { get; internal set; }
        private int _currentSubTestFailures;
        private readonly List<Action> _cleanups = new List<Action>();

        public TestCase(string methodName = "RunTest")
        {
            MethodName = methodName;
        }

        public virtual void SetUp() { }

        public virtual void TearDown() { }

        public static void SetUpClass() { }

        public static void TearDownClass() { }

        public void AddCleanup(Action cleanupAction)
        {
            if (cleanupAction != null)
                _cleanups.Add(cleanupAction);
        }

        public void DoCleanups(TestResult result = null)
        {
            // Cleanups run in LIFO order (reverse of registration)
            for (int i = _cleanups.Count - 1; i >= 0; i--)
            {
                try
                {
                    _cleanups[i]();
                }
                catch (Exception ex)
                {
                    result?.AddError(TestName, new Exception($"Cleanup action failed: {ex.Message}", ex));
                }
            }
            _cleanups.Clear();
        }

        public void SkipTest(string reason = "Test skipped")
        {
            throw new SkipTestException(reason);
        }

        public SubTest SubTest(string msg = null, Dictionary<string, object> parameters = null)
        {
            var subTest = new SubTest(TestName, msg, parameters, this);
            CurrentSubTest = subTest;
            return subTest;
        }

        public void SubTest(string msg, Action action, Dictionary<string, object> parameters = null)
        {
            if (action == null)
                throw new ArgumentNullException(nameof(action));

            var subTest = new SubTest(TestName, msg, parameters, this);
            CurrentSubTest = subTest;
            try
            {
                action();
                CurrentResult?.AddSubTest(TestName, subTest, null);
            }
            catch (Exception ex)
            {
                _currentSubTestFailures++;
                CurrentResult?.AddSubTest(TestName, subTest, ex);
            }
            finally
            {
                CurrentSubTest = null;
            }
        }

        public virtual TestResult Run(TestResult result = null)
        {
            result = result ?? new TestResult();
            CurrentResult = result;
            _currentSubTestFailures = 0;

            if (result.ShouldStop)
                return result;

            // Check class-level skip
            var classSkip = GetType().GetCustomAttribute<SkipAttribute>();
            if (classSkip != null)
            {
                result.AddSkip(TestName, classSkip.Reason);
                return result;
            }

            // Find method
            MethodInfo method = GetType().GetMethod(MethodName, BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
            if (method == null)
            {
                result.AddError(TestName, new MissingMethodException($"No such test method '{MethodName}' on class {GetType().FullName}"));
                return result;
            }

            // Check method-level skip
            var methodSkip = method.GetCustomAttribute<SkipAttribute>();
            if (methodSkip != null)
            {
                result.AddSkip(TestName, methodSkip.Reason);
                return result;
            }

            // Check SkipIf
            var skipIf = method.GetCustomAttribute<SkipIfAttribute>() ?? GetType().GetCustomAttribute<SkipIfAttribute>();
            if (skipIf != null && EvaluateCondition(skipIf.ConditionMemberName))
            {
                result.AddSkip(TestName, skipIf.Reason);
                return result;
            }

            // Check SkipUnless
            var skipUnless = method.GetCustomAttribute<SkipUnlessAttribute>() ?? GetType().GetCustomAttribute<SkipUnlessAttribute>();
            if (skipUnless != null && !EvaluateCondition(skipUnless.ConditionMemberName))
            {
                result.AddSkip(TestName, skipUnless.Reason);
                return result;
            }

            bool expectFailure = method.GetCustomAttribute<ExpectedFailureAttribute>() != null ||
                                 GetType().GetCustomAttribute<ExpectedFailureAttribute>() != null;

            Stopwatch stopwatch = Stopwatch.StartNew();
            bool setUpSucceeded = false;

            try
            {
                try
                {
                    SetUp();
                    setUpSucceeded = true;
                }
                catch (SkipTestException skipEx)
                {
                    result.AddSkip(TestName, skipEx.Message);
                    return result;
                }
                catch (Exception ex)
                {
                    result.AddError(TestName, new Exception($"SetUp failed: {ex.Message}", ex));
                    return result;
                }

                try
                {
                    method.Invoke(this, null);

                    if (expectFailure)
                    {
                        result.AddUnexpectedSuccess(TestName);
                    }
                    else if (_currentSubTestFailures == 0)
                    {
                        result.AddSuccess(TestName);
                    }
                }
                catch (TargetInvocationException tie)
                {
                    Exception actualException = tie.InnerException ?? tie;
                    HandleTestException(actualException, expectFailure, result);
                }
                catch (Exception ex)
                {
                    HandleTestException(ex, expectFailure, result);
                }
            }
            finally
            {
                if (setUpSucceeded)
                {
                    try
                    {
                        TearDown();
                    }
                    catch (Exception ex)
                    {
                        result.AddError(TestName, new Exception($"TearDown failed: {ex.Message}", ex));
                    }
                }

                DoCleanups(result);

                stopwatch.Stop();
                result.AddTime(stopwatch.Elapsed.TotalSeconds);
                CurrentResult = null;
            }

            return result;
        }

        private void HandleTestException(Exception ex, bool expectFailure, TestResult result)
        {
            if (ex is SkipTestException skipEx)
            {
                result.AddSkip(TestName, skipEx.Message);
                return;
            }

            if (expectFailure)
            {
                result.AddExpectedFailure(TestName, ex);
                return;
            }

            if (ex is AssertionException assertionEx)
            {
                result.AddFailure(TestName, assertionEx);
            }
            else
            {
                result.AddError(TestName, ex);
            }
        }

        private bool EvaluateCondition(string memberName)
        {
            if (string.IsNullOrEmpty(memberName))
                return false;

            var prop = GetType().GetProperty(memberName, BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Static);
            if (prop != null && prop.PropertyType == typeof(bool))
                return (bool)prop.GetValue(prop.GetGetMethod().IsStatic ? null : this);

            var method = GetType().GetMethod(memberName, BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Static, null, Type.EmptyTypes, null);
            if (method != null && method.ReturnType == typeof(bool))
                return (bool)method.Invoke(method.IsStatic ? null : this, null);

            var field = GetType().GetField(memberName, BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Static);
            if (field != null && field.FieldType == typeof(bool))
                return (bool)field.GetValue(field.IsStatic ? null : this);

            return false;
        }
    }
}
