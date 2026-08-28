using System;
using NinjaTrader.UnitTest;

namespace NinjaTrader.UnitTest.Tests.Assertions
{
    public class NumericAndRegexAssertionTests : TestCase
    {
        public void TestAssertRaises()
        {
            AssertRaises<DivideByZeroException>(() =>
            {
                int zero = 0;
                int result = 10 / zero;
            });

            AssertRaises(typeof(ArgumentNullException), () =>
            {
                throw new ArgumentNullException("testParam");
            });

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
    }
}
