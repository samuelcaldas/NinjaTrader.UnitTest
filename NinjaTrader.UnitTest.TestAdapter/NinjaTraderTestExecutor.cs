using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using Microsoft.VisualStudio.TestPlatform.ObjectModel;
using Microsoft.VisualStudio.TestPlatform.ObjectModel.Adapter;
using Microsoft.VisualStudio.TestPlatform.ObjectModel.Logging;
using VSTestCase = Microsoft.VisualStudio.TestPlatform.ObjectModel.TestCase;
using VSTestResult = Microsoft.VisualStudio.TestPlatform.ObjectModel.TestResult;

namespace NinjaTrader.UnitTest.TestAdapter
{
    /// <summary>
    /// Test executor for Visual Studio Test Explorer, executing NinjaTrader.UnitTest test cases.
    /// </summary>
    [ExtensionUri(Constants.ExecutorUri)]
    public class NinjaTraderTestExecutor : ITestExecutor
    {
        private volatile bool _isCancelled;

        public void Cancel()
        {
            _isCancelled = true;
        }

        public void RunTests(IEnumerable<string> sources, IRunContext runContext, IFrameworkHandle frameworkHandle)
        {
            _isCancelled = false;

            if (sources == null || frameworkHandle == null)
                return;

            var allTests = new List<VSTestCase>();
            foreach (string source in sources)
            {
                if (_isCancelled)
                    break;

                var discovered = NinjaTraderTestDiscoverer.DiscoverTestsInSource(source, frameworkHandle, null);
                allTests.AddRange(discovered);
            }

            RunTests(allTests, runContext, frameworkHandle);
        }

        public void RunTests(IEnumerable<VSTestCase> tests, IRunContext runContext, IFrameworkHandle frameworkHandle)
        {
            _isCancelled = false;

            if (tests == null || frameworkHandle == null)
                return;

            // Group tests by source assembly
            var testsBySource = new Dictionary<string, List<VSTestCase>>();
            foreach (var test in tests)
            {
                if (!testsBySource.TryGetValue(test.Source, out var list))
                {
                    list = new List<VSTestCase>();
                    testsBySource[test.Source] = list;
                }
                list.Add(test);
            }

            foreach (var kvp in testsBySource)
            {
                if (_isCancelled)
                    break;

                ExecuteTestsInAssembly(kvp.Key, kvp.Value, frameworkHandle);
            }
        }

        private void ExecuteTestsInAssembly(string sourceAssemblyPath, List<VSTestCase> tests, IFrameworkHandle frameworkHandle)
        {
            ResolveEventHandler resolver = CreateAssemblyResolver(sourceAssemblyPath);
            AppDomain.CurrentDomain.AssemblyResolve += resolver;

            try
            {
                Assembly assembly = Assembly.LoadFrom(sourceAssemblyPath);

                foreach (var test in tests)
                {
                    if (_isCancelled)
                        break;

                    ExecuteSingleTest(assembly, test, frameworkHandle);
                }
            }
            catch (Exception ex)
            {
                frameworkHandle.SendMessage(TestMessageLevel.Error, $"[NinjaTrader.UnitTest.TestAdapter] Execution error in {sourceAssemblyPath}: {ex.Message}");
            }
            finally
            {
                AppDomain.CurrentDomain.AssemblyResolve -= resolver;
            }
        }

        private void ExecuteSingleTest(Assembly assembly, VSTestCase testCase, IFrameworkHandle frameworkHandle)
        {
            frameworkHandle.RecordStart(testCase);

            var vsResult = new VSTestResult(testCase)
            {
                StartTime = DateTimeOffset.Now
            };

            var stopwatch = Stopwatch.StartNew();

            try
            {
                string typeName = GetTypeNameFromFullyQualifiedName(testCase.FullyQualifiedName);
                string methodName = GetMethodNameFromFullyQualifiedName(testCase.FullyQualifiedName);

                Type testType = assembly.GetType(typeName);
                if (testType == null)
                {
                    vsResult.Outcome = TestOutcome.NotFound;
                    vsResult.ErrorMessage = $"Test class '{typeName}' could not be resolved.";
                }
                else
                {
                    RunTestTypeMethod(testType, methodName, vsResult);
                }
            }
            catch (Exception ex)
            {
                vsResult.Outcome = TestOutcome.Failed;
                vsResult.ErrorMessage = ex.Message;
                vsResult.ErrorStackTrace = ex.StackTrace;
            }
            finally
            {
                stopwatch.Stop();
                vsResult.EndTime = DateTimeOffset.Now;
                vsResult.Duration = stopwatch.Elapsed;

                frameworkHandle.RecordResult(vsResult);
                frameworkHandle.RecordEnd(testCase, vsResult.Outcome);
            }
        }

        private static void RunTestTypeMethod(Type testType, string methodName, VSTestResult vsResult)
        {
            using (var stringWriter = new StringWriter())
            {
                var customOutput = new TextWriterOutput(stringWriter);
                var testResult = new NinjaTrader.UnitTest.TestResult(verbosity: 2, output: customOutput);

                object instance = Activator.CreateInstance(testType, new object[] { methodName });
                if (instance is NinjaTrader.UnitTest.TestCase tc)
                {
                    tc.Run(testResult);
                    MapTestResultToVsResult(testResult, vsResult, stringWriter.ToString());
                }
                else
                {
                    vsResult.Outcome = TestOutcome.NotFound;
                    vsResult.ErrorMessage = $"Type {testType.FullName} does not inherit from NinjaTrader.UnitTest.TestCase.";
                }
            }
        }

        private static void MapTestResultToVsResult(
            NinjaTrader.UnitTest.TestResult testResult,
            VSTestResult vsResult,
            string logOutput)
        {
            if (!string.IsNullOrWhiteSpace(logOutput))
            {
                vsResult.Messages.Add(new TestResultMessage(TestResultMessage.StandardOutCategory, logOutput));
            }

            if (testResult.Failures.Count > 0)
            {
                vsResult.Outcome = TestOutcome.Failed;
                var (testName, ex) = testResult.Failures[0];
                vsResult.ErrorMessage = ex?.Message ?? "Assertion Failed";
                vsResult.ErrorStackTrace = ex?.StackTrace;
                return;
            }

            if (testResult.SubTestFailures.Count > 0)
            {
                vsResult.Outcome = TestOutcome.Failed;
                var (testName, subTest, ex) = testResult.SubTestFailures[0];
                vsResult.ErrorMessage = $"SubTest Failure in [{subTest?.Message}]: {ex?.Message}";
                vsResult.ErrorStackTrace = ex?.StackTrace;
                return;
            }

            if (testResult.Errors.Count > 0)
            {
                vsResult.Outcome = TestOutcome.Failed;
                var (testName, ex) = testResult.Errors[0];
                vsResult.ErrorMessage = $"Runtime Error: {ex?.Message}";
                vsResult.ErrorStackTrace = ex?.StackTrace;
                return;
            }

            if (testResult.UnexpectedSuccesses.Count > 0)
            {
                vsResult.Outcome = TestOutcome.Failed;
                vsResult.ErrorMessage = "Unexpected success for test marked with [ExpectedFailure]";
                return;
            }

            if (testResult.Skipped.Count > 0)
            {
                vsResult.Outcome = TestOutcome.Skipped;
                vsResult.ErrorMessage = testResult.Skipped[0].Reason;
                return;
            }

            vsResult.Outcome = TestOutcome.Passed;
        }

        private static string GetTypeNameFromFullyQualifiedName(string fqn)
        {
            int lastDot = fqn.LastIndexOf('.');
            return lastDot > 0 ? fqn.Substring(0, lastDot) : fqn;
        }

        private static string GetMethodNameFromFullyQualifiedName(string fqn)
        {
            int lastDot = fqn.LastIndexOf('.');
            return lastDot >= 0 ? fqn.Substring(lastDot + 1) : fqn;
        }

        private static ResolveEventHandler CreateAssemblyResolver(string sourceAssemblyPath)
        {
            string sourceDir = Path.GetDirectoryName(sourceAssemblyPath);
            return (sender, args) =>
            {
                var requestedName = new AssemblyName(args.Name).Name;

                string localPath = Path.Combine(sourceDir, requestedName + ".dll");
                if (File.Exists(localPath))
                    return Assembly.LoadFrom(localPath);

                string libPath = Path.Combine(sourceDir, "..", "..", "lib", requestedName + ".dll");
                if (File.Exists(libPath))
                    return Assembly.LoadFrom(libPath);

                return null;
            };
        }
    }
}
