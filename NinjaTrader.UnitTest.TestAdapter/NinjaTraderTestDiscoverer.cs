using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using Microsoft.VisualStudio.TestPlatform.ObjectModel;
using Microsoft.VisualStudio.TestPlatform.ObjectModel.Adapter;
using Microsoft.VisualStudio.TestPlatform.ObjectModel.Logging;
using VSTestCase = Microsoft.VisualStudio.TestPlatform.ObjectModel.TestCase;

namespace NinjaTrader.UnitTest.TestAdapter
{
    /// <summary>
    /// Test discoverer for Visual Studio Test Explorer, discovering NinjaTrader.UnitTest test cases.
    /// </summary>
    [FileExtension(".dll")]
    [FileExtension(".exe")]
    [DefaultExecutorUri(Constants.ExecutorUri)]
    public class NinjaTraderTestDiscoverer : ITestDiscoverer
    {
        public void DiscoverTests(IEnumerable<string> sources, IDiscoveryContext discoveryContext, IMessageLogger logger, ITestCaseDiscoverySink discoverySink)
        {
            if (sources == null || discoverySink == null)
                return;

            foreach (string source in sources)
            {
                DiscoverTestsInSource(source, logger, discoverySink);
            }
        }

        public static List<VSTestCase> DiscoverTestsInSource(string source, IMessageLogger logger = null, ITestCaseDiscoverySink discoverySink = null)
        {
            var discoveredTests = new List<VSTestCase>();

            if (string.IsNullOrEmpty(source) || !File.Exists(source))
                return discoveredTests;

            ResolveEventHandler resolver = CreateAssemblyResolver(source);
            AppDomain.CurrentDomain.AssemblyResolve += resolver;

            try
            {
                Assembly assembly = Assembly.LoadFrom(source);
                Type[] types = GetExportedOrAllTypes(assembly);

                foreach (Type type in types)
                {
                    if (!IsTestCaseClass(type))
                        continue;

                    MethodInfo[] methods = type.GetMethods(BindingFlags.Public | BindingFlags.Instance);
                    foreach (MethodInfo method in methods)
                    {
                        if (!IsTestMethod(method))
                            continue;

                        string fullyQualifiedName = $"{type.FullName}.{method.Name}";
                        var testCase = new VSTestCase(fullyQualifiedName, new Uri(Constants.ExecutorUri), source)
                        {
                            DisplayName = $"{type.Name}.{method.Name}"
                        };

                        var (filePath, lineNumber) = SourceCodeNavigator.GetSourceLocation(source, type.FullName, method.Name);
                        if (!string.IsNullOrEmpty(filePath))
                        {
                            testCase.CodeFilePath = filePath;
                            testCase.LineNumber = lineNumber;
                        }

                        discoveredTests.Add(testCase);
                        discoverySink?.SendTestCase(testCase);
                    }
                }
            }
            catch (Exception ex)
            {
                logger?.SendMessage(TestMessageLevel.Warning, $"[NinjaTrader.UnitTest.TestAdapter] Error inspecting {source}: {ex.Message}");
            }
            finally
            {
                AppDomain.CurrentDomain.AssemblyResolve -= resolver;
            }

            return discoveredTests;
        }

        private static bool IsTestCaseClass(Type type)
        {
            if (type == null || !type.IsClass || type.IsAbstract)
                return false;

            Type current = type;
            while (current != null && current != typeof(object))
            {
                if (current.FullName == "NinjaTrader.UnitTest.TestCase" || current.Name == "TestCase")
                    return true;

                current = current.BaseType;
            }

            return false;
        }

        private static bool IsTestMethod(MethodInfo method)
        {
            if (method.IsSpecialName || method.GetParameters().Length > 0)
                return false;

            if (method.Name.StartsWith("Test", StringComparison.OrdinalIgnoreCase))
                return true;

            foreach (var attr in method.GetCustomAttributes(true))
            {
                string attrName = attr.GetType().Name;
                if (attrName == "TestAttribute" || attrName == "TestMethodAttribute")
                    return true;
            }

            return false;
        }

        private static Type[] GetExportedOrAllTypes(Assembly assembly)
        {
            try
            {
                return assembly.GetTypes();
            }
            catch (ReflectionTypeLoadException ex)
            {
                var list = new List<Type>();
                foreach (var t in ex.Types)
                {
                    if (t != null)
                        list.Add(t);
                }
                return list.ToArray();
            }
        }

        private static ResolveEventHandler CreateAssemblyResolver(string sourceAssemblyPath)
        {
            string sourceDir = Path.GetDirectoryName(sourceAssemblyPath);
            return (sender, args) =>
            {
                var requestedName = new AssemblyName(args.Name).Name;

                // Check same directory
                string localPath = Path.Combine(sourceDir, requestedName + ".dll");
                if (File.Exists(localPath))
                    return Assembly.LoadFrom(localPath);

                // Check lib directory if in project structure
                string libPath = Path.Combine(sourceDir, "..", "..", "lib", requestedName + ".dll");
                if (File.Exists(libPath))
                    return Assembly.LoadFrom(libPath);

                return null;
            };
        }
    }
}
