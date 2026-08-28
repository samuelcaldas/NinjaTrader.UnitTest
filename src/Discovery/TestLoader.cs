using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace NinjaTrader.UnitTest
{
    /// <summary>
    /// Loads and discovers test suites from TestCase classes, types, and assemblies.
    /// </summary>
    public class TestLoader
    {
        public static TestSuite LoadTestsFromTestCase<T>() where T : TestCase
        {
            return LoadTestsFromTestCase(typeof(T));
        }

        public static TestSuite LoadTestsFromTestCase(Type testClass)
        {
            if (testClass == null)
                throw new ArgumentNullException(nameof(testClass));

            if (!typeof(TestCase).IsAssignableFrom(testClass))
                throw new ArgumentException($"Type '{testClass.FullName}' must inherit from {nameof(TestCase)}", nameof(testClass));

            var suite = new TestSuite();
            var methods = GetTestMethods(testClass);

            foreach (var method in methods)
            {
                TestCase instance = CreateTestCaseInstance(testClass, method.Name);
                if (instance != null)
                {
                    suite.Add(instance);
                }
            }

            return suite;
        }

        public static TestSuite LoadTestsFromAssembly(Assembly assembly)
        {
            if (assembly == null)
                throw new ArgumentNullException(nameof(assembly));

            var suite = new TestSuite();
            var testClasses = assembly.GetTypes()
                .Where(t => t.IsClass && !t.IsAbstract && typeof(TestCase).IsAssignableFrom(t));

            foreach (var testClass in testClasses)
            {
                var classSuite = LoadTestsFromTestCase(testClass);
                suite.Add(classSuite);
            }

            return suite;
        }

        public static TestSuite LoadTestsFromNames(IEnumerable<string> names, Assembly searchAssembly = null)
        {
            var suite = new TestSuite();
            var assembly = searchAssembly ?? Assembly.GetCallingAssembly();

            foreach (var name in names)
            {
                AddNamedTestToSuite(name, assembly, suite);
            }

            return suite;
        }

        public static List<MethodInfo> GetTestMethods(Type testClass)
        {
            var methods = new List<MethodInfo>();
            var candidateMethods = testClass.GetMethods(BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly);

            foreach (var method in candidateMethods)
            {
                if (IsTestMethod(method))
                {
                    methods.Add(method);
                }
            }

            return methods;
        }

        #region Private Helpers

        private static bool IsTestMethod(MethodInfo method)
        {
            if (method.GetParameters().Length > 0)
                return false;

            if (method.GetCustomAttribute<TestAttribute>() != null)
                return true;

            string name = method.Name;
            if (name.Equals("SetUp", StringComparison.OrdinalIgnoreCase) ||
                name.Equals("TearDown", StringComparison.OrdinalIgnoreCase))
                return false;

            return name.StartsWith("Test", StringComparison.OrdinalIgnoreCase) ||
                   name.StartsWith("test_", StringComparison.OrdinalIgnoreCase);
        }

        private static void AddNamedTestToSuite(string name, Assembly assembly, TestSuite suite)
        {
            int lastDot = name.LastIndexOf('.');
            if (lastDot <= 0)
                return;

            string className = name.Substring(0, lastDot);
            string methodName = name.Substring(lastDot + 1);

            Type testType = assembly.GetType(className) ??
                            assembly.GetTypes().FirstOrDefault(t => t.Name == className && typeof(TestCase).IsAssignableFrom(t));

            if (testType == null || !typeof(TestCase).IsAssignableFrom(testType))
                return;

            var instance = CreateTestCaseInstance(testType, methodName);
            if (instance != null)
            {
                suite.Add(instance);
            }
        }

        private static TestCase CreateTestCaseInstance(Type testClass, string methodName)
        {
            var ctorWithName = testClass.GetConstructor(new[] { typeof(string) });
            if (ctorWithName != null)
            {
                return (TestCase)ctorWithName.Invoke(new object[] { methodName });
            }

            var defaultCtor = testClass.GetConstructor(Type.EmptyTypes);
            if (defaultCtor != null)
            {
                var instance = (TestCase)defaultCtor.Invoke(null);
                instance.MethodName = methodName;
                return instance;
            }

            throw new InvalidOperationException($"Class '{testClass.FullName}' must have either a constructor taking (string methodName) or a parameterless constructor.");
        }

        #endregion
    }
}
