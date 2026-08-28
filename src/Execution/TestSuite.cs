using System;
using System.Collections.Generic;
using System.Reflection;

namespace NinjaTrader.UnitTest
{
    /// <summary>
    /// Aggregates test cases and nested suites to execute them sequentially.
    /// Manages class-level SetUpClass and TearDownClass fixtures.
    /// </summary>
    public class TestSuite
    {
        private readonly List<object> _tests = new List<object>();

        public int CountTestCases()
        {
            int count = 0;
            foreach (var test in _tests)
            {
                if (test is TestCase)
                    count++;
                else if (test is TestSuite suite)
                    count += suite.CountTestCases();
            }
            return count;
        }

        public void Add(TestCase testCase)
        {
            if (testCase != null)
                _tests.Add(testCase);
        }

        public void Add(TestSuite testSuite)
        {
            if (testSuite != null)
                _tests.Add(testSuite);
        }

        public void AddTests(IEnumerable<TestCase> testCases)
        {
            if (testCases == null)
                return;

            foreach (var tc in testCases)
            {
                Add(tc);
            }
        }

        public TestResult Run(TestResult result = null)
        {
            result = result ?? new TestResult();

            Type currentClassType = null;
            bool currentClassSetUpSucceeded = true;

            foreach (var item in _tests)
            {
                if (result.ShouldStop)
                    break;

                if (item is TestSuite childSuite)
                {
                    currentClassType = TearDownPreviousClass(currentClassType, result);
                    childSuite.Run(result);
                    continue;
                }

                if (item is TestCase testCase)
                {
                    currentClassSetUpSucceeded = TransitionClassFixture(testCase.GetType(), ref currentClassType, currentClassSetUpSucceeded, result);
                    ExecuteTestCase(testCase, currentClassType, currentClassSetUpSucceeded, result);
                }
            }

            TearDownPreviousClass(currentClassType, result);
            return result;
        }

        #region Private Fixture Helpers

        private Type TearDownPreviousClass(Type currentClassType, TestResult result)
        {
            if (currentClassType != null)
            {
                InvokeClassFixture(currentClassType, "TearDownClass", typeof(TearDownClassAttribute), result);
            }
            return null;
        }

        private bool TransitionClassFixture(Type testType, ref Type currentClassType, bool currentSetUpSucceeded, TestResult result)
        {
            if (testType == currentClassType)
                return currentSetUpSucceeded;

            if (currentClassType != null)
            {
                InvokeClassFixture(currentClassType, "TearDownClass", typeof(TearDownClassAttribute), result);
            }

            currentClassType = testType;
            return InvokeClassFixture(currentClassType, "SetUpClass", typeof(SetUpClassAttribute), result);
        }

        private void ExecuteTestCase(TestCase testCase, Type classType, bool setUpSucceeded, TestResult result)
        {
            if (setUpSucceeded)
            {
                testCase.Run(result);
                return;
            }

            result.AddError(testCase.TestName, new Exception($"Skipped test execution because SetUpClass failed for {classType.Name}"));
        }

        private static bool InvokeClassFixture(Type classType, string standardMethodName, Type attributeType, TestResult result)
        {
            try
            {
                if (TryInvokeAttributeFixture(classType, attributeType))
                    return true;

                return TryInvokeStandardFixture(classType, standardMethodName);
            }
            catch (TargetInvocationException tie)
            {
                Exception ex = tie.InnerException ?? tie;
                result?.AddError($"{classType.Name}.{standardMethodName}", ex);
                return false;
            }
            catch (Exception ex)
            {
                result?.AddError($"{classType.Name}.{standardMethodName}", ex);
                return false;
            }
        }

        private static bool TryInvokeAttributeFixture(Type classType, Type attributeType)
        {
            foreach (var method in classType.GetMethods(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static))
            {
                if (method.GetCustomAttribute(attributeType) != null)
                {
                    method.Invoke(null, null);
                    return true;
                }
            }
            return false;
        }

        private static bool TryInvokeStandardFixture(Type classType, string standardMethodName)
        {
            var standardMethod = classType.GetMethod(standardMethodName, BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static, null, Type.EmptyTypes, null);
            if (standardMethod != null && standardMethod.DeclaringType == classType)
            {
                standardMethod.Invoke(null, null);
                return true;
            }
            return true;
        }

        #endregion
    }
}
