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
            if (testCases != null)
            {
                foreach (var tc in testCases)
                    Add(tc);
            }
        }

        public TestResult Run(TestResult result = null)
        {
            result = result ?? new TestResult();

            Type currentClassType = null;
            bool currentClassSetUpSucceeded = true;

            for (int i = 0; i < _tests.Count; i++)
            {
                if (result.ShouldStop)
                    break;

                var item = _tests[i];

                if (item is TestSuite childSuite)
                {
                    // If switching from a class, tear down previous class
                    if (currentClassType != null)
                    {
                        InvokeClassFixture(currentClassType, "TearDownClass", typeof(TearDownClassAttribute), result);
                        currentClassType = null;
                    }
                    childSuite.Run(result);
                    continue;
                }

                if (item is TestCase testCase)
                {
                    Type testType = testCase.GetType();

                    // Check if class changed
                    if (testType != currentClassType)
                    {
                        if (currentClassType != null)
                        {
                            InvokeClassFixture(currentClassType, "TearDownClass", typeof(TearDownClassAttribute), result);
                        }

                        currentClassType = testType;
                        currentClassSetUpSucceeded = InvokeClassFixture(currentClassType, "SetUpClass", typeof(SetUpClassAttribute), result);
                    }

                    if (currentClassSetUpSucceeded)
                    {
                        testCase.Run(result);
                    }
                    else
                    {
                        result.AddError(testCase.TestName, new Exception($"Skipped test execution because SetUpClass failed for {currentClassType.Name}"));
                    }
                }
            }

            // Tear down the last class fixture
            if (currentClassType != null)
            {
                InvokeClassFixture(currentClassType, "TearDownClass", typeof(TearDownClassAttribute), result);
            }

            return result;
        }

        private static bool InvokeClassFixture(Type classType, string standardMethodName, Type attributeType, TestResult result)
        {
            try
            {
                // Look for explicit attribute
                foreach (var method in classType.GetMethods(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static))
                {
                    if (method.GetCustomAttribute(attributeType) != null)
                    {
                        method.Invoke(null, null);
                        return true;
                    }
                }

                // Look for standard method name (SetUpClass / TearDownClass)
                var standardMethod = classType.GetMethod(standardMethodName, BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static, null, Type.EmptyTypes, null);
                if (standardMethod != null && standardMethod.DeclaringType == classType)
                {
                    standardMethod.Invoke(null, null);
                    return true;
                }
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

            return true;
        }
    }
}
