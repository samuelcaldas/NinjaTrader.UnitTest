using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using NinjaTrader.UnitTest.Mocking;

namespace NinjaTrader.UnitTest
{
    /// <summary>
    /// Comprehensive self-test suite validating all capabilities of NinjaTrader.UnitTest.
    /// </summary>
    public class FrameworkSelfTests : TestCase
    {
        private static int _classSetUpCount = 0;
        private static int _classTearDownCount = 0;
        private bool _setUpExecuted = false;
        private bool _cleanupExecuted = false;

        public new static void SetUpClass()
        {
            _classSetUpCount++;
        }

        public new static void TearDownClass()
        {
            _classTearDownCount++;
        }

        public override void SetUp()
        {
            _setUpExecuted = true;
        }

        public override void TearDown()
        {
            _setUpExecuted = false;
        }

        #region Basic & Advanced Assertion Tests

        public void TestAssertEqualAndNotEqual()
        {
            AssertEqual(42, 42);
            AssertEqual("hello", "hello");
            AssertNotEqual(42, 99);
            AssertNotEqual("apple", "orange");

            // Aliases
            AreEqual(100, 100);
            AreNotEqual("foo", "bar");
        }

        public void TestAssertTrueAndFalse()
        {
            AssertTrue(1 + 1 == 2);
            AssertFalse(1 + 1 == 3);

            // Aliases
            IsTrue(true);
            IsFalse(false);
        }

        public void TestAssertIsAndIsNot()
        {
            var obj1 = new object();
            var obj2 = obj1;
            var obj3 = new object();

            AssertIs(obj1, obj2);
            AssertIsNot(obj1, obj3);

            // Aliases
            AreSame(obj1, obj2);
            AreNotSame(obj1, obj3);
        }

        public void TestAssertIsNoneAndIsNotNone()
        {
            string nullStr = null;
            string validStr = "ninja";

            AssertIsNone(nullStr);
            AssertIsNotNone(validStr);

            // Aliases
            IsNull(nullStr);
            IsNotNull(validStr);
        }

        public void TestAssertInAndNotInWithVariousCollections()
        {
            // List<T>
            var list = new List<int> { 1, 2, 3, 5, 8 };
            AssertIn(3, list);
            AssertNotIn(4, list);

            // Array
            var array = new string[] { "ES", "NQ", "YM", "RTY" };
            AssertIn("ES", array);
            AssertNotIn("CL", array);

            // HashSet (verifies fix for previous IList cast bug)
            var hashSet = new HashSet<string> { "AAPL", "MSFT", "GOOG" };
            AssertIn("AAPL", hashSet);
            AssertNotIn("TSLA", hashSet);

            // LINQ IEnumerable
            var linqSeq = list.Where(x => x > 2);
            AssertIn(5, linqSeq);
            AssertNotIn(1, linqSeq);

            // Aliases
            Contains("MSFT", hashSet);
            DoesNotContain("AMZN", hashSet);
        }

        public void TestAssertRaises()
        {
            // Generic form
            AssertRaises<DivideByZeroException>(() =>
            {
                int a = 10;
                int b = 0;
                int c = a / b;
            });

            // Type parameter form
            AssertRaises(typeof(ArgumentNullException), () =>
            {
                throw new ArgumentNullException("testParam");
            });

            // Regex message verification
            AssertRaisesRegex<InvalidOperationException>(() =>
            {
                throw new InvalidOperationException("Invalid operation: Code 404");
            }, "Code 404");

            // Alias
            Throws<IndexOutOfRangeException>(() =>
            {
                var arr = new int[2];
                int x = arr[5];
            });
        }

        public void TestAssertAlmostEqual()
        {
            double price1 = 5000.25000001;
            double price2 = 5000.25000002;
            AssertAlmostEqual(price1, price2, places: 6);

            double actualDelta = 5000.25;
            double expectedDelta = 5000.28;
            AssertAlmostEqual(expectedDelta, actualDelta, delta: 0.05);

            AssertNotAlmostEqual(5000.25, 5010.50, delta: 1.0);

            // Alias
            AreAlmostEqual(100.001, 100.002, delta: 0.01);
        }

        public void TestNumericComparisons()
        {
            AssertGreater(10, 5);
            AssertGreaterEqual(10, 10);
            AssertLess(5, 10);
            AssertLessEqual(10, 10);

            // Aliases
            Greater(20.5, 10.2);
            GreaterOrEqual(20.5, 20.5);
            Less(10.2, 20.5);
            LessOrEqual(10.2, 10.2);
        }

        public void TestAssertRegex()
        {
            string orderText = "Order #12345 filled at 5025.50";
            AssertRegex(orderText, @"Order #\d+ filled");
            AssertNotRegex(orderText, @"Order #\d+ rejected");
        }

        public void TestSequenceAndCountEqual()
        {
            var seq1 = new List<int> { 1, 2, 3, 4 };
            var seq2 = new int[] { 1, 2, 3, 4 };
            var seq3 = new List<int> { 4, 3, 2, 1 };

            AssertSequenceEqual(seq1, seq2);
            AssertCountEqual(seq1, seq3);

            var emptyList = new List<string>();
            var populatedList = new List<string> { "data" };

            AssertEmpty(emptyList);
            AssertNotEmpty(populatedList);

            // Aliases
            IsEmpty(emptyList);
            IsNotEmpty(populatedList);
        }

        public void TestAssertIsInstance()
        {
            object text = "Sample Text";
            AssertIsInstance<string>(text);
            AssertIsInstance(text, typeof(string));
            AssertNotIsInstance<int>(text);

            // Aliases
            IsInstanceOfType(text, typeof(string));
            IsNotInstanceOfType(text, typeof(int));
        }

        #endregion

        #region Lifecycle & Fixtures Tests

        public void TestSetUpExecution()
        {
            AssertTrue(_setUpExecuted, "SetUp() was not executed before test method");
        }

        public void TestCleanupsExecution()
        {
            AddCleanup(() =>
            {
                _cleanupExecuted = true;
            });

            AssertFalse(_cleanupExecuted, "Cleanup should not execute during the test method");
        }

        #endregion

        #region SubTest Execution

        public void TestSubTestIsolation()
        {
            var numbers = new int[] { 2, 4, 6, 8 };

            foreach (var n in numbers)
            {
                SubTest($"Testing even number {n}", () =>
                {
                    AssertEqual(0, n % 2);
                });
            }
        }

        #endregion

        #region Error vs Failure Distinction

        public void TestErrorAndFailureClassification()
        {
            // Create mini inner test cases to test classification
            var failingTest = new InnerFailingTest("TestFail");
            var errorTest = new InnerErrorTest("TestError");
            var passingTest = new InnerPassingTest("TestPass");

            var result = new TestResult(verbose: false, output: new TextWriterOutput(new StringWriter()));
            failingTest.Run(result);
            errorTest.Run(result);
            passingTest.Run(result);

            AssertEqual(1, result.FailureCount, "Expected exactly 1 failure");
            AssertEqual(1, result.ErrorCount, "Expected exactly 1 error");
            AssertEqual(1, result.SuccessCount, "Expected exactly 1 success");
            AssertEqual(3, result.RunCount, "Expected exactly 3 run count");
            AssertFalse(result.WasSuccessful());
        }

        #endregion

        #region Skip and ExpectedFailure Tests

        public void TestDynamicSkip()
        {
            var skippedTest = new InnerSkippedTest("TestSkip");
            var result = new TestResult(verbose: false, output: new TextWriterOutput(new StringWriter()));
            skippedTest.Run(result);

            AssertEqual(1, result.SkipCount);
            AssertEqual(0, result.FailureCount);
            AssertEqual(0, result.ErrorCount);
        }

        public void TestExpectedFailureHandling()
        {
            var expectedFailTest = new InnerExpectedFailTest("TestExpectedToFail");
            var result = new TestResult(verbose: false, output: new TextWriterOutput(new StringWriter()));
            expectedFailTest.Run(result);

            AssertEqual(1, result.ExpectedFailureCount);
            AssertEqual(0, result.FailureCount);
            AssertEqual(0, result.ErrorCount);
            AssertTrue(result.WasSuccessful());
        }

        #endregion

        #region Mocking Kit Tests

        public void TestBarSeriesBuilderAndMockBars()
        {
            var series = new BarSeriesBuilder("ES 03-26")
                .AddBar(5000.0, 5010.0, 4995.0, 5005.0, 1000)
                .AddBar(5005.0, 5020.0, 5002.0, 5018.0, 1500)
                .AddBar(5018.0, 5025.0, 5010.0, 5022.0, 1200)
                .Build();

            AssertEqual(3, series.Count);
            AssertEqual(2, series.CurrentBar);

            // Test 0 barsAgo (current)
            AssertEqual(5022.0, series.Close(0));
            AssertEqual(5018.0, series.Open(0));
            AssertEqual(5025.0, series.High(0));
            AssertEqual(5010.0, series.Low(0));
            AssertEqual(1200, series.Volume(0));

            // Test 1 bar ago
            AssertEqual(5018.0, series.Close(1));
            AssertEqual(5005.0, series.Open(1));

            // Test 2 bars ago
            AssertEqual(5005.0, series.Close(2));
            AssertEqual(5000.0, series.Open(2));
        }

        public void TestMockInstrumentCalculations()
        {
            var es = MockInstrument.CreateFutures("ES", tickSize: 0.25, pointValue: 50.0);

            // Tick rounding
            AssertEqual(5000.25, es.RoundToTick(5000.20));
            AssertEqual(5000.50, es.RoundToTick(5000.40));

            // Tick distance
            AssertEqual(4.0, es.CalculateTicks(1.0)); // 1.0 point = 4 ticks on ES

            // PnL calculation: 2 contracts bought at 5000 and sold at 5010 = 10 pts * $50 * 2 = $1000
            double longPnL = es.CalculatePnL(5000.0, 5010.0, 2, isLong: true);
            AssertEqual(1000.0, longPnL);

            // Short PnL: 2 contracts shorted at 5010 and covered at 5000 = 10 pts * $50 * 2 = $1000
            double shortPnL = es.CalculatePnL(5010.0, 5000.0, 2, isLong: false);
            AssertEqual(1000.0, shortPnL);
        }

        public void TestMockAccountAndOrderFills()
        {
            var account = new MockAccount("TestAccount", 50000.0);
            var nq = MockInstrument.CreateFutures("NQ", tickSize: 0.25, pointValue: 20.0);

            var buyOrder = account.SubmitOrder(nq, MockOrderAction.Buy, MockOrderType.Market, 2);
            AssertEqual(MockOrderState.Submitted, buyOrder.State);

            // Fill the order at 18000.00
            account.FillOrder(buyOrder, 18000.0, 2);
            AssertTrue(buyOrder.IsFilled);

            var pos = account.GetPosition(nq);
            AssertEqual(2, pos.Quantity);
            AssertEqual(18000.0, pos.AveragePrice);
            AssertTrue(pos.IsLong);

            // Unrealized PnL at 18050.00 = 50 pts * $20 * 2 = $2000
            double unrealized = pos.GetUnrealizedPnL(18050.0);
            AssertEqual(2000.0, unrealized);

            // Sell 2 contracts at 18050.00 to close
            var sellOrder = account.SubmitOrder(nq, MockOrderAction.Sell, MockOrderType.Market, 2);
            account.FillOrder(sellOrder, 18050.0, 2);

            AssertEqual(0, pos.Quantity);
            AssertTrue(pos.IsFlat);
            AssertEqual(2000.0, pos.RealizedPnL);
            AssertEqual(52000.0, account.CashValue);
        }

        public void TestNinjaScriptTestHarnessExecution()
        {
            var bars = new BarSeriesBuilder("ES")
                .AddTrend(barCount: 5, startPrice: 5000, stepPerBar: 5)
                .Build();

            var harness = new NinjaScriptTestHarness(bars);
            var visitedStates = new List<MockState>();
            var processedBars = new List<int>();

            harness.OnStateChange(state => visitedStates.Add(state));
            harness.OnBarUpdate(barIndex => processedBars.Add(barIndex));

            harness.RunAllBars();

            AssertTrue(visitedStates.Contains(MockState.SetDefaults));
            AssertTrue(visitedStates.Contains(MockState.Configure));
            AssertTrue(visitedStates.Contains(MockState.DataLoaded));
            AssertTrue(visitedStates.Contains(MockState.Historical));
            AssertEqual(5, processedBars.Count);
            AssertEqual(4, harness.CurrentBar);
        }

        #endregion

        #region TestLoader Discovery Tests

        public void TestAutoDiscovery()
        {
            var suite = TestLoader.LoadTestsFromTestCase<FrameworkSelfTests>();
            AssertGreater(suite.CountTestCases(), 10, "TestLoader should discover all test methods");
        }

        #endregion
    }

    #region Helper Test Classes for Self-Tests

    internal class InnerPassingTest : TestCase
    {
        public InnerPassingTest(string name) : base(name) { }
        public void TestPass() => AssertTrue(true);
    }

    internal class InnerFailingTest : TestCase
    {
        public InnerFailingTest(string name) : base(name) { }
        public void TestFail() => Fail("Expected failure");
    }

    internal class InnerErrorTest : TestCase
    {
        public InnerErrorTest(string name) : base(name) { }
        public void TestError() => throw new InvalidOperationException("Unexpected runtime crash");
    }

    internal class InnerSkippedTest : TestCase
    {
        public InnerSkippedTest(string name) : base(name) { }
        public void TestSkip() => SkipTest("Skipping intentionally");
    }

    internal class InnerExpectedFailTest : TestCase
    {
        public InnerExpectedFailTest(string name) : base(name) { }

        [ExpectedFailure("This test is expected to fail")]
        public void TestExpectedToFail()
        {
            AssertEqual(1, 2);
        }
    }

    #endregion
}
